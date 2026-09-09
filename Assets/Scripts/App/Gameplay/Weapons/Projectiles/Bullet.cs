using Driver.Extensions;
using Driver.Gameplay.Characters;
using UnityEngine;

namespace Driver.Gameplay.Weapons.Projectiles
{
    public class Bullet : Projectile
    {
        [SerializeField] private TrailRenderer trail;

        public override void Init(ActivationData activationData)
        {
            base.Init(activationData);

            ClearTrail();
        }

        public override void Stop()
        {
            base.Stop();

            ClearTrail();
        }

        private void ClearTrail()
        {
            if (trail != null)
            {
                trail.Clear();
            }
        }

        private void Update()
        {
            if (!IsActive)
                return;

            float step = Data.Speed * Time.deltaTime;

            if (Physics.SphereCast(transform.position, Data.Radius, Direction, out RaycastHit hit,
                    step, Data.HitMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.TryGetComponent(out CharacterProxy proxy) && proxy.Character.IsAlive)
                {
                    proxy.Character.TakeDamage(Data.Damage);
                }

                Recycle();
                return;
            }

            transform.position += Direction * step;
            DistanceTravelled += step;

            if (DistanceTravelled >= Data.MaxDistance || !Data.Camera.IsInView(transform.position))
            {
                Recycle();
            }
        }
    }
}