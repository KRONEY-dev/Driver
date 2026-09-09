using Driver.Controllers;
using UnityEngine;
using Zenject;

namespace Driver.Zenject.Installers
{
    public class LoadingInstaller : SceneInstaller
    {
        [SerializeField] private BootstrapController bootstrap;

        public override void InstallBindings()
        {
            base.InstallBindings();

            InstallBindingUIComponents();

            Container.Bind<IInitializable>().FromInstance(bootstrap).AsSingle().NonLazy();
            Container.BindInitializableExecutionOrder<BootstrapController>(0);
        }
    }
}