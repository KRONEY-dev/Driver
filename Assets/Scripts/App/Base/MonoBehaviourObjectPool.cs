using Driver.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Driver.Base.ObjectPool
{
    public readonly struct IdentifiedObjectPoolInfo
    {
        public readonly string ObjectPoolId;
        public readonly MonoBehaviourPoolInitData ObjectPoolInitData;

        public IdentifiedObjectPoolInfo(string objectPoolId, MonoBehaviourPoolInitData objectPoolInitData)
        {
            ObjectPoolId = objectPoolId;
            ObjectPoolInitData = objectPoolInitData;
        }
    }

    public class ObjectPoolsController<ObjectT, TActivationData> where ObjectT : IMonoBehaviourObjectPoolItem<TActivationData>
    {
        private readonly IdentifiedMonoBehaviourObjectPool<ObjectT, TActivationData>[] objectPools;

        public ObjectPoolsController(IEnumerable<IdentifiedObjectPoolInfo> objectPoolInfos)
        {
            var poolsCount = objectPoolInfos.Count();

            objectPools = new IdentifiedMonoBehaviourObjectPool<ObjectT, TActivationData>[poolsCount];

            int index = 0;
            foreach (var poolInfo in objectPoolInfos)
            {
                objectPools[index] = new IdentifiedMonoBehaviourObjectPool<ObjectT, TActivationData>(poolInfo.ObjectPoolId, poolInfo.ObjectPoolInitData);

                index++;
            }
        }

        public IEnumerable<ObjectPoolHandle<ObjectT>> GetAllActiveItems()
        {
            var activeObjects = new List<ObjectPoolHandle<ObjectT>>();

            foreach (var objectPool in objectPools)
            {
                activeObjects.AddRange(objectPool.GetAllActiveObjects());
            }

            return activeObjects;
        }

        public bool TryGetMonoBehaviourObjectPoolByIdentifier(string id, out IdentifiedMonoBehaviourObjectPool<ObjectT, TActivationData> objectPool)
        {
            return objectPools.TryGet(it => it.Id.Equals(id), out objectPool);
        }
    }

    public readonly struct NoActivationData { }
    public interface IMonoBehaviourObjectPoolItem<T>
    {
        void Activate(T data, Action requestDeactivation);
        void Deactivate();
    }

    public class IdentifiedMonoBehaviourObjectPool<TObject, TActivationData> : MonoBehaviourObjectPool<TObject, TActivationData> where TObject : IMonoBehaviourObjectPoolItem<TActivationData>
    {
        public readonly string Id;

        public IdentifiedMonoBehaviourObjectPool(string id, MonoBehaviourPoolInitData initData) : base(initData)
        {
            Id = id;
        }
    }

    public readonly struct ObjectPoolHandle<TObject>
    {
        private readonly IPublicPoolObject<TObject> target;
        private readonly Guid capturedId;

        public ObjectPoolHandle(IPublicPoolObject<TObject> target)
        {
            this.target = target;
            capturedId = target.Id;
        }

        public bool TryResolve(out TObject value)
        {
            if (target != null && target.Id == capturedId && (target.IsOccupied.HasValue && target.IsOccupied.Value))
            {
                value = target.Item;
                return true;
            }

            value = default;
            return false;
        }
    }

    public interface IPublicPoolObject<T>
    {
        T Item { get; }
        Guid Id { get; }
        bool? IsOccupied { get; }
    }

    public readonly struct MonoBehaviourPoolInitData
    {
        public readonly GameObject Prefab;
        public readonly int InitPoolSize;
        public readonly Transform Parent;

        public MonoBehaviourPoolInitData(GameObject prefab, int initPoolSize, Transform parent = null)
        {
            Prefab = prefab;
            InitPoolSize = initPoolSize;
            Parent = parent;
        }
    }

    public class MonoBehaviourObjectPool<TObject, TActivationData> where TObject : IMonoBehaviourObjectPoolItem<TActivationData>
    {
        private readonly List<PoolObject> objects;

        private readonly MonoBehaviourPoolInitData initData;

        public MonoBehaviourObjectPool(MonoBehaviourPoolInitData initData)
        {
            this.initData = initData;

            if (!initData.Prefab.TryGetComponent(out TObject _))
            {
                Debug.LogError("Error");
                return;
            }

            objects = new List<PoolObject>(initData.InitPoolSize);

            for (int i = 0; i < initData.InitPoolSize; i++)
            {
                Create(true);
            }
        }

        public ObjectPoolHandle<TObject> ReleseObject(TActivationData activationData)
        {
            var activeObject = objects.FirstOrDefault(it => !it.IsOccupied.HasValue || !it.IsOccupied.Value);

            activeObject ??= Create(false);

            activeObject.Activate(activationData);

            return new ObjectPoolHandle<TObject>(activeObject);
        }

        public IEnumerable<ObjectPoolHandle<TObject>> GetAllActiveObjects()
        {
            var activeObjects = objects.FindAll(it => it.IsOccupied.HasValue && it.IsOccupied.Value);

            return activeObjects.Select(it => new ObjectPoolHandle<TObject>(it));
        }

        private PoolObject Create(bool isWarmup)
        {
            var prefab = initData.Prefab;
            var parent = initData.Parent;

            GameObject createdGameObject;
            PoolObject createdPoolObject;

            if (parent != null)
            {
                createdGameObject = UnityEngine.Object.Instantiate(prefab, parent);
            }
            else
            {
                createdGameObject = UnityEngine.Object.Instantiate(prefab);
            }

            createdPoolObject = new PoolObject(createdGameObject.GetComponent<TObject>());
            objects.Add(createdPoolObject);

            if (isWarmup)
            {
                createdPoolObject.Deactivate();
            }

            return createdPoolObject;
        }

        private class PoolObject : IPublicPoolObject<TObject>
        {
            public TObject Item { get; }
            public Guid Id { get; private set; }
            public bool? IsOccupied { get; private set; }

            public PoolObject(TObject item)
            {
                Item = item;
            }

            public void Activate(TActivationData data)
            {
                if (IsOccupied.HasValue && IsOccupied.Value)
                    return;

                Id = Guid.NewGuid();
                IsOccupied = true;

                Item.Activate(data, Deactivate);
            }

            public void Deactivate()
            {
                if (IsOccupied.HasValue && !IsOccupied.Value)
                    return;

                Item.Deactivate();

                Id = Guid.Empty;
                IsOccupied = false;
            }
        }
    }
}