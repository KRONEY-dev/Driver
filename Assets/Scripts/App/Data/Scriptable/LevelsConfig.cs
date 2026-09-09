using System;
using UnityEngine;

namespace Driver.Data.Scriptable
{
    [CreateAssetMenu(fileName = "LevelsConfig", menuName = "ScriptableObjects/LevelsConfig")]
    public class LevelsConfig : ScriptableObject
    {
        public LevelOutroConfig Outro;

        public LevelConfig[] Levels;

        public LevelConfig Get(int index)
        {
            int count = Levels.Length;

            return Levels[((index % count) + count) % count];
        }
    }

    [Serializable]
    public class LevelOutroConfig
    {
        public float AlignDuration;
        public float MoveAwaySpeedMultiplier;
        public float MaxDuration;
    }
}