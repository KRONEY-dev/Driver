using Driver.Controllers.Interfaces;
using Driver.Data.Scriptable;
using Driver.Gameplay.Characters.Components;
using Driver.Gameplay.Weapons;
using UnityEngine;

namespace Driver.Gameplay.Characters
{
    [RequireComponent(typeof(Rigidbody))]
    public class Car : Character<CarCharacterConfig>
    {
        public override CharacterType CharacterType => CharacterType.Car;

        public float DistanceTravelled => movement.DistanceTravelled;
        public Vector3 HealthPosition => healthPositionTransform.position;
        public Collider BodyCollider => bodyCollider;

        [SerializeField] private CarMovement movement;
        [SerializeField] private Transform turretPivot;
        [SerializeField] private Transform turretMuzzle;
        [SerializeField] private Transform healthPositionTransform;
        [SerializeField] private Collider bodyCollider;

        private Turret turret;

        public void Init(CarConfig carConfig, ITimerController timerController, Camera camera, Transform bulletsParent)
        {
            base.Init(carConfig.CarCharacterConfig, timerController);

            movement.Init(carConfig.CarCharacterConfig);

            turret ??= new Turret(carConfig.Turret, turretPivot, turretMuzzle, bulletsParent, camera);
            turret.Stop();
        }

        public void StartDriving()
        {
            movement.StartMoving();
        }

        public void StopDriving()
        {
            movement.StopMoving();
            turret?.Stop();
        }

        public void StartMovingAway(float duration, float speedMultiplier)
        {
            movement.StartMovingAway(duration, speedMultiplier);
            turret?.Stop();
        }

        public void ResetToStart()
        {
            movement.ResetToStart();
            turret?.Stop();
        }

        public void AimTurret(Vector2 pointerScreenPosition)
        {
            turret?.Aim(pointerScreenPosition);
        }

        public void SetTurretFiring(bool value)
        {
            turret?.SetFiring(value);
        }

        public override void Stop()
        {
            base.Stop();

            StopDriving();
        }

        private void Update()
        {
            turret?.Update();
        }
    }
}