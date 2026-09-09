using System;
using Driver.Extensions;
using UnityEngine;

namespace Driver.Data.Scriptable
{
    [CreateAssetMenu(fileName = "VfxConfig", menuName = "ScriptableObjects/VfxConfig")]
    public class VfxConfig : ScriptableObject
    {
        public VfxEntry[] Effects;

        public bool TryGetByType(VfxType type, out VfxEntry entry)
        {
            return Effects.TryGet(it => it.Type == type, out entry);
        }
    }

    public enum VfxType
    {
        BulletImpact,
        EnemyDeath
    }

    [Serializable]
    public class VfxEntry
    {
        public VfxType Type;
        public GameObject Prefab;
        public int InitialPoolSize;

        public string GetId()
        {
            return Type.ToString();
        }
    }
}