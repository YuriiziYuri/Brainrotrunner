using BrainrotRunner.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainrotRunner.UI
{

    public sealed class GameOverView : MonoBehaviour
    {
        [Tooltip("Скільки не приймати ввід після програшу, щоб той самий дотик не перезапустив забіг.")]
        [SerializeField] private float inputLockTime = 0.4f;

        private GameManager _game;
        private float _shownAt;
        private bool _wasGameOver;
        private GUIStyle _titleStyle;
        private GUIStyle _textStyle;

        private void Start() => _game = GameManager.Instance;

        private void Update()
        {
            if (_game == null)
                return;

            bool isGameOver = _game.State == GameState.GameOver;

            if (isGameOver && !_wasGameOver)
                _shownAt = Time.unscaledTime;

            _wasGameOver = isGameOver;

            if (!isGameOver || Time.unscaledTime - _shownAt < inputLockTime)
                return;

            if (PressedThisFrame())
                _game.Restart();
        }

        public static bool PressedThisFrame()
        {
            Pointer pointer = Pointer.current;
            if (pointer != null && (pointer.press.wasPressedThisFrame || pointer.press.wasReleasedThisFrame))
                return true;

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.tap.wasPressedThisFrame)
                return true;

            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        }

        private void OnGUI()
        {
            if (_game == null || _game.State != GameState.GameOver)
                return;

            EnsureStyles();

            float width = Screen.width * 0.8f;
            float height = Screen.height * 0.3f;
            Rect area = new Rect((Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f, width, height);

            GUI.Box(area, GUIContent.none);

            GUILayout.BeginArea(area);
            GUILayout.FlexibleSpace();
            GUILayout.Label("ЗАБІГ ЗАВЕРШЕНО", _titleStyle);
            GUILayout.Label(_game.LastFailReason, _textStyle);
            GUILayout.Label($"Дистанція: {_game.LastRunDistance:F0} м", _textStyle);
            GUILayout.Space(Screen.height * 0.02f);
            GUILayout.Label("Торкніться, щоб почати знову", _textStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
                return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Screen.height * 0.035f),
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            _textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Screen.height * 0.025f),
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };
        }
    }
}
