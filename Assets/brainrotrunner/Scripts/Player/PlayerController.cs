using BrainrotRunner.Core;
using BrainrotRunner.InputLayer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainrotRunner.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private SwipeDetector swipeDetector;

        private PlayerMotor _motor;
        private GameManager _game;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            if (swipeDetector == null)
                swipeDetector = FindFirstObjectByType<SwipeDetector>();
        }

        private void OnEnable()
        {
            if (swipeDetector == null)
                swipeDetector = FindFirstObjectByType<SwipeDetector>();

            if (swipeDetector != null)
            {
                swipeDetector.Swiped -= OnSwiped;
                swipeDetector.Swiped += OnSwiped;
            }
        }

        private void Start()
        {
            _game = GameManager.Instance;
            if (_game != null)
            {
                _game.RunStarted -= OnRunStarted;
                _game.RunEnded -= OnRunEnded;
                _game.RunStarted += OnRunStarted;
                _game.RunEnded += OnRunEnded;
            }
        }

        private void OnDisable()
        {
            if (swipeDetector != null)
                swipeDetector.Swiped -= OnSwiped;
        }

        private void OnDestroy()
        {
            if (_game != null)
            {
                _game.RunStarted -= OnRunStarted;
                _game.RunEnded -= OnRunEnded;
            }
        }

        private void Update()
        {
            if (_game == null) _game = GameManager.Instance;
            if (_game != null && _game.State != GameState.Running)
                return;

            Keyboard kb = Keyboard.current;
            if (kb == null)
                return;

            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                _motor.RequestLaneChange(-1);
            else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                _motor.RequestLaneChange(+1);
            else if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                _motor.RequestJump();
            else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame || kb.leftCtrlKey.wasPressedThisFrame || kb.cKey.wasPressedThisFrame)
                _motor.RequestSlide();
        }

        private void OnRunStarted() => _motor.ResetState();

        private void OnRunEnded() => _motor.Kill();

        private void OnSwiped(SwipeDirection direction)
        {
            if (_game == null) _game = GameManager.Instance;
            if (_game != null && _game.State != GameState.Running)
                return;

            switch (direction)
            {
                case SwipeDirection.Left:
                    _motor.RequestLaneChange(-1);
                    break;
                case SwipeDirection.Right:
                    _motor.RequestLaneChange(+1);
                    break;
                case SwipeDirection.Up:
                    _motor.RequestJump();
                    break;
                case SwipeDirection.Down:
                    _motor.RequestSlide();
                    break;
            }
        }
    }
}