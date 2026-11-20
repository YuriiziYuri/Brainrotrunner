using UnityEngine;

namespace BrainrotRunner.World
{

    public sealed class Obstacle : MonoBehaviour
    {
        public ObstacleType Type { get; private set; }
        public int Lane { get; private set; }

        private Renderer _renderer;
        private BoxCollider _collider;

        private void Awake() => Cache();

        private void Cache()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            if (_collider == null)
                _collider = GetComponent<BoxCollider>();
        }

        public void Configure(ObstacleType type, int lane, Vector3 localPosition,
            float laneWidth, Material material)
        {
            Cache();

            Type = type;
            Lane = lane;

            Vector3 size = SizeFor(type, laneWidth);
            float centerY = CenterYFor(type);

            transform.localPosition = new Vector3(localPosition.x, centerY, localPosition.z);
            transform.localScale = size;
            _renderer.sharedMaterial = material;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        public Bounds WorldBounds => _collider != null
            ? _collider.bounds
            : new Bounds(transform.position, transform.lossyScale);

        private static Vector3 SizeFor(ObstacleType type, float laneWidth)
        {
            float width = laneWidth * 0.8f;

            switch (type)
            {
                case ObstacleType.LaneBlock: return new Vector3(width, 2.4f, 0.8f);

                case ObstacleType.JumpBlock: return new Vector3(width, 0.7f, 0.8f);

                default: return new Vector3(width, 0.6f, 0.8f);
            }
        }

        private static float CenterYFor(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.LaneBlock: return 1.2f;
                case ObstacleType.JumpBlock: return 0.35f;
                default: return 1.7f;
            }
        }
    }
}
