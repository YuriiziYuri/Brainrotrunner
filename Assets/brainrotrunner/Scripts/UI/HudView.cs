using BrainrotRunner.Core;
using BrainrotRunner.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BrainrotRunner.UI
{

    public sealed class HudView : MonoBehaviour
    {
        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.35f);

        private Text _scoreText;
        private Text _coinText;
        private Image _progressFill;
        private RectTransform _pauseButton;
        private GameObject _pausePanel;
        private bool _paused;

        private void Awake() => Build();

        private void Update()
        {
            GameManager game = GameManager.Instance;
            ScoreSystem score = ScoreSystem.Instance;
            CoinSystem coins = CoinSystem.Instance;

            if (score != null)
            {
                _scoreText.text = score.Score.ToString();
                _progressFill.fillAmount = score.Progress01;
            }

            if (coins != null)
                _coinText.text = "● " + coins.Collected;

            bool canPause = game != null && game.State == GameState.Running;
            _pauseButton.gameObject.SetActive(canPause || _paused);

            HandlePauseInput(canPause);
        }

        private void HandlePauseInput(bool canPause)
        {
            Pointer pointer = Pointer.current;
            if (pointer == null || !GameOverView.PressedThisFrame())
                return;

            Vector2 position = pointer.position.ReadValue();

            if (_paused)
            {
                SetPaused(false);
                return;
            }

            if (canPause && RectTransformUtility.RectangleContainsScreenPoint(_pauseButton, position, null))
                SetPaused(true);
        }

        private void SetPaused(bool paused)
        {
            _paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            _pausePanel.SetActive(paused);
        }

        private void OnDisable() => Time.timeScale = 1f;

        private void Build()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasObject = new GameObject("HudCanvas",
                typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2400f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform safeArea = CreateRect("SafeArea", canvasObject.transform);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            _scoreText = CreateText("Score", safeArea, font, 64, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f),
                new Vector2(400f, 90f));

            _coinText = CreateText("Coins", safeArea, font, 48, TextAnchor.UpperRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-180f, -40f),
                new Vector2(260f, 70f));
            _coinText.color = new Color(0.98f, 0.82f, 0.25f);

            RectTransform progressBack = CreateRect("ProgressBack", safeArea);
            progressBack.anchorMin = new Vector2(0f, 1f);
            progressBack.anchorMax = new Vector2(1f, 1f);
            progressBack.pivot = new Vector2(0.5f, 1f);
            progressBack.anchoredPosition = new Vector2(0f, -150f);
            progressBack.sizeDelta = new Vector2(-80f, 14f);
            Image backImage = progressBack.gameObject.AddComponent<Image>();
            backImage.color = Dim;

            RectTransform progressFill = CreateRect("ProgressFill", progressBack);
            progressFill.anchorMin = Vector2.zero;
            progressFill.anchorMax = Vector2.one;
            progressFill.offsetMin = Vector2.zero;
            progressFill.offsetMax = Vector2.zero;
            _progressFill = progressFill.gameObject.AddComponent<Image>();
            _progressFill.color = new Color(0.45f, 0.85f, 0.55f);
            _progressFill.type = Image.Type.Filled;
            _progressFill.fillMethod = Image.FillMethod.Horizontal;
            _progressFill.fillAmount = 0f;

            Text pauseLabel = CreateText("Pause", safeArea, font, 56, TextAnchor.MiddleCenter,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-70f, -45f),
                new Vector2(110f, 110f));
            pauseLabel.text = "II";
            _pauseButton = pauseLabel.rectTransform;

            _pausePanel = CreateRect("PausePanel", safeArea).gameObject;
            RectTransform pauseRect = _pausePanel.GetComponent<RectTransform>();
            pauseRect.anchorMin = Vector2.zero;
            pauseRect.anchorMax = Vector2.one;
            pauseRect.offsetMin = Vector2.zero;
            pauseRect.offsetMax = Vector2.zero;
            Image pauseBackground = _pausePanel.AddComponent<Image>();
            pauseBackground.color = new Color(0f, 0f, 0f, 0.6f);

            Text pausedText = CreateText("PausedText", pauseRect, font, 64, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(900f, 200f));
            pausedText.text = "ПАУЗА\nторкніться, щоб продовжити";

            _pausePanel.SetActive(false);
        }

        private static RectTransform CreateRect(string rectName, Transform parent)
        {
            GameObject item = new GameObject(rectName, typeof(RectTransform));
            item.transform.SetParent(parent, false);

            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static Text CreateText(string textName, Transform parent, Font font, int size,
            TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size2)
        {
            GameObject item = new GameObject(textName, typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);

            Text text = item.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(anchorMin.x, anchorMax.y);
            rect.anchoredPosition = position;
            rect.sizeDelta = size2;
            return text;
        }
    }
}
