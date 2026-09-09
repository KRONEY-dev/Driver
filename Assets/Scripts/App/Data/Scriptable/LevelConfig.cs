using System;
using UnityEngine;

namespace Driver.Data.Scriptable
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ScriptableObjects/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        public string DisplayName;

        [Header("Track")]
        public float Length;
        public GameObject GroundTilePrefab;
        public float GroundTileLength;
        public float GroundPaddingBefore;
        public float GroundPaddingAfter;

        [Header("Enemies")]
        public EnemySpawnEntry[] Spawns;
    }

    [Serializable]
    public class EnemySpawnEntry
    {
        public EnemyType EnemyType;
        public int Count;
        public float FromDistance;
        public float ToDistance;
        public float LateralRange;
    }
}