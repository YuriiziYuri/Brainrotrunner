using BrainrotRunner.Core;
using UnityEngine;

namespace BrainrotRunner.Player
{
    /// Авто-біг, зміна смуг, стрибок, ковзання
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private GameTuning tuning;
        [Tooltip("Висота центру персонажа над дорогою у звичайній позі.")]
        [SerializeField] private float groundY = 0.9f;

        private LaneTracker _lanes;

        private float _laneFromX;
        private float _laneToX;
        private float _laneProgress = 1f;

        private float _runTime;
        private float _airHeight;
        private float _verticalVelocity;
        private float _slideTimeLeft;
        private float _heightScale = 1f;

        public LaneTracker Lanes => _lanes;
        public PlayerState State { get; private set; } = PlayerState.Running;
        public bool IsChangingLane => _laneProgress < 1f;
        public bool IsGrounded => _airHeight <= 0f;
        public bool IsSliding => State == PlayerState.Slide;
        public float ForwardSpeed { get; private set; }
        public float DistanceTravelled { get; private set; }

        public float CurrentHeight => 2f * groundY * _heightScale;

        public Vector3 GroundAnchor => new Vector3(transform.position.x, groundY, transform.position.z);

        private void Awake()
        {
            _lanes = new LaneTracker(tuning);
            ResetState();
        }

        private void Update()
        {
            if (State == PlayerState.Dead)
                return;

            float dt = Time.deltaTime;
            _runTime += dt;

            UpdateForwardSpeed();
            UpdateLanePosition(dt);
            UpdateVertical(dt);
            UpdateSlide(dt);

            Vector3 position = transform.position;
            position.x = CurrentLaneX();
            position.y = groundY * _heightScale + _airHeight;
            position.z += ForwardSpeed * dt;
            transform.position = position;

        
            transform.localScale = new Vector3(1f, _heightScale, 1f);

            DistanceTravelled = position.z;
        }

        private void UpdateForwardSpeed()
        {
            ForwardSpeed = Mathf.Min(
                tuning.maxSpeed,
                tuning.startSpeed + tuning.speedRamp * _runTime);
        }

        private void UpdateLanePosition(float dt)
        {
            if (_laneProgress >= 1f)
                return;

            _laneProgress = Mathf.Min(1f, _laneProgress + dt / Mathf.Max(0.01f, tuning.laneChangeTime));
        }

        private float CurrentLaneX()
        {
            if (_laneProgress >= 1f)
                return _laneToX;

          
            return Mathf.Lerp(_laneFromX, _laneToX, Mathf.SmoothStep(0f, 1f, _laneProgress));
        }

        private void UpdateVertical(float dt)
        {
            if (State != PlayerState.Jump)
                return;

           
            float gravity = _verticalVelocity > 0f
                ? tuning.Gravity
                : tuning.Gravity * tuning.fallGravityMultiplier;

            _verticalVelocity += gravity * dt;
            _airHeight += _verticalVelocity * dt;

            if (_airHeight > 0f)
                return;

            _airHeight = 0f;
            _verticalVelocity = 0f;
            State = PlayerState.Running;
        }

        private void UpdateSlide(float dt)
        {
            float targetScale = State == PlayerState.Slide ? tuning.slideHeightScale : 1f;
            _heightScale = Mathf.MoveTowards(
                _heightScale, targetScale, dt / Mathf.Max(0.01f, tuning.slideTransitionTime));

            if (State != PlayerState.Slide)
                return;

            _slideTimeLeft -= dt;
            if (_slideTimeLeft <= 0f)
                State = PlayerState.Running;
        }

      
        public void RequestLaneChange(int direction)
        {
            if (State == PlayerState.Dead)
                return;

            if (IsChangingLane && !tuning.allowChainedLaneChange)
                return;

            if (!_lanes.TryShift(direction))
                return;

            _laneFromX = CurrentLaneX();
            _laneToX = _lanes.CurrentX;
            _laneProgress = 0f;
        }

     
        public void RequestJump()
        {
            if (State == PlayerState.Dead || !IsGrounded)
                return;

            _slideTimeLeft = 0f;
            _verticalVelocity = tuning.JumpVelocity;
            _airHeight = 0.0001f;
            State = PlayerState.Jump;
        }

       
        public void RequestSlide()
        {
            if (State == PlayerState.Dead || !IsGrounded)
                return;

            _slideTimeLeft = tuning.slideDuration;
            State = PlayerState.Slide;
        }

        public void Kill() => State = PlayerState.Dead;

        public void ResetState()
        {
            _lanes.Reset();
            _runTime = 0f;
            _laneProgress = 1f;
            _laneFromX = _lanes.CurrentX;
            _laneToX = _lanes.CurrentX;
            _airHeight = 0f;
            _verticalVelocity = 0f;
            _slideTimeLeft = 0f;
            _heightScale = 1f;
            ForwardSpeed = tuning.startSpeed;
            DistanceTravelled = 0f;
            State = PlayerState.Running;
            transform.localScale = Vector3.one;
            transform.position = new Vector3(_lanes.CurrentX, groundY, 0f);
        }
    }
}
