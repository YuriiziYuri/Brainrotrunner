using System;
using BrainrotRunner.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainrotRunner.InputLayer
{
    public sealed class SwipeDetector : MonoBehaviour
    {
        private const float FallbackDpi = 160f;

        [SerializeField] private GameTuning tuning;
        [SerializeField] private bool keyboardFallback = true;

        public event Action<SwipeDirection> Swiped;

        private Vector2 _startPosition;
        private float _startTime;
        private bool _tracking;
        private bool _consumed;

        public float ThresholdPixels
        {
            get
            {
                float dpi = Screen.dpi > 1f ? Screen.dpi : FallbackDpi;
                return tuning.swipeThresholdCm / 2.54f * dpi;
            }
        }

        private void Update()
        {
            ReadPointer();

            if (keyboardFallback)
                ReadKeyboard();
        }

        private void ReadPointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
                return;

            bool pressed = pointer.press.isPressed;

            if (pressed && !_tracking)
            {
                _tracking = true;
                _consumed = false;
                _startPosition = pointer.position.ReadValue();
                _startTime = Time.unscaledTime;
                return;
            }

            if (!pressed)
            {
                _tracking = false;
                return;
            }

            if (_consumed)
                return;

            if (Time.unscaledTime - _startTime > tuning.swipeMaxTime)
            {
                _consumed = true;
                return;
            }

            Vector2 delta = pointer.position.ReadValue() - _startPosition;
            float threshold = ThresholdPixels;

            if (delta.magnitude < threshold)
                return;

            _consumed = true;
            Swiped?.Invoke(Resolve(delta));
        }

        private static SwipeDirection Resolve(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                return delta.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;

            return delta.y > 0f ? SwipeDirection.Up : SwipeDirection.Down;
        }

        private void ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                Swiped?.Invoke(SwipeDirection.Left);
            else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                Swiped?.Invoke(SwipeDirection.Right);
            else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                Swiped?.Invoke(SwipeDirection.Up);
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                Swiped?.Invoke(SwipeDirection.Down);
        }
    }
}
