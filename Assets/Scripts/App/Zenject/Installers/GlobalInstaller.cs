using System;
using Driver.Controllers;
using Driver.Controllers.Interfaces;
using Driver.Managers;
using Driver.Managers.Interfaces;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Driver.Zenject.Installers
{
    public class GlobalInstaller : MonoInstaller
    {
        [SerializeField] private UIManager uIManager;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private TimerController timerController;
        [SerializeField] private CoroutineController coroutineController;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            SetupBindings();
        }

        public void BindFromPrefab<T>(Object prefabToBind, Transform parent, Action<T> callback = null)
        {
            Container.Bind<T>()
                     .FromComponentInNewPrefab(prefabToBind)
                     .UnderTransform(parent)
                     .AsCached()
                     .OnInstantiated((injectContext, _object) => callback?.Invoke((T)_object))
                     .NonLazy();
        }

        public T BindFromType<T>(Type type)
        {
            if (typeof(T).IsAssignableFrom(type))
            {
                Container.Bind(type)
                         .To(type)
                         .AsSingle()
                         .NonLazy();

                return (T)Container.Resolve(type);
            }
            else
            {
                Debug.LogError($"Type {type} cannot be bound to {typeof(T)}");
                return default;
            }
        }

        private void SetupBindings()
        {
            Container.Bind<ITimerController>()
                     .FromComponentInNewPrefab(timerController)
                     .AsSingle()
                     .NonLazy();

            Container.Bind<ICoroutineController>()
                     .FromComponentInNewPrefab(coroutineController)
                     .AsSingle()
                     .NonLazy();

            Container.Bind<IUIManager>()
                     .FromComponentInNewPrefab(uIManager)
                     .AsSingle()
                     .NonLazy();

            Container.Bind<IDataManager>()
                     .FromComponentInNewPrefab(dataManager)
                     .AsSingle()
                     .NonLazy();

            Container.BindInterfacesAndSelfTo<AppStateManager>()
                     .FromNew()
                     .AsSingle()
                     .NonLazy();

            Container.Resolve<IUIManager>()
                     .Init(this);
        }
    }
}