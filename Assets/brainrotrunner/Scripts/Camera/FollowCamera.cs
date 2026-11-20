using BrainrotRunner.Core;
using UnityEngine;

namespace BrainrotRunner.CameraLayer
{

    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private GameTuning tuning;
        [SerializeField] private Transform target;

        private Vector3 _velocity;
        private float _horizontalVelocity;
        private float _groundReferenceY;
        private bool _hasGroundReference;

        private void Start()
        {
            if (target == null)
                return;

            _groundReferenceY = target.position.y;
            _hasGroundReference = true;
            transform.position = DesiredPosition(target.position);
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 targetPosition = target.position;
            Vector3 desired = DesiredPosition(targetPosition);
            Vector3 current = transform.position;

            float x = Mathf.SmoothDamp(
                current.x, desired.x, ref _horizontalVelocity, tuning.cameraHorizontalSmoothTime);


            Vector3 verticalSource = new Vector3(desired.x, current.y, current.z);
            Vector3 smoothed = Vector3.SmoothDamp(
                verticalSource, desired, ref _velocity, tuning.cameraSmoothTime);

            transform.position = new Vector3(x, smoothed.y, smoothed.z);

            Vector3 lookAt = new Vector3(
                targetPosition.x,
                FollowedY(targetPosition.y),
                targetPosition.z + tuning.cameraLookAhead);
            transform.rotation = Quaternion.LookRotation(lookAt - transform.position, Vector3.up);
        }

        private Vector3 DesiredPosition(Vector3 targetPosition)
        {
            Vector3 anchored = new Vector3(
                targetPosition.x, FollowedY(targetPosition.y), targetPosition.z);

            return anchored + tuning.cameraOffset;
        }

        private float FollowedY(float targetY)
        {
            if (!_hasGroundReference)
                return targetY;

            return Mathf.Lerp(_groundReferenceY, targetY, tuning.cameraVerticalFollow);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}
