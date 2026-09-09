using Driver.Data.Scriptable;
using Driver.Gameplay.Input;
using Driver.Gameplay.Level;
using UnityEngine;
using Zenject;

namespace Driver.Zenject.Installers
{
    public class LevelInstaller : SceneInstaller
    {
        [Header("Scriptable")]
        [SerializeField] private LevelsConfig levelsConfig;
        [SerializeField] private CarConfig carConfig;
        [SerializeField] private EnemiesConfig enemiesConfig;
        [SerializeField] private VfxConfig vfxConfig;

        [Header("Controllers")]
        [SerializeField] private LevelController levelController;

        public override void InstallBindings()
        {
            base.InstallBindings();

            SetupBindings();

            InstallBindingUIComponents();

            Container.Bind<IInitializable>().FromInstance(levelController).AsSingle().NonLazy();
            Container.BindInitializableExecutionOrder<LevelController>(0);
        }

        private void SetupBindings()
        {
            #region Scriptable
            Container.Bind<LevelsConfig>().FromInstance(levelsConfig).AsSingle().NonLazy();
            Container.Bind<CarConfig>().FromInstance(carConfig).AsSingle().NonLazy();
            Container.Bind<EnemiesConfig>().FromInstance(enemiesConfig).AsSingle().NonLazy();
            Container.Bind<VfxConfig>().FromInstance(vfxConfig).AsSingle().NonLazy();
            #endregion

            Container.Bind<IGameplayInput>().To<GameplayInput>().AsSingle();
        }
    }
}