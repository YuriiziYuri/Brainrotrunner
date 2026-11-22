using System.Collections.Generic;
using BrainrotRunner.Core;
using BrainrotRunner.Player;
using UnityEngine;

namespace BrainrotRunner.World
{
    public sealed class TrackChunk : MonoBehaviour
    {
        private const float MarkerStep = 10f;
        private const float PillarStep = 6f;
        private const int ObstaclePoolSize = 18;
        private const int CoinPoolSize = 36;

        private GameTuning _tuning;
        private TrackPalette _palette;
        private readonly List<Obstacle> _obstacles = new List<Obstacle>();
        private readonly List<Coin> _coins = new List<Coin>();
        private BiomePortal _portal;
        private int _lastFreeLane = 1;

        private readonly List<GameObject> _fallbackVisuals = new List<GameObject>();
        private readonly List<GameObject> _fallbackRoadParts = new List<GameObject>();
        private readonly Dictionary<int, GameObject> _biomeRoots = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, bool> _biomeHasCustomRoad = new Dictionary<int, bool>();
        private readonly Dictionary<int, List<GameObject>> _biomeEnvProps = new Dictionary<int, List<GameObject>>();

        public float Length { get; private set; }
        public float StartZ => transform.position.z;
        public float EndZ => StartZ + Length;
        public int BiomeIndex { get; private set; }

        public IReadOnlyList<Obstacle> Obstacles => _obstacles;
        public IReadOnlyList<Coin> Coins => _coins;

        public void Build(GameTuning tuning, TrackPalette palette)
        {
            _tuning = tuning;
            _palette = palette;
            Length = tuning.chunkLength;

            BuildFallbackGeometry();

            if (_palette.HasBiomes)
                PrewarmBiomeGeometry();

            for (int i = 0; i < ObstaclePoolSize; i++)
            {
                Obstacle obs = CreateObstacle();
                if (_palette.HasBiomes)
                    obs.PrewarmBiomes(_palette.biomes);
                _obstacles.Add(obs);
            }

            for (int i = 0; i < CoinPoolSize; i++)
                _coins.Add(CreateCoin());

            GameObject portalObj = new GameObject("BiomePortal");
            portalObj.transform.SetParent(transform, false);
            _portal = portalObj.AddComponent<BiomePortal>();
            _portal.Hide();
        }

