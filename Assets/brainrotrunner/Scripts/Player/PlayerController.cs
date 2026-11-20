using BrainrotRunner.Core;
using BrainrotRunner.InputLayer;
using UnityEngine;

namespace BrainrotRunner.Player
{

    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private SwipeDetector swipeDetector;

        private PlayerMotor _motor;
        private GameManager _game;

        private void Awake() => _motor = GetComponent<PlayerMotor>();

        private void OnEnable()
        {
            if (swipeDetector != null)
                swipeDetector.Swiped += OnSwiped;
        }

        private void Start()
        {
            _game = GameManager.Instance;
            if (_game != null)
            {
                _game.RunStarted += OnRunStarted;
                _game.RunEnded += OnRunEnded;
            }
        }

        private void OnDisable()
        {
            if (swipeDetector != null)
                swipeDetector.Swiped -= OnSwiped;

            if (_game != null)
            {
                _game.RunStarted -= OnRunStarted;
                _game.RunEnded -= OnRunEnded;
            }
        }

        private void OnRunStarted() => _motor.ResetState();

        private void OnRunEnded() => _motor.Kill();

        private void OnSwiped(SwipeDirection direction)
        {
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
