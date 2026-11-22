using BrainrotRunner.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainrotRunner.Player
{
    public sealed class PlayerMotor : MonoBehaviour
    {
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int SlideHash = Animator.StringToHash("Slide");

        [SerializeField] private GameTuning tuning;
        [SerializeField] private float groundY = 0f;
        [SerializeField] private float laneSwitchTime = 0.11f;
        [SerializeField] private float deepCrouchScaleY = 0.45f;
        [SerializeField] private float minJumpHeight = 3.6f;
        [SerializeField] private bool handleDirectInput = true;
        [SerializeField] private float laneStepCooldown = 0.08f;

        private Animator _animator;
        private CapsuleCollider _collider;
        private float _defaultColliderHeight = 2f;
        private Vector3 _defaultColliderCenter = Vector3.zero;

        private LaneTracker _lanes;

        private int _currentLaneIndex = 0; // -1 = ліва, 0 = центр, 1 = права
        private int _queuedLaneStep = 0;
        private float _lastLaneCommandTime = -10f;
        private float _laneFromX;
        private float _laneToX;
        private float _laneProgress = 1f;

        private float _runTime;
        private float _airHeight;
        private float _verticalVelocity;
        private float _slideTimeLeft;
        private float _heightScale = 1f;

        private Vector2 _pointerStartPos;
        private bool _pointerTracking;

        private float EffectiveJumpHeight => Mathf.Max(tuning.jumpHeight, minJumpHeight);
        private float EffectiveTimeToApex => Mathf.Max(tuning.timeToApex, 0.42f);
        private float EffectiveGravity => -2f * EffectiveJumpHeight / (EffectiveTimeToApex * EffectiveTimeToApex);
        private float EffectiveJumpVelocity => 2f * EffectiveJumpHeight / EffectiveTimeToApex;

        public LaneTracker Lanes => _lanes;
        public PlayerState State { get; private set; } = PlayerState.Running;
        public bool IsChangingLane => _laneProgress < 1f;
        public bool IsGrounded => _airHeight <= 0f;
        public bool IsSliding => State == PlayerState.Slide;
        public float ForwardSpeed { get; private set; }
        public float DistanceTravelled { get; private set; }
        public float CurrentHeight => _defaultColliderHeight * _heightScale;

        private void Awake()
        {
            RefreshAnimatorReference();
            _collider = GetComponent<CapsuleCollider>();
            if (_collider != null)
            {
                _defaultColliderHeight = _collider.height;
                _defaultColliderCenter = _collider.center;
            }

            _lanes = new LaneTracker(tuning);
            ResetState();
        }

        public void RefreshAnimatorReference()
        {
            Animator[] anims = GetComponentsInChildren<Animator>(true);
            _animator = null;
            foreach (var a in anims)
            {
                if (a != null && a.gameObject != this.gameObject && a.runtimeAnimatorController != null)
                {
                    _animator = a;
                    break;
                }
            }
            if (_animator == null && anims.Length > 0)
                _animator = anims[anims.Length - 1];
        }

        private void OnEnable()
        {
            RefreshAnimatorReference();
            if (State == PlayerState.Dead)
            {
                ResetState();
            }
        }

        private void Update()
        {
            if (State == PlayerState.Dead)
                return;

            if (_animator == null)
                RefreshAnimatorReference();

            if (handleDirectInput)
                ReadDirectInput();

            float dt = Time.deltaTime;
            _runTime += dt;

            UpdateForwardSpeed();
            UpdateLanePosition(dt);
            UpdateVertical(dt);
            UpdateSlide(dt);

            float visualScaleY = Mathf.Lerp(deepCrouchScaleY, 1f, (_heightScale - deepCrouchScaleY) / Mathf.Max(0.01f, 1f - deepCrouchScaleY));
            visualScaleY = Mathf.Clamp(visualScaleY, deepCrouchScaleY, 1f);
            transform.localScale = new Vector3(1f, visualScaleY, 1f);

            Vector3 position = transform.position;
            position.x = CurrentLaneX();
            position.y = groundY + _airHeight;
            position.z += ForwardSpeed * dt;
            transform.position = position;

            transform.localRotation = Quaternion.identity;

            if (_collider != null)
            {
                float newHeight = _defaultColliderHeight * _heightScale;
                _collider.height = newHeight;
                _collider.center = new Vector3(
                    _defaultColliderCenter.x,
                    _defaultColliderCenter.y - (_defaultColliderHeight - newHeight) * 0.5f,
                    _defaultColliderCenter.z);
            }

            DistanceTravelled = position.z;
        }

        private void ReadDirectInput()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
                    RequestLaneChange(-1);
                else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                    RequestLaneChange(1);

                if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                    RequestJump();

                if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                    RequestSlide();
            }

            if (Pointer.current != null)
            {
                if (Pointer.current.press.wasPressedThisFrame)
                {
                    _pointerStartPos = Pointer.current.position.ReadValue();
                    _pointerTracking = true;
                }
                else if (_pointerTracking && Pointer.current.press.isPressed)
                {
                    Vector2 delta = Pointer.current.position.ReadValue() - _pointerStartPos;
                    if (delta.magnitude >= 45f)
                    {
                        _pointerTracking = false;
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                            RequestLaneChange(delta.x < 0f ? -1 : 1);
                        else if (delta.y > 0f)
                            RequestJump();
                        else
                            RequestSlide();
                    }
                }
                else if (Pointer.current.press.wasReleasedThisFrame)
                {
                    _pointerTracking = false;
                }
            }
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
            {
                if (_queuedLaneStep != 0)
                {
                    int step = _queuedLaneStep;
                    _queuedLaneStep = 0;
                    ApplySingleLaneStep(step);
                }
                return;
            }

            float duration = Mathf.Max(0.05f, laneSwitchTime);
            _laneProgress = Mathf.Min(1f, _laneProgress + dt / duration);

            if (_laneProgress >= 1f && _queuedLaneStep != 0)
            {
                int step = _queuedLaneStep;
                _queuedLaneStep = 0;
                ApplySingleLaneStep(step);
            }
        }

        private float CurrentLaneX()
        {
            if (_laneProgress >= 1f)
                return _laneToX;

            return Mathf.Lerp(_laneFromX, _laneToX, _laneProgress);
        }

        private void UpdateVertical(float dt)
        {
            if (State != PlayerState.Jump)
                return;

            float gravity = _verticalVelocity > 0f
                ? EffectiveGravity
                : EffectiveGravity * Mathf.Min(tuning.fallGravityMultiplier, 1.45f);

            _verticalVelocity += gravity * dt;
            _airHeight += _verticalVelocity * dt;

            if (_airHeight > 0f)
                return;

            _airHeight = 0f;
            _verticalVelocity = 0f;

            if (_slideTimeLeft > 0f)
            {
                State = PlayerState.Slide;
                if (_animator != null && _animator.runtimeAnimatorController != null)
                {
                    _animator.ResetTrigger(JumpHash);
                    _animator.SetTrigger(SlideHash);
                }
            }
            else
            {
                State = PlayerState.Running;
                ResumeRunAnimation();
            }
        }

        private void UpdateSlide(float dt)
        {
            float targetScale = State == PlayerState.Slide ? deepCrouchScaleY : 1f;
            float speed = 1f / Mathf.Max(0.01f, tuning.slideTransitionTime * 0.6f);
            _heightScale = Mathf.MoveTowards(_heightScale, targetScale, dt * speed);

            if (State != PlayerState.Slide)
                return;

            _slideTimeLeft -= dt;
            if (_slideTimeLeft <= 0f)
            {
                _slideTimeLeft = 0f;
                State = PlayerState.Running;
                ResumeRunAnimation();
            }
        }

        private void ResumeRunAnimation()
        {
            HeroSwitcher hs = GetComponent<HeroSwitcher>();
            if (hs != null) hs.ForcePlayRunAnimation();
        }

        public void RequestLaneChange(int direction)
        {
            if (State == PlayerState.Dead || direction == 0)
                return;

            // Захист від подвійного спрацьовування за один кадр/клік (щоб завжди рухався строго на 1 одиницю)
            if (Time.unscaledTime - _lastLaneCommandTime < laneStepCooldown)
                return;

            _lastLaneCommandTime = Time.unscaledTime;
            int step = direction < 0 ? -1 : 1;

            if (IsChangingLane)
            {
                _queuedLaneStep = step;
                return;
            }

            ApplySingleLaneStep(step);
        }

        private void ApplySingleLaneStep(int step)
        {
            int targetLane = Mathf.Clamp(_currentLaneIndex + step, -1, 1);
            if (targetLane == _currentLaneIndex)
                return;

            _lanes?.TryShift(step);

            _laneFromX = CurrentLaneX();
            _currentLaneIndex = targetLane;
            _laneToX = _currentLaneIndex * tuning.laneWidth;
            _laneProgress = 0f;
        }

        public void RequestJump()
        {
            if (State == PlayerState.Dead || !IsGrounded)
                return;

            _slideTimeLeft = 0f;
            _heightScale = 1f;
            _verticalVelocity = EffectiveJumpVelocity;
            _airHeight = 0.0001f;
            State = PlayerState.Jump;

            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.ResetTrigger(SlideHash);
                _animator.SetTrigger(JumpHash);
            }
        }

        public void RequestSlide()
        {
            if (State == PlayerState.Dead)
                return;

            _slideTimeLeft = tuning.slideDuration;

            if (!IsGrounded)
            {
                _verticalVelocity = -Mathf.Abs(EffectiveJumpVelocity) * 2.2f;
                return;
            }

            State = PlayerState.Slide;

            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.ResetTrigger(JumpHash);
                _animator.SetTrigger(SlideHash);
            }
        }

        public void Kill()
        {
            if (State == PlayerState.Dead)
                return;

            HeroSelectUI ui = FindFirstObjectByType<HeroSelectUI>();
            if (ui != null && ui.IsInvulnerable())
                return;

            State = PlayerState.Dead;
            transform.localRotation = Quaternion.identity;

            if (ui != null)
            {
                ui.OnPlayerDied();
            }
            else
            {
                ResetState();
            }
        }

        public void ReviveInPlace()
        {
            RefreshAnimatorReference();
            _airHeight = 0f;
            _verticalVelocity = 0f;
            _slideTimeLeft = 0f;
            _heightScale = 1f;
            _queuedLaneStep = 0;
            State = PlayerState.Running;

            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            if (_collider != null)
            {
                _collider.height = _defaultColliderHeight;
                _collider.center = _defaultColliderCenter;
            }

            Vector3 pos = transform.position;
            pos.y = groundY;
            transform.position = pos;

            ResumeRunAnimation();
        }

        public void ResetState()
        {
            RefreshAnimatorReference();
            _lanes?.Reset();
            _currentLaneIndex = 0;
            _queuedLaneStep = 0;
            _lastLaneCommandTime = -10f;
            _pointerTracking = false;
            _runTime = 0f;
            _laneProgress = 1f;
            _laneFromX = 0f;
            _laneToX = 0f;
            _airHeight = 0f;
            _verticalVelocity = 0f;
            _slideTimeLeft = 0f;
            _heightScale = 1f;
            ForwardSpeed = tuning.startSpeed;
            DistanceTravelled = 0f;
            State = PlayerState.Running;

            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            if (_collider != null)
            {
                _collider.height = _defaultColliderHeight;
                _collider.center = _defaultColliderCenter;
            }

            transform.position = new Vector3(0f, groundY, 0f);
            ResumeRunAnimation();
        }
    }
}