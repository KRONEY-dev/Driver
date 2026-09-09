using Driver.Data.Scriptable;
using UnityEngine;

namespace Driver.Gameplay.Characters.Components
{
    public class CarMovement : MonoBehaviour
    {
        public float DistanceTravelled => distanceTravelled;
        public bool IsMoving => isMoving;

        private CarCharacterConfig config;

        private Vector3 startPosition;
        private float distanceTravelled;
        private bool isMoving;

        private bool movingAway;
        private float moveAwayTime;
        private float moveAwayDuration;
        private float moveAwaySpeedMultiplier;

        public void Init(CarCharacterConfig carCharacterConfig)
        {
            config = carCharacterConfig;
            startPosition = transform.position;

            ResetToStart();
        }

        public void ResetToStart()
        {
            distanceTravelled = 0f;
            isMoving = false;
            movingAway = false;
            moveAwayTime = 0f;

            UpdateTransform(0f, true);
        }

        public void StartMoving()
        {
            isMoving = true;
        }

        public void StopMoving()
        {
            isMoving = false;
        }

        public void StartMovingAway(float duration, float speedMultiplier)
        {
            movingAway = true;
            moveAwayTime = 0f;
            moveAwayDuration = Mathf.Max(duration, 0.01f);
            moveAwaySpeedMultiplier = speedMultiplier;
            isMoving = true;
        }

        private void Update()
        {
            if (!isMoving || config == null)
                return;

            float speed = config.ForwardSpeed;

            if (movingAway)
            {
                moveAwayTime += Time.deltaTime;
                speed *= Mathf.Lerp(1f, moveAwaySpeedMultiplier, Mathf.Clamp01(moveAwayTime / moveAwayDuration));
            }

            distanceTravelled += speed * Time.deltaTime;

            UpdateTransform(Time.deltaTime, false);
        }

        private void UpdateTransform(float deltaTime, bool instant)
        {
            if (config == null)
                return;

            float ramp = config.SwayRampDistance > 0f
                ? Mathf.SmoothStep(0f, 1f, distanceTravelled / config.SwayRampDistance)
                : 1f;

            if (movingAway)
                ramp *= 1f - Mathf.Clamp01(moveAwayTime / moveAwayDuration);

            float phase = distanceTravelled * config.SwayFrequency;
            float lateral = config.SwayAmplitude * ramp * Mathf.Sin(phase);

            transform.position = startPosition + new Vector3(lateral, 0f, distanceTravelled);

            float slope = config.SwayAmplitude * config.SwayFrequency * ramp * Mathf.Cos(phase);
            float turn = Mathf.Atan2(slope * config.TurnResponse, 1f) * Mathf.Rad2Deg;
            float bank = -slope * config.BankAngle;

            Quaternion targetRotation = Quaternion.Euler(0f, turn, bank);

            transform.rotation = instant
                ? targetRotation
                : Quaternion.Slerp(transform.rotation, targetRotation, config.RotationLerp * deltaTime);
        }
    }
}