using System;
using UnityEngine;

namespace BrainrotRunner.World
{
    public sealed class BiomePortal : MonoBehaviour
    {
        public static event Action<int> PortalEntered;

        private int _nextBiomeIndex;
        private bool _triggered;
        private BoxCollider _collider;

        public void Setup(float roadWidth, int nextBiomeIndex, Material portalMaterial = null)
        {
            _nextBiomeIndex = nextBiomeIndex;
            _triggered = false;

            if (_collider == null)
                _collider = GetComponent<BoxCollider>();

            if (_collider == null)
                _collider = gameObject.AddComponent<BoxCollider>();

            _collider.isTrigger = true;
            _collider.enabled = true;

            transform.localScale = new Vector3(roadWidth, 3.2f, 0.5f);

            // Вимикаємо рендерер, щоб не було жовтої стіни попереду, але тригер переходу біому працював
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = false;

            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        public void TriggerPortal()
        {
            if (_triggered)
                return;

            _triggered = true;
            PortalEntered?.Invoke(_nextBiomeIndex);
        }
    }
}