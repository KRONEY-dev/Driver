using System;
using Driver.Base.ObjectPool;
using UnityEngine;

namespace Driver.Gameplay.Vfx
{
    public class OneShotVfx : MonoBehaviour, IMonoBehaviourObjectPoolItem<OneShotVfx.ActivationData>
    {
        public readonly struct ActivationData
        {
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;

            public ActivationData(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }

        [SerializeField] private ParticleSystem particles;

        private Action requestDeactivation;

        public void Activate(ActivationData data, Action requestDeactivation)
        {
            this.requestDeactivation = requestDeactivation;

            transform.SetPositionAndRotation(data.Position, data.Rotation);

            gameObject.SetActive(true);

            particles.Clear(true);
            particles.Play(true);
        }

        public void Deactivate()
        {
            requestDeactivation = null;

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            gameObject.SetActive(false);
        }

        private void OnParticleSystemStopped()
        {
            requestDeactivation?.Invoke();
        }
    }
}