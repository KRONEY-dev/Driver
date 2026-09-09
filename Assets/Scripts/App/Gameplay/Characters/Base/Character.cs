using System;
using Driver.Controllers.Interfaces;
using Driver.Data.Scriptable;
using UnityEngine;

namespace Driver.Gameplay.Characters
{
    public enum CharacterType
    {
        Car = 0,
        Enemy = 1
    }

    public interface ICharacter
    {
        Guid Id { get; }

        bool IsAlive { get; }

        void TakeDamage(float damage);
    }

    public abstract class Character<CC> : MonoBehaviour, ICharacter
        where CC : ICharacterConfig
    {
        public event Action OnHealthOutEvent;
        public event Action<float> OnHealthChangedEvent;
        public event Action<float, CharacterType, Vector3> OnDamagedEvent;

        public Guid Id { get; private set; }

        public bool IsAlive => Health != null && Health.IsAlive;

        public float HealthNormalized => Health != null ? Health.Normalized : 1f;

        public abstract CharacterType CharacterType { get; }

        protected bool IsStopped;

        protected CC CharacterConfig;

        protected ITimerController TimerController;

        protected HealthController Health;

        public virtual void Init(CC characterConfig, ITimerController timerController)
        {
            Id = Guid.NewGuid();

            CharacterConfig = characterConfig;
            TimerController = timerController;

            Health = new HealthController(CharacterConfig.MaxHealth);

            Health.OnHealthOutEvent += OnHealthOutEventHandler;
            Health.OnHealthChangedEvent += OnHealthChangedEventHandler;

            IsStopped = false;
        }

        public virtual void Stop()
        {
            if (IsStopped)
                return;

            IsStopped = true;

            Id = Guid.Empty;

            CharacterConfig = default;
            TimerController = null;
            Health = null;

            OnHealthOutEvent = null;
            OnHealthChangedEvent = null;
            OnDamagedEvent = null;
        }

        public virtual void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive)
                return;

            OnDamagedEvent?.Invoke(damage, CharacterType, transform.position);

            Health.TakeDamage(damage);
        }

        protected virtual void OnHealthOutEventHandler()
        {
            OnHealthOutEvent?.Invoke();

            Stop();
        }

        protected virtual void OnHealthChangedEventHandler(float newHealth)
        {
            OnHealthChangedEvent?.Invoke(newHealth);
        }

        protected class HealthController
        {
            public event Action OnHealthOutEvent;
            public event Action<float> OnHealthChangedEvent;

            public bool IsAlive { get; private set; }

            public float Health => health;
            public float Max { get; }
            public float Normalized => Max > 0f ? Mathf.Clamp01(health / Max) : 0f;

            private float health;

            public HealthController(float health)
            {
                this.health = health;
                Max = health;

                IsAlive = true;
            }

            public void TakeDamage(float damage)
            {
                health = Math.Max(0, health - damage);

                OnHealthChangedEvent?.Invoke(health);

                CheckIsAlive();
            }

            private void CheckIsAlive()
            {
                if (IsAlive && health <= 0)
                {
                    IsAlive = false;

                    OnHealthOutEvent?.Invoke();
                }
            }
        }
    }
}