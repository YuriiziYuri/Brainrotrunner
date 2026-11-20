using UnityEngine;

namespace BrainrotRunner.World
{
    [CreateAssetMenu(menuName = "Brainrot/Track Palette", fileName = "TrackPalette")]
    public sealed class TrackPalette : ScriptableObject
    {
        public Material road;
        public Material laneLine;
        public Material marker;
        public Material pillar;

        [Header("Obstacles")]
        public Material laneBlock;
        public Material jumpBlock;
        public Material slideBar;

        [Header("Collectibles")]
        public Material coin;

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
