using UnityEngine;

namespace BrainrotRunner.UI
{

    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;
        private ScreenOrientation _appliedOrientation;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (_applied == Screen.safeArea && _appliedOrientation == Screen.orientation)
                return;

            Apply();
        }

        private void Apply()
        {
            Rect safeArea = Screen.safeArea;
            _applied = safeArea;
            _appliedOrientation = Screen.orientation;

            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            Vector2 min = safeArea.position;
            Vector2 max = safeArea.position + safeArea.size;

            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
