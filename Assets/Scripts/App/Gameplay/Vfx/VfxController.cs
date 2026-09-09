using System.Linq;
using Driver.Base.ObjectPool;
using Driver.Data.Scriptable;
using UnityEngine;

namespace Driver.Gameplay.Vfx
{
    public class VfxController
    {
        private readonly ObjectPoolsController<OneShotVfx, OneShotVfx.ActivationData> pools;

        public VfxController(VfxConfig config, Transform parent)
        {
            pools = new ObjectPoolsController<OneShotVfx, OneShotVfx.ActivationData>(config.Effects
                .Where(it => it.Prefab != null)
                .Select(it => new IdentifiedObjectPoolInfo(
                    it.GetId(),
                    new MonoBehaviourPoolInitData(it.Prefab, it.InitialPoolSize, parent))));
        }

        public void Play(VfxType type, Vector3 position, Quaternion rotation)
        {
            if (pools.TryGetMonoBehaviourObjectPoolByIdentifier(type.ToString(),
                    out IdentifiedMonoBehaviourObjectPool<OneShotVfx, OneShotVfx.ActivationData> pool))
            {
                pool.ReleseObject(new OneShotVfx.ActivationData(position, rotation));
            }
        }
    }
}