using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrainrotRunner.World
{
    [Serializable]
    public sealed class BiomeVisualSet
    {
        public string biomeName = "City";
        public Color skyColor = new Color(0.45f, 0.68f, 0.92f);

        [Header("Road Segment (10m)")]
        public GameObject roadPrefab;

        [Header("Obstacles (3 Types)")]
        public GameObject laneBlockPrefab;   // Обійти (Ambulance, Snowmobile, Jeep)
        public GameObject jumpBlockPrefab;   // Перестрибнути (Barrier, IceLog, BoulderLog)
        public GameObject slideBarPrefab;    // Підкотитися (SignGate, IceArch, WoodGate)

        [Header("Side Environment Props")]
        public GameObject[] sideEnvPrefabs;

        public GameObject ForObstacle(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.JumpBlock: return jumpBlockPrefab;
                case ObstacleType.SlideBar: return slideBarPrefab;
                default: return laneBlockPrefab;
            }
        }
    }

    [CreateAssetMenu(menuName = "Brainrot/Track Palette", fileName = "TrackPalette")]
    public sealed class TrackPalette : ScriptableObject
    {
        [Header("Fallback Greybox Materials")]
        public Material road;
        public Material laneLine;
        public Material marker;
        public Material pillar;

        [Header("Fallback Obstacles")]
        public Material laneBlock;
        public Material jumpBlock;
        public Material slideBar;

        [Header("Collectibles")]
        public Material coin;

        [Header("3D Biomes (3 по черзі: City -> Tundra -> Savanna)")]
        [Min(1)] public int chunksPerBiome = 3;
        public float biomeSwitchDistance = 180f;
        public List<BiomeVisualSet> biomes = new List<BiomeVisualSet>
        {
            new BiomeVisualSet { biomeName = "01_City", skyColor = new Color(0.45f, 0.68f, 0.92f) },
            new BiomeVisualSet { biomeName = "02_Tundra", skyColor = new Color(0.72f, 0.86f, 0.96f) },
            new BiomeVisualSet { biomeName = "03_Savanna", skyColor = new Color(0.95f, 0.72f, 0.42f) }
        };

        public bool HasBiomes => biomes != null && biomes.Count > 0;

        public Material ForObstacle(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.JumpBlock: return jumpBlock;
                case ObstacleType.SlideBar: return slideBar;
                default: return laneBlock;
            }
        }
    }
}