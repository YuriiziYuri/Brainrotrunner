using UnityEngine;

namespace BrainrotRunner.World
{
    public sealed class Coin : MonoBehaviour
    {
        public bool Collected { get; private set; }

        public void Place(Vector3 localPosition, Material material)
        {
            Collected = false;
            transform.localPosition = localPosition;
            transform.localScale = new Vector3(0.45f, 0.45f, 0.1f);
            transform.localRotation = Quaternion.identity;
            GetComponent<Renderer>().sharedMaterial = material;
            gameObject.SetActive(true);
        }

        public void Collect()
        {
            Collected = true;
            gameObject.SetActive(false);
        }

        public void Hide()
        {
            Collected = false;
            gameObject.SetActive(false);
        }
    }
}
