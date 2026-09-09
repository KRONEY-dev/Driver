using System;
using UnityEngine;

namespace Driver.Data.Scriptable
{
    [CreateAssetMenu(fileName = "CarConfig", menuName = "ScriptableObjects/CarConfig")]
    public class CarConfig : ScriptableObject
    {
        public CarCharacterConfig CarCharacterConfig;

        public TurretConfig Turret;
    }

    public interface ICharacterConfig
    {
        float MaxHealth { get; }
    }

    [Serializable]
    public class CarCharacterConfig : ICharacterConfig
    {
        [field: Header("Health")]
        [field: SerializeField] public float MaxHealth { get; protected set; }

        [Header("Movement")]
        public float ForwardSpeed;

        [Header("Weave")]
        public float SwayAmplitude;
        public float SwayFrequency;
        public float SwayRampDistance;

        [Header("Feel")]
        [Range(0f, 1f)] public float TurnResponse;
        public float BankAngle;
        public float RotationLerp;
    }

    [Serializable]
    public class TurretConfig
    {
        [Header("Projectile")]
        public GameObject BulletPrefab;
        public int InitialPoolSize;
        public float BulletSpeed;
        public float BulletDamage;
        public float BulletMaxDistance;
        public float BulletRadius;
        public LayerMask HitMask;

        [Header("Aiming")]
        public float MaxAngle;
        public float AngleFollowSpeed;

        [Header("Firing")]
        public float FireRate;
    }
}