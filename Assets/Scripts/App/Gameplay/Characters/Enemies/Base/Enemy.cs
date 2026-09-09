using System;
using Driver.Base.ObjectPool;
using Driver.Controllers.Interfaces;
using Driver.Data.Scriptable;
using UnityEngine;

namespace Driver.Gameplay.Characters.Enemies
{
    public interface IEnemy : ICharacter
    {
        GameObject GameObject { get; }
        Collider Collider { get; }
    }

    public abstract class Enemy : Character<EnemyConfig>, IEnemy, IMonoBehaviourObjectPoolItem<Enemy.ActivationData>
    {
        public readonly struct ActivationData
        {
            public readonly EnemyConfig Config;
            public readonly ITimerController TimerController;
            public readonly Transform Target;
            public readonly Collider TargetCollider;
            public readonly Vector3 Position;

            public ActivationData(EnemyConfig config, ITimerController timerController, Transform target,
                Collider targetCollider, Vector3 position)
            {
                Config = config;
                TimerController = timerController;
                Target = target;
                TargetCollider = targetCollider;
                Position = position;
            }
        }

        public override CharacterType CharacterType => CharacterType.Enemy;

        GameObject IEnemy.GameObject => gameObject;
        Collider IEnemy.Collider => MainCollider;

        [SerializeField] protected Collider MainCollider;
        [SerializeField] protected Animator Animator;

        protected Transform Target;
        protected Collider TargetCollider;

        private Action requestDeactivation;

        public void RequestDeactivation()
        {
            requestDeactivation?.Invoke();
        }

        public virtual void Activate(ActivationData activationData, Action requestDeactivation)
        {
            this.requestDeactivation = requestDeactivation;

            gameObject.SetActive(true);

            Init(activationData.Config, activationData.TimerController);

            Target = activationData.Target;
            TargetCollider = activationData.TargetCollider;

            SetPosition(activationData.Position);

            if (MainCollider != null)
                MainCollider.enabled = true;
        }

        public virtual void Deactivate()
        {
            Stop();

            gameObject.SetActive(false);
        }

        public override void Stop()
        {
            if (IsStopped)
                return;

            base.Stop();

            Target = null;
            TargetCollider = null;
        }

        protected override void OnHealthOutEventHandler()
        {
            base.OnHealthOutEventHandler();

            RequestDeactivation();
        }

        protected void Die()
        {
            OnHealthOutEventHandler();
        }

        protected abstract void UpdateBehaviour(float deltaTime);

        private void Update()
        {
            if (!IsAlive || Target == null)
                return;

            UpdateBehaviour(Time.deltaTime);
        }
    }
}