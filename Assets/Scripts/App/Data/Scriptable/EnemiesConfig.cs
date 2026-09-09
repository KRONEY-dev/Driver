using System;
using Driver.Extensions;
using UnityEngine;

namespace Driver.Data.Scriptable
{
    [CreateAssetMenu(fileName = "EnemiesConfig", menuName = "ScriptableObjects/EnemiesConfig")]
    public class EnemiesConfig : ScriptableObject
    {
        public EnemyConfig[] Enemies;

        public bool TryGetByType(EnemyType type, out EnemyConfig enemyConfig)
        {
            return Enemies.TryGet(it => it.EnemyType == type, out enemyConfig);
        }
    }

    public enum EnemyType
    {
        Walker
    }

    [Serializable]
    public class EnemyConfig : ICharacterConfig
    {
        [Header("Identity")]
        public EnemyType EnemyType;
        public GameObject Prefab;
        public int InitialPoolSize;

        [field: Header("Stats")]
        [field: SerializeField] public float MaxHealth { get; protected set; }
        public float MoveSpeed;
        public float TurnSpeed;
        public float DamageToCar;
        public float AggroRadius;
        public float PatrolDistance;
        public float PatrolSpeed;
        public float PatrolPauseDuration;
        public float PatrolPauseRandom;
        public float OffscreenLifetime;

        public string GetId()
        {
            return EnemyType.ToString();
        }
    }
}