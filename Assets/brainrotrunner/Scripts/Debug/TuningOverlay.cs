using BrainrotRunner.Core;
using BrainrotRunner.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace BrainrotRunner.DebugTools
{

    public sealed class TuningOverlay : MonoBehaviour
    {
        [SerializeField] private bool visible = true;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private InputLayer.SwipeDetector swipeDetector;
        [Tooltip("Діагностика рендеру: скільки об'єктів у сцені та скільки з них бачить камера.")]
        [SerializeField] private bool showRenderDiagnostics = true;

        private float _smoothedDeltaTime;
        private GUIStyle _style;
        private Renderer[] _renderers;
        private int _visibleRenderers;
        private float _nextScan;

        private void Awake()
        {

            if (!UnityEngine.Debug.isDebugBuild)
            {
                visible = false;
                showRenderDiagnostics = false;
            }
        }

        private void Update()
        {
            _smoothedDeltaTime = Mathf.Lerp(_smoothedDeltaTime, Time.unscaledDeltaTime, 0.1f);

            if (!showRenderDiagnostics || Time.unscaledTime < _nextScan)
                return;

            _nextScan = Time.unscaledTime + 1f;
            _renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            _visibleRenderers = 0;
            foreach (Renderer renderer in _renderers)
            {
                if (renderer.isVisible)
                    _visibleRenderers++;
            }
        }

        private void OnGUI()
        {
            if (!visible || motor == null)
                return;

            if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver)
                return;

            _style ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Screen.height * 0.028f),
                normal = { textColor = Color.white }
            };

            float fps = _smoothedDeltaTime > 0f ? 1f / _smoothedDeltaTime : 0f;
            GameManager game = GameManager.Instance;

            GUILayout.BeginArea(new Rect(Screen.width * 0.04f, Screen.height * 0.06f,
                Screen.width * 0.6f, Screen.height * 0.4f));
            GUILayout.Label($"FPS {fps:F0}", _style);
            GUILayout.Label($"Speed {motor.ForwardSpeed:F1}", _style);
            GUILayout.Label($"Lane {motor.Lanes.CurrentLane}{(motor.IsChangingLane ? " (moving)" : "")}", _style);
            GUILayout.Label($"Action {motor.State}{(motor.IsGrounded ? "" : " (air)")}", _style);
            GUILayout.Label($"Distance {motor.DistanceTravelled:F0} m", _style);
            if (swipeDetector != null)
                GUILayout.Label($"Swipe threshold {swipeDetector.ThresholdPixels:F0} px @ {Screen.dpi:F0} dpi", _style);
            if (game != null)
                GUILayout.Label($"State {game.State}", _style);

            if (showRenderDiagnostics)
            {
                int total = _renderers != null ? _renderers.Length : 0;
                GUILayout.Label($"Renderers {_visibleRenderers}/{total}", _style);

                Camera camera = Camera.main;
                if (camera == null)
                {
                    GUILayout.Label("Camera.main NULL", _style);
                }
                else
                {
                    Vector3 position = camera.transform.position;
                    Vector3 euler = camera.transform.eulerAngles;
                    GUILayout.Label($"Cam {position.x:F1},{position.y:F1},{position.z:F1}", _style);
                    GUILayout.Label($"Rot {euler.x:F0},{euler.y:F0},{euler.z:F0}", _style);
                }

                GUILayout.Label($"Player {motor.transform.position.x:F1}," +
                                $"{motor.transform.position.y:F1},{motor.transform.position.z:F1}", _style);

                RenderPipelineAsset pipeline =
                    QualitySettings.renderPipeline != null
                        ? QualitySettings.renderPipeline
                        : GraphicsSettings.defaultRenderPipeline;
                GUILayout.Label($"SRP {(pipeline != null ? pipeline.name : "BUILT-IN")}", _style);
            }

            GUILayout.EndArea();
        }
    }
}
