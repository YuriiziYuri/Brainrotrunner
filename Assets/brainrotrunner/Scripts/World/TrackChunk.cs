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
        private const int ObstaclePoolSize = 12;
        private const int CoinPoolSize = 30;

        private GameTuning _tuning;
        private TrackPalette _palette;
        private readonly List<Obstacle> _obstacles = new List<Obstacle>();
        private readonly List<Coin> _coins = new List<Coin>();

        public float Length { get; private set; }
        public float StartZ => transform.position.z;
        public float EndZ => StartZ + Length;

        public IReadOnlyList<Obstacle> Obstacles => _obstacles;

        public void Build(GameTuning tuning, TrackPalette palette)
        {
            _tuning = tuning;
            _palette = palette;
            Length = tuning.chunkLength;

            float roadWidth = tuning.laneWidth * 3f + 1f;

            CreateBox("RoadSlab", _palette.road,
                new Vector3(0f, -0.1f, Length * 0.5f),
                new Vector3(roadWidth, 0.2f, Length));

            for (int side = -1; side <= 1; side += 2)
            {
                CreateBox("LaneLine", _palette.laneLine,
                    new Vector3(side * tuning.laneWidth * 0.5f, 0.005f, Length * 0.5f),
                    new Vector3(0.08f, 0.02f, Length));
            }

            for (float z = MarkerStep; z < Length; z += MarkerStep)
            {
                CreateBox("Marker", _palette.marker,
                    new Vector3(0f, 0.01f, z), new Vector3(roadWidth, 0.02f, 0.25f));
            }

            for (float z = 0f; z < Length; z += PillarStep)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    CreateBox("Pillar", _palette.pillar,
                        new Vector3(side * (roadWidth * 0.5f + 0.8f), 1f, z),
                        new Vector3(0.5f, 2f, 0.5f));
                }
            }

            for (int i = 0; i < ObstaclePoolSize; i++)
                _obstacles.Add(CreateObstacle());

            for (int i = 0; i < CoinPoolSize; i++)
                _coins.Add(CreateCoin());
        }

        public void Populate(System.Random random, bool allowObstacles)
        {
            foreach (Obstacle obstacle in _obstacles)
                obstacle.Hide();

            foreach (Coin coin in _coins)
                coin.Hide();

            if (!allowObstacles)
                return;

            int used = 0;
            int coinsUsed = 0;
            float z = NextSpacing(random);

            while (z < Length - 4f && used < _obstacles.Count - 1)
            {
                bool blockTwo = random.NextDouble() < _tuning.doubleBlockChance;
                int freeLane = random.Next(LaneTracker.LaneCount);

                int firstOther = freeLane == 0 ? 1 : 0;
                int secondOther = freeLane == 2 ? 1 : 2;

                PlaceObstacle(ref used, blockTwo || random.NextDouble() < 0.5 ? firstOther : secondOther,
                    z, random);

                if (blockTwo)
                    PlaceObstacle(ref used, secondOther, z, random);

                PlaceCoinTrail(ref coinsUsed, freeLane, z);

                z += NextSpacing(random);
            }
        }

        private void PlaceObstacle(ref int used, int lane, float z, System.Random random)
        {
            if (used >= _obstacles.Count)
                return;

            ObstacleType type = RandomType(random);
            float x = (lane - LaneTracker.CenterLane) * _tuning.laneWidth;

            _obstacles[used].Configure(type, lane, new Vector3(x, 0f, z),
                _tuning.laneWidth, _palette.ForObstacle(type));

            used++;
        }

        private void PlaceCoinTrail(ref int used, int lane, float rowZ)
        {
            float x = (lane - LaneTracker.CenterLane) * _tuning.laneWidth;
            float startZ = rowZ - (_tuning.coinsPerRow - 1) * _tuning.coinSpacing * 0.5f;

            for (int i = 0; i < _tuning.coinsPerRow && used < _coins.Count; i++)
            {
                float z = startZ + i * _tuning.coinSpacing;
                if (z < 1f || z > Length - 1f)
                    continue;

                _coins[used].Place(new Vector3(x, 0.7f, z), _palette.coin);
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

        private void CreateBox(string boxName, Material material, Vector3 position, Vector3 scale)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = boxName;
            box.transform.SetParent(transform, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(box.GetComponent<Collider>());
        }
    }
}
