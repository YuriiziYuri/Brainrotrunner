using System.Collections.Generic;
using UnityEngine;

namespace BrainrotRunner.World
{
    public sealed class Obstacle : MonoBehaviour
    {
        public ObstacleType Type { get; private set; }
        public int Lane { get; private set; }

        private Renderer _fallbackRenderer;
        private BoxCollider _collider;
        private Transform _visualRoot;

        private readonly Dictionary<int, Dictionary<ObstacleType, GameObject>> _biomeModels =
            new Dictionary<int, Dictionary<ObstacleType, GameObject>>();

        private void Awake() => Cache();

        private void Cache()
        {
            if (_fallbackRenderer == null)
                _fallbackRenderer = GetComponent<Renderer>();

            if (_collider == null)
                _collider = GetComponent<BoxCollider>();

            if (_collider != null)
            {
                _collider.enabled = true;
                _collider.isTrigger = true;
            }
        }

        public void PrewarmBiomes(IReadOnlyList<BiomeVisualSet> biomes)
        {
            Cache();
            if (biomes == null || biomes.Count == 0)
                return;

            if (_visualRoot == null)
            {
                GameObject root = new GameObject("VisualRoot");
                root.transform.SetParent(transform, false);
                _visualRoot = root.transform;
            }

            ObstacleType[] types =
            {
                ObstacleType.LaneBlock,
                ObstacleType.JumpBlock,
                ObstacleType.SlideBar
            };

            for (int i = 0; i < biomes.Count; i++)
            {
                var dict = new Dictionary<ObstacleType, GameObject>();
                foreach (ObstacleType t in types)
                {
                    GameObject prefab = biomes[i].ForObstacle(t);
                    if (prefab == null)
                        continue;

                    GameObject instance = Instantiate(prefab, _visualRoot, false);
                    instance.name = $"{biomes[i].biomeName}_{t}";
                    StripColliders(instance);
                    instance.SetActive(false);
                    dict[t] = instance;
                }
                _biomeModels[i] = dict;
            }
        }

        public void Configure(ObstacleType type, int lane, Vector3 localPosition,
            float laneWidth, Material fallbackMaterial, int biomeIndex)
        {
            Cache();

            Type = type;
            Lane = lane;

            Vector3 targetSize = SizeFor(type, laneWidth);
            float centerY = CenterYFor(type);

            transform.localPosition = new Vector3(localPosition.x, 0f, localPosition.z);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (_collider != null)
            {
                _collider.enabled = true;
                _collider.isTrigger = true;
                _collider.center = new Vector3(0f, centerY, 0f);
                _collider.size = new Vector3(targetSize.x * 0.72f, targetSize.y * 0.82f, targetSize.z * 0.72f);
            }

            GameObject activeModel = null;
            foreach (var kvp in _biomeModels)
            {
                foreach (var modelEntry in kvp.Value)
                {
                    bool shouldShow = kvp.Key == biomeIndex && modelEntry.Key == type;
                    modelEntry.Value.SetActive(shouldShow);
                    if (shouldShow)
                        activeModel = modelEntry.Value;
                }
            }

            if (activeModel != null && _visualRoot != null)
            {
                _visualRoot.localPosition = Vector3.zero;
                _visualRoot.localRotation = Quaternion.identity;
                _visualRoot.localScale = Vector3.one;

                FitModelPreserveOrientation(activeModel, targetSize, type);

                if (_fallbackRenderer != null)
                    _fallbackRenderer.enabled = false;
            }
            else
            {
                transform.localPosition = new Vector3(localPosition.x, centerY, localPosition.z);
                transform.localScale = targetSize;

                if (_collider != null)
                {
                    _collider.center = Vector3.zero;
                    _collider.size = new Vector3(0.72f, 0.82f, 0.72f);
                }

                if (_fallbackRenderer != null)
                {
                    _fallbackRenderer.enabled = true;
                    _fallbackRenderer.sharedMaterial = fallbackMaterial;
                }
            }

            gameObject.SetActive(true);
        }

        private static void FitModelPreserveOrientation(GameObject model, Vector3 targetSize, ObstacleType type)
        {
            model.transform.localPosition = Vector3.zero;
            model.transform.localScale = Vector3.one;

            if (!TryGetMeshBoundsInRoot(model.transform.parent, model, out Bounds b))
                return;

            float scaleX = targetSize.x / b.size.x;
            float scaleY = targetSize.y / b.size.y;
            float scaleZ = targetSize.z / b.size.z;
            float uniformScale = Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ));

            model.transform.localScale = Vector3.one * uniformScale;

            if (!TryGetMeshBoundsInRoot(model.transform.parent, model, out b))
                return;

            const float groundSnapOffset = -0.015f;
            model.transform.localPosition = new Vector3(
                -b.center.x,
                -b.min.y + groundSnapOffset,
                -b.center.z);
        }

        private static bool TryGetMeshBoundsInRoot(Transform root, GameObject target, out Bounds result)
        {
            result = new Bounds(Vector3.zero, Vector3.zero);
            MeshFilter[] filters = target.GetComponentsInChildren<MeshFilter>(false);
            Matrix4x4 worldToRoot = root.worldToLocalMatrix;
            bool found = false;

            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter mf = filters[i];
                if (mf.sharedMesh == null)
                    continue;

                if (mf.TryGetComponent<MeshRenderer>(out var mr) && !mr.enabled)
                    continue;

                Bounds mb = mf.sharedMesh.bounds;
                Matrix4x4 toRoot = worldToRoot * mf.transform.localToWorldMatrix;

                for (int x = 0; x <= 1; x++)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        for (int z = 0; z <= 1; z++)
                        {
                            Vector3 corner = new Vector3(
                                x == 0 ? mb.min.x : mb.max.x,
                                y == 0 ? mb.min.y : mb.max.y,
                                z == 0 ? mb.min.z : mb.max.z);
                            Vector3 p = toRoot.MultiplyPoint3x4(corner);

                            if (!found)
                            {
                                result = new Bounds(p, Vector3.zero);
                                found = true;
                            }
                            else
                            {
                                result.Encapsulate(p);
                            }
                        }
                    }
                }
            }

            return found && result.size.x > 0.0001f && result.size.y > 0.0001f && result.size.z > 0.0001f;
        }

        public void Hide() => gameObject.SetActive(false);

        public Bounds WorldBounds => _collider != null
            ? _collider.bounds
            : new Bounds(transform.position, transform.lossyScale);

        private static void StripColliders(GameObject root)
        {
            foreach (Collider col in root.GetComponentsInChildren<Collider>(true))
                Destroy(col);
        }

        private static Vector3 SizeFor(ObstacleType type, float laneWidth)
        {
            float width = laneWidth * 0.78f;

            switch (type)
            {
                case ObstacleType.LaneBlock: return new Vector3(width, 1.85f, 3.4f);
                case ObstacleType.JumpBlock: return new Vector3(width, 0.65f, 0.85f);
                default: return new Vector3(width, 2.4f, 0.85f);
            }
        }

        private static float CenterYFor(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.LaneBlock: return 0.925f;
                case ObstacleType.JumpBlock: return 0.325f;
                default: return 1.75f;
            }
        }
    }
}