        private void BuildFallbackGeometry()
        {
            float roadWidth = _tuning.laneWidth * 3f + 0.6f;

            _fallbackRoadParts.Add(CreateBox("RoadSlab", _palette.road,
                new Vector3(0f, -0.1f, Length * 0.5f),
                new Vector3(roadWidth, 0.2f, Length)));

            for (int side = -1; side <= 1; side += 2)
            {
                _fallbackRoadParts.Add(CreateBox("LaneLine", _palette.laneLine,
                    new Vector3(side * _tuning.laneWidth * 0.5f, 0.005f, Length * 0.5f),
                    new Vector3(0.08f, 0.02f, Length)));
            }

            for (float z = MarkerStep; z < Length; z += MarkerStep)
            {
                _fallbackRoadParts.Add(CreateBox("Marker", _palette.marker,
                    new Vector3(0f, 0.01f, z), new Vector3(roadWidth, 0.02f, 0.25f)));
            }

            for (float z = 0f; z < Length; z += PillarStep)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    _fallbackVisuals.Add(CreateBox("Pillar", _palette.pillar,
                        new Vector3(side * (roadWidth * 0.5f + 0.8f), 1f, z),
                        new Vector3(0.5f, 2f, 0.5f)));
                }
            }
        }

        private void PrewarmBiomeGeometry()
        {
            float targetRoadWidth = _tuning.laneWidth * 3f + 0.6f;
            float actualRoadHalfWidth = targetRoadWidth * 0.5f;

            for (int b = 0; b < _palette.biomes.Count; b++)
            {
                BiomeVisualSet biome = _palette.biomes[b];
                GameObject biomeRoot = new GameObject($"Biome_{biome.biomeName}");
                biomeRoot.transform.SetParent(transform, false);
                biomeRoot.transform.localPosition = Vector3.zero;
                biomeRoot.transform.localRotation = Quaternion.identity;

                bool hasRoadPrefab = biome.roadPrefab != null;
                _biomeHasCustomRoad[b] = hasRoadPrefab;

                if (hasRoadPrefab)
                {
                    // Перевіряємо орієнтацію префабу дороги (як у City_Road, який у прев'ю стоїть вертикально)
                    GameObject probeHolder = new GameObject("RoadProbe");
                    GameObject probeModel = Instantiate(biome.roadPrefab, probeHolder.transform, false);
                    Bounds rawBounds = GetLocalBounds(probeHolder);

                    Quaternion baseRot = Quaternion.identity;
                    if (rawBounds.size.y > rawBounds.size.z * 1.2f && rawBounds.size.y > rawBounds.size.x * 0.8f)
                    {
                        baseRot = Quaternion.Euler(-90f, 0f, 0f);
                        probeModel.transform.localRotation = baseRot * probeModel.transform.localRotation;
                        rawBounds = GetLocalBounds(probeHolder);
                    }
                    else if (rawBounds.size.x > rawBounds.size.z * 1.4f && rawBounds.size.y < rawBounds.size.x * 0.5f)
                    {
                        baseRot = Quaternion.Euler(0f, 90f, 0f);
                        probeModel.transform.localRotation = baseRot * probeModel.transform.localRotation;
                        rawBounds = GetLocalBounds(probeHolder);
                    }

                    float rawWidth = rawBounds.size.x > 0.05f ? rawBounds.size.x : targetRoadWidth;
                    float rawLen = rawBounds.size.z > 0.05f ? rawBounds.size.z : 10f;
                    DestroyImmediate(probeHolder);

                    float scaleX = targetRoadWidth / rawWidth;
                    int segCount = Mathf.Max(1, Mathf.RoundToInt(Length / rawLen));
                    float exactStepZ = Length / segCount;
                    float scaleZ = exactStepZ / rawLen;

                    for (int s = 0; s < segCount; s++)
                    {
                        float zCenter = (s + 0.5f) * exactStepZ;

                        GameObject segHolder = new GameObject($"RoadSeg_{s}");
                        segHolder.transform.SetParent(biomeRoot.transform, false);
                        segHolder.transform.localPosition = Vector3.zero;
                        segHolder.transform.localRotation = Quaternion.identity;

                        GameObject seg = Instantiate(biome.roadPrefab, segHolder.transform, false);
                        seg.name = "Model";
                        StripColliders(seg);

                        seg.transform.localRotation = baseRot * seg.transform.localRotation;

                        // Масштабуємо в просторі тримача, щоб X завжди був шириною, а Z — довжиною
                        segHolder.transform.localScale = new Vector3(scaleX, 1f, scaleZ);

                        Bounds bnd = GetLocalBounds(segHolder);
                        // Верхня поверхня дороги рівно на y = 0.0f під ногами гравця
                        segHolder.transform.localPosition = new Vector3(
                            -bnd.center.x,
                            -bnd.max.y,
                            zCenter - bnd.center.z);
                    }
                }

                // Тротуари з боків дороги (верхня грань на y = 0)
                float sidewalkWidth = 10f;
                for (int side = -1; side <= 1; side += 2)
                {
                    GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    sidewalk.name = $"Sidewalk_{side}";
                    sidewalk.transform.SetParent(biomeRoot.transform, false);
                    sidewalk.transform.localPosition = new Vector3(
                        side * (actualRoadHalfWidth + sidewalkWidth * 0.5f),
                        -0.1f,
                        Length * 0.5f);
                    sidewalk.transform.localScale = new Vector3(sidewalkWidth, 0.2f, Length);

                    Renderer sr = sidewalk.GetComponent<Renderer>();
                    if (sr != null)
                        sr.sharedMaterial = _palette.pillar != null ? _palette.pillar : _palette.road;

                    Destroy(sidewalk.GetComponent<Collider>());
                }

                // Ліхтарі та будівлі біома по обидва боки
                var envList = new List<GameObject>();
                if (biome.sideEnvPrefabs != null && biome.sideEnvPrefabs.Length > 0)
                {
                    var thinPrefabs = new List<GameObject>();
                    var bigPrefabs = new List<GameObject>();

                    foreach (GameObject p in biome.sideEnvPrefabs)
                    {
                        if (p == null) continue;
                        GameObject probe = Instantiate(p);
                        Bounds pb = GetLocalBounds(probe);
                        DestroyImmediate(probe);

                        float maxAxis = Mathf.Max(pb.size.x, Mathf.Max(pb.size.y, pb.size.z));
                        float minAxis = Mathf.Min(pb.size.x, Mathf.Min(pb.size.y, pb.size.z));
                        bool isPole = maxAxis > minAxis * 3.0f && Mathf.Min(pb.size.x, pb.size.z) < 1.5f;

                        if (isPole) thinPrefabs.Add(p);
                        else bigPrefabs.Add(p);
                    }

                    if (thinPrefabs.Count > 0)
                    {
                        int thinIdx = 0;
                        for (float z = 5f; z < Length; z += 10f)
                        {
                            for (int side = -1; side <= 1; side += 2)
                            {
                                GameObject prefab = thinPrefabs[thinIdx % thinPrefabs.Count];
                                thinIdx++;

                                GameObject prop = SpawnAlignedProp(
                                    prefab, biomeRoot.transform, $"Thin_{biome.biomeName}_{z:F0}_{side}",
                                    side, actualRoadHalfWidth + 0.35f, z, 4.6f, 1.4f, true);

                                envList.Add(prop);
                            }
                        }
                    }

                    if (bigPrefabs.Count > 0)
                    {
                        int bigIdx = 0;
                        float stepZ = 12f;
                        for (float z = 6f; z < Length; z += stepZ)
                        {
                            for (int side = -1; side <= 1; side += 2)
                            {
                                GameObject prefab = bigPrefabs[bigIdx % bigPrefabs.Count];
                                bigIdx++;

                                float zPos = Mathf.Clamp(z + (side > 0 ? stepZ * 0.4f : 0f), 3.5f, Length - 3.5f);
                                float offsetFromCurb = thinPrefabs.Count > 0 ? 1.55f : 0.45f;

                                GameObject prop = SpawnAlignedProp(
                                    prefab, biomeRoot.transform, $"Big_{biome.biomeName}_{z:F0}_{side}",
                                    side, actualRoadHalfWidth + offsetFromCurb, zPos, 3.6f, 5.2f, false);

                                envList.Add(prop);
                            }
                        }
                    }
                }

                biomeRoot.SetActive(false);
                _biomeRoots[b] = biomeRoot;
                _biomeEnvProps[b] = envList;
            }
        }

        private static GameObject SpawnAlignedProp(
            GameObject prefab, Transform parent, string objName,
            int side, float innerEdgeX, float zPos, float targetHeight, float maxFootprint, bool isLampPost)
        {
            GameObject holder = new GameObject(objName);
            holder.transform.SetParent(parent, false);
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;
            holder.transform.localScale = Vector3.one;

            GameObject model = Instantiate(prefab, holder.transform, false);
            model.name = "Model";
            StripColliders(model);

            Bounds localBnd = GetLocalBounds(holder);

            if (isLampPost && localBnd.size.y < Mathf.Max(localBnd.size.x, localBnd.size.z) * 0.6f)
            {
                if (localBnd.size.z > localBnd.size.x)
                    model.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f) * model.transform.localRotation;
                else
                    model.transform.localRotation = Quaternion.Euler(0f, 0f, 90f) * model.transform.localRotation;

                localBnd = GetLocalBounds(holder);
            }

            float yaw = side < 0 ? 90f : -90f;
            holder.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            localBnd = GetLocalBounds(holder);
            if (localBnd.size.y > 0.001f)
            {
                float scale = targetHeight / localBnd.size.y;
                float maxHorizontal = Mathf.Max(localBnd.size.x, localBnd.size.z);
                if (!isLampPost && maxHorizontal * scale > maxFootprint)
                    scale = maxFootprint / maxHorizontal;

                model.transform.localScale *= scale;
                localBnd = GetLocalBounds(holder);
            }

            model.transform.localPosition += new Vector3(
                -localBnd.center.x,
                -localBnd.min.y,
                -localBnd.center.z);

            localBnd = GetLocalBounds(holder);
            float centerX = side * (innerEdgeX + localBnd.extents.x);
            holder.transform.localPosition = new Vector3(centerX, 0f, zPos);

            return holder;
        }

        private static Bounds GetLocalBounds(GameObject root)
        {
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(false);
            if (filters.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            Matrix4x4 rootWorldToLocal = root.transform.worldToLocalMatrix;
            bool hasBounds = false;
            Bounds combined = new Bounds(Vector3.zero, Vector3.zero);

            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter mf = filters[i];
                if (mf.sharedMesh == null)
                    continue;

                if (mf.TryGetComponent<MeshRenderer>(out var mr) && !mr.enabled)
                    continue;

                Bounds meshBnd = mf.sharedMesh.bounds;
                Matrix4x4 toRoot = rootWorldToLocal * mf.transform.localToWorldMatrix;
                EncapsulateTransformedBounds(ref combined, ref hasBounds, meshBnd, toRoot);
            }

            return combined;
        }

        private static void EncapsulateTransformedBounds(ref Bounds combined, ref bool hasBounds, Bounds b, Matrix4x4 matrix)
        {
            Vector3 min = b.min;
            Vector3 max = b.max;

            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    for (int z = 0; z <= 1; z++)
                    {
                        Vector3 corner = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        Vector3 localPoint = matrix.MultiplyPoint3x4(corner);

                        if (!hasBounds)
                        {
                            combined = new Bounds(localPoint, Vector3.zero);
                            hasBounds = true;
                        }
                        else
                        {
                            combined.Encapsulate(localPoint);
                        }
                    }
                }
            }
        }

        public void Populate(System.Random random, bool allowObstacles, int biomeIndex,
            bool hasPortalAtEnd = false, int nextBiomeIndex = 0)
        {
            BiomeIndex = biomeIndex;
            ApplyBiomeEnvironment(biomeIndex);

            foreach (Obstacle obstacle in _obstacles)
                obstacle.Hide();

            foreach (Coin coin in _coins)
                coin.Hide();

            if (_portal != null)
            {
                _portal.Hide();
                foreach (Renderer r in _portal.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;
            }

            if (!allowObstacles)
                _lastFreeLane = LaneTracker.CenterLane;

            int used = 0;
            int coinsUsed = 0;
            float maxZ = hasPortalAtEnd ? Length - 8f : Length - 4f;
            float step = Mathf.Max(8f, NextSpacing(random));
            float z = step * 0.5f;

            while (z < maxZ && used < _obstacles.Count - 2)
            {
                int shift = random.Next(-1, 2);
                int freeLane = Mathf.Clamp(_lastFreeLane + shift, 0, LaneTracker.LaneCount - 1);

                if (allowObstacles)
                {
                    bool blockTwo = random.NextDouble() < _tuning.doubleBlockChance;

                    if (blockTwo)
                    {
                        for (int lane = 0; lane < LaneTracker.LaneCount; lane++)
                        {
                            if (lane == freeLane)
                                continue;

                            PlaceObstacle(ref used, lane, z, random, biomeIndex);
                        }
                    }
                    else
                    {
                        int blockedLane = (freeLane + 1 + random.Next(2)) % LaneTracker.LaneCount;
                        PlaceObstacle(ref used, blockedLane, z, random, biomeIndex);
                    }
                }

                PlaceGuidedCoinTrail(ref coinsUsed, _lastFreeLane, freeLane, z, step);
                _lastFreeLane = freeLane;

                step = Mathf.Max(8f, NextSpacing(random));
                z += step;
            }
        }

        private void ApplyBiomeEnvironment(int biomeIndex)
        {
            bool useBiome = _palette.HasBiomes && _biomeRoots.ContainsKey(biomeIndex);
            bool hasCustomRoad = useBiome && _biomeHasCustomRoad.TryGetValue(biomeIndex, out bool hasR) && hasR;

            foreach (GameObject roadPart in _fallbackRoadParts)
            {
                if (roadPart != null)
                    roadPart.SetActive(!hasCustomRoad);
            }

            foreach (GameObject fb in _fallbackVisuals)
                fb.SetActive(!useBiome);

            foreach (var kvp in _biomeRoots)
            {
                bool isCurrent = useBiome && kvp.Key == biomeIndex;
                kvp.Value.SetActive(isCurrent);

                if (isCurrent && _biomeEnvProps.TryGetValue(kvp.Key, out var props))
                {
                    for (int i = 0; i < props.Count; i++)
                        props[i].SetActive(true);
                }
            }
        }

        private void PlaceObstacle(ref int used, int lane, float z, System.Random random, int biomeIndex)
        {
            if (used >= _obstacles.Count)
                return;

            ObstacleType type = RandomType(random);
            float x = (lane - LaneTracker.CenterLane) * _tuning.laneWidth;

            _obstacles[used].Configure(type, lane, new Vector3(x, 0f, z),
                _tuning.laneWidth, _palette.ForObstacle(type), biomeIndex);

            used++;
        }

        private void PlaceGuidedCoinTrail(ref int used, int fromLane, int toLane, float rowZ, float step)
        {
            float laneX = (toLane - LaneTracker.CenterLane) * _tuning.laneWidth;
            int count = Mathf.Max(3, _tuning.coinsPerRow);
            float span = Mathf.Min(step * 0.7f, (count - 1) * _tuning.coinSpacing);
            float startZ = rowZ - span * 0.5f;

            for (int i = 0; i < count && used < _coins.Count; i++)
            {
                float z = startZ + i * (count > 1 ? span / (count - 1) : 0f);
                if (z < 1f || z > Length - 1f)
                    continue;

                _coins[used].Place(new Vector3(laneX, 0.7f, z), _palette.coin);
                used++;
            }
        }

        private Coin CreateCoin()
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Coin";
            box.transform.SetParent(transform, false);
            box.GetComponent<BoxCollider>().isTrigger = true;
            Coin coin = box.AddComponent<Coin>();
            coin.Hide();
            return coin;
        }

        private float NextSpacing(System.Random random) =>
            Mathf.Lerp(_tuning.rowSpacingMin, _tuning.rowSpacingMax, (float)random.NextDouble());

        private static ObstacleType RandomType(System.Random random)
        {
            int roll = random.Next(3);
            return roll == 0 ? ObstacleType.LaneBlock
                : roll == 1 ? ObstacleType.JumpBlock
                : ObstacleType.SlideBar;
        }

        public void MoveTo(float startZ) =>
            transform.position = new Vector3(0f, 0f, startZ);

        private Obstacle CreateObstacle()
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Obstacle";
            box.transform.SetParent(transform, false);
            Obstacle obstacle = box.AddComponent<Obstacle>();
            obstacle.Hide();
            return obstacle;
        }

        private GameObject CreateBox(string boxName, Material material, Vector3 position, Vector3 scale)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = boxName;
            box.transform.SetParent(transform, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(box.GetComponent<Collider>());
            return box;
        }

        private static void StripColliders(GameObject root)
        {
            foreach (Collider col in root.GetComponentsInChildren<Collider>(true))
                Destroy(col);
        }
    }
}