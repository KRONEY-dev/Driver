using UnityEngine;

namespace Driver.Gameplay.Level
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private float lookAheadZ;
        [SerializeField] private float lookHeight;
        [SerializeField] private float followSmoothTime;
        [SerializeField] private float rotationSharpness;
        [SerializeField, Range(0f, 1f)] private float lateralFollow;

        private float lateralVelocity;
        private float centerX;
        private bool frozen;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            centerX = target != null ? target.position.x : 0f;

            if (target != null)
                SnapToTarget();
        }

        public void Freeze()
        {
            frozen = true;
        }

        public void SnapToTarget()
        {
            frozen = false;

            if (target == null)
                return;

            transform.position = DesiredPosition();
            transform.rotation = Quaternion.LookRotation(LookPoint() - transform.position);

            lateralVelocity = 0f;
        }

        private void LateUpdate()
        {
            if (target == null || frozen)
                return;

            Vector3 desired = DesiredPosition();
            float x = Mathf.SmoothDamp(transform.position.x, desired.x, ref lateralVelocity, followSmoothTime);
            transform.position = new Vector3(x, desired.y, desired.z);

            Quaternion look = Quaternion.LookRotation(LookPoint() - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }

        private Vector3 DesiredPosition()
        {
            Vector3 position = target.position;
            float x = Mathf.Lerp(centerX, position.x, lateralFollow) + positionOffset.x;

            return new Vector3(x, position.y + positionOffset.y, position.z + positionOffset.z);
        }

        private Vector3 LookPoint()
        {
            Vector3 position = target.position;
            float x = Mathf.Lerp(centerX, position.x, lateralFollow);

            return new Vector3(x, position.y + lookHeight, position.z + lookAheadZ);
        }
    }
}