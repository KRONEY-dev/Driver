using System;
using Driver.Base.ObjectPool;
using UnityEngine;

namespace Driver.Gameplay.Weapons.Projectiles
{
    public abstract class Projectile : MonoBehaviour, IMonoBehaviourObjectPoolItem<Projectile.ActivationData>
    {
        public readonly struct ActivationData
        {
            public readonly Vector3 Position;
            public readonly Vector3 Direction;
            public readonly float Speed;
            public readonly float Damage;
            public readonly float MaxDistance;
            public readonly float Radius;
            public readonly LayerMask HitMask;
            public readonly Camera Camera;

            public ActivationData(Vector3 position, Vector3 direction, float speed, float damage,
                float maxDistance, float radius, LayerMask hitMask, Camera camera)
            {
                Position = position;
                Direction = direction;
                Speed = speed;
                Damage = damage;
                MaxDistance = maxDistance;
                Radius = radius;
                HitMask = hitMask;
                Camera = camera;
            }
        }

        public Guid Id { get; private set; }

        protected bool IsActive;

        protected ActivationData Data;

        protected Vector3 Direction;

        protected float DistanceTravelled;

        protected Action RequestDeactivationEvent;

        public virtual void Activate(ActivationData activationData, Action requestDeactivation)
        {
            RequestDeactivationEvent = requestDeactivation;

            gameObject.SetActive(true);

            Init(activationData);
        }

        public virtual void Deactivate()
        {
            Stop();

            gameObject.SetActive(false);
        }

        public virtual void Init(ActivationData activationData)
        {
            Id = Guid.NewGuid();

            Data = activationData;

            Direction = activationData.Direction.sqrMagnitude > Mathf.Epsilon
                ? activationData.Direction.normalized
                : Vector3.forward;

            DistanceTravelled = 0f;

            transform.SetPositionAndRotation(activationData.Position, Quaternion.LookRotation(Direction));

            IsActive = true;
        }

        public virtual void Stop()
        {
            IsActive = false;

            Id = Guid.Empty;
            DistanceTravelled = 0f;
        }

        protected void Recycle()
        {
            RequestDeactivationEvent?.Invoke();
        }
    }
}