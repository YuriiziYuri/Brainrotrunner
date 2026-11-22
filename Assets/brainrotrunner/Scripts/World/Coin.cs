using BrainrotRunner.Progression;
using UnityEngine;

namespace BrainrotRunner.World
{
    public sealed class Coin : MonoBehaviour
    {
        private Renderer _renderer;
        private Collider _collider;
        private bool _collected;

        public Bounds WorldBounds => _collider != null
            ? _collider.bounds
            : new Bounds(transform.position, Vector3.one * 0.8f);

        private void Awake() => Cache();

        private void Cache()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            if (_collider == null)
                _collider = GetComponent<Collider>();

            if (_collider != null)
                _collider.isTrigger = true;
        }

        public void Place(Vector3 localPosition, Material material)
        {
            Cache();
            _collected = false;
            transform.localPosition = localPosition;
            transform.localScale = new Vector3(0.55f, 0.55f, 0.2f);

            if (_renderer != null && material != null)
                _renderer.sharedMaterial = material;

            if (_collider != null)
                _collider.enabled = true;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _collected = true;
            gameObject.SetActive(false);
        }

        public void Collect()
        {
            if (_collected || !gameObject.activeInHierarchy)
                return;

            _collected = true;
            CoinSystem.Instance.Add();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_collected)
                transform.Rotate(0f, 180f * Time.deltaTime, 0f, Space.Self);
        }
    }
}