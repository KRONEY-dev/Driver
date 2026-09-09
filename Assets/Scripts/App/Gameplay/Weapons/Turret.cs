using Driver.Base.ObjectPool;
using Driver.Data.Scriptable;
using Driver.Gameplay.Weapons.Projectiles;
using UnityEngine;

namespace Driver.Gameplay.Weapons
{
    public class Turret
    {
        private readonly TurretConfig config;
        private readonly Transform pivot;
        private readonly Transform muzzle;
        private readonly Camera camera;

        private readonly MonoBehaviourObjectPool<Bullet, Projectile.ActivationData> bulletsObjectPool;

        private float targetAngle;
        private float currentAngle;
        private bool isFiring;
        private float fireCooldown;

        public Turret(TurretConfig config, Transform pivot, Transform muzzle, Transform poolParent, Camera camera)
        {
            this.config = config;
            this.pivot = pivot;
            this.muzzle = muzzle;
            this.camera = camera;

            bulletsObjectPool = new MonoBehaviourObjectPool<Bullet, Projectile.ActivationData>(
                new MonoBehaviourPoolInitData(config.BulletPrefab, config.InitialPoolSize, poolParent));
        }

        public void Aim(Vector2 screenPoint)
        {
            Ray ray = camera.ScreenPointToRay(screenPoint);
            Plane aimPlane = new Plane(Vector3.up, new Vector3(0f, muzzle.position.y, 0f));

            if (!aimPlane.Raycast(ray, out float enter))
                return;

            Vector3 local = pivot.parent.InverseTransformPoint(ray.GetPoint(enter));
            float angle = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;

            targetAngle = Mathf.Clamp(angle, -config.MaxAngle, config.MaxAngle);
        }

        public void SetFiring(bool value)
        {
            isFiring = value;
        }

        public void Stop()
        {
            isFiring = false;
            fireCooldown = 0f;
            targetAngle = 0f;
        }

        public void Update()
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, config.AngleFollowSpeed * Time.deltaTime);
            pivot.localRotation = Quaternion.Euler(0f, currentAngle, 0f);

            if (isFiring)
            {
                fireCooldown -= Time.deltaTime;

                if (fireCooldown <= 0f)
                {
                    fireCooldown += 1f / config.FireRate;

                    Fire();
                }
            }
            else
            {
                fireCooldown = 0f;
            }
        }

        private void Fire()
        {
            Vector3 direction = pivot.parent.TransformDirection(Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward);
            direction.y = 0f;
            direction.Normalize();

            bulletsObjectPool.ReleseObject(new Projectile.ActivationData(
                muzzle.position, direction,
                config.BulletSpeed, config.BulletDamage, config.BulletMaxDistance, config.BulletRadius, config.HitMask,
                camera));
        }
    }
}