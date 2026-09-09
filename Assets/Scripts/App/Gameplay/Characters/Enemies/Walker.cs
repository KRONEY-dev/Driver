using System;
using UnityEngine;

namespace Driver.Gameplay.Characters.Enemies
{
    [RequireComponent(typeof(Rigidbody))]
    public class Walker : Enemy
    {
        private enum Phase
        {
            Idle,
            Chasing,
            Attacking
        }

        [SerializeField] private string runningAnimatorParam;
        [SerializeField] private string walkingAnimatorParam;

        private Phase phase;

        private Vector3 patrolHome;
        private Vector3 patrolAway;
        private bool patrolToAway;
        private bool patrolPaused;
        private float patrolPauseTime;

        public override void Activate(ActivationData activationData, Action requestDeactivation)
        {
            base.Activate(activationData, requestDeactivation);

            Vector3 forward = transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            forward.Normalize();

            patrolHome = transform.position;
            patrolAway = patrolHome + forward * CharacterConfig.PatrolDistance;

            patrolToAway = UnityEngine.Random.value > 0.5f;
            patrolPaused = UnityEngine.Random.value > 0.5f;
            patrolPauseTime = UnityEngine.Random.Range(0f, NextPauseTime());

            transform.position = Vector3.Lerp(patrolHome, patrolAway, UnityEngine.Random.value);

            SetPhase(Phase.Idle);
        }

        protected override void UpdateBehaviour(float deltaTime)
        {
            switch (phase)
            {
                case Phase.Idle:
                    if (FlatDistanceToTarget() <= CharacterConfig.AggroRadius)
                    {
                        SetPhase(Phase.Chasing);
                        return;
                    }

                    Patrol(deltaTime);
                    break;

                case Phase.Chasing:
                    TurnTowards(Target.position, deltaTime);
                    MoveForward(CharacterConfig.MoveSpeed, deltaTime);
                    break;
            }
        }

        private void Patrol(float deltaTime)
        {
            Vector3 goal = patrolToAway ? patrolAway : patrolHome;

            TurnTowards(goal, deltaTime);

            if (patrolPaused)
            {
                SetWalking(false);

                patrolPauseTime -= deltaTime;
                if (patrolPauseTime <= 0f)
                    patrolPaused = false;

                return;
            }

            SetWalking(true);

            MoveForward(CharacterConfig.PatrolSpeed, deltaTime);

            Vector3 toGoal = goal - transform.position;
            toGoal.y = 0f;

            if (toGoal.magnitude <= 0.2f || Vector3.Dot(transform.forward, toGoal) <= 0f)
            {
                patrolToAway = !patrolToAway;
                patrolPaused = true;
                patrolPauseTime = NextPauseTime();
            }
        }

        private float NextPauseTime()
        {
            return CharacterConfig.PatrolPauseDuration + UnityEngine.Random.Range(0f, CharacterConfig.PatrolPauseRandom);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsAlive || phase == Phase.Attacking || other != TargetCollider)
                return;

            Attack();
        }

        private void Attack()
        {
            SetPhase(Phase.Attacking);

            if (Target.TryGetComponent(out CharacterProxy proxy) && proxy.Character is { IsAlive: true } car)
                car.TakeDamage(CharacterConfig.DamageToCar);

            Die();
        }

        private void TurnTowards(Vector3 point, float deltaTime)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, CharacterConfig.TurnSpeed * deltaTime);
        }

        private void MoveForward(float speed, float deltaTime)
        {
            Vector3 step = transform.forward;
            step.y = 0f;

            if (step.sqrMagnitude < 0.0001f)
                return;

            transform.position += step.normalized * (speed * deltaTime);
        }

        private float FlatDistanceToTarget()
        {
            Vector3 delta = Target.position - transform.position;
            delta.y = 0f;

            return delta.magnitude;
        }

        private void SetPhase(Phase value)
        {
            phase = value;

            if (Animator == null)
                return;

            Animator.SetBool(runningAnimatorParam, value == Phase.Chasing);

            if (value != Phase.Idle)
                Animator.SetBool(walkingAnimatorParam, false);
        }

        private void SetWalking(bool value)
        {
            if (Animator == null)
                return;

            Animator.SetBool(walkingAnimatorParam, value);
        }
    }
}