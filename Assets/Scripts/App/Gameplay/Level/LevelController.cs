using Cysharp.Threading.Tasks;
using Driver.Controllers.Interfaces;
using Driver.Data.Scriptable;
using Driver.Gameplay.Characters;
using Driver.Gameplay.Input;
using Driver.Managers;
using Driver.Managers.Interfaces;
using Driver.UI.Pages;
using Driver.UI.Popups;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Driver.Gameplay.Level
{
    public enum RunOutcome
    {
        Win,
        Lose
    }

    public class LevelController : MonoBehaviour, IInitializable
    {
        [SerializeField] private Level levelPrefab;
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private Camera levelCamera;
        [SerializeField] private CameraFollow cameraFollow;

        [Inject] private LevelsConfig levelsConfig;
        [Inject] private CarConfig carConfig;
        [Inject] private EnemiesConfig enemiesConfig;
        [Inject] private VfxConfig vfxConfig;
        [Inject] private ITimerController timerController;
        [Inject] private IGameplayInput input;
        [Inject] private IUIManager uiManager;
        [Inject] private IDataManager dataManager;
        [Inject] private AppStateManager appStateManager;

        private int CurrentLevelIndex => dataManager.UserData?.UserProgression?.CurrentLevelIndex ?? 0;

        private LevelConfig levelConfig;
        private CancellationTokenSource lifetime;
        private Level level;
        private GameplayPage gameplayPage;

        public void Initialize()
        {
            gameplayPage = uiManager.GetPage<GameplayPage>();
            gameplayPage.SetHudVisible(false);

            levelConfig = levelsConfig.Get(CurrentLevelIndex);
            BuildLevel();

            lifetime = new CancellationTokenSource();
            RunLoopAsync(lifetime.Token).Forget();
        }

        private void OnDestroy()
        {
            lifetime?.Cancel();
            lifetime?.Dispose();
        }

        private void LateUpdate()
        {
            if (level == null || !level.IsRunning)
                return;

            gameplayPage.SetHealthNormalized(level.Car.HealthNormalized);
            gameplayPage.SetHealthBarViewportPosition(levelCamera.WorldToViewportPoint(level.Car.HealthPosition));
            gameplayPage.SetProgress(level.GetProgressByDistance(), level.Car.DistanceTravelled);
        }

        private void BuildLevel()
        {
            level = Instantiate(levelPrefab, transform);
            level.Init(levelConfig, levelsConfig.Outro, carConfig, carPrefab, vfxConfig, enemiesConfig, timerController,
                input, levelCamera, cameraFollow);

            level.OnCharacterDamagedEvent += OnCharacterDamagedEventHandler;
            level.OnRunningChanged += OnRunningChangedHandler;
        }

        private void DestroyLevel()
        {
            if (level == null)
                return;

            level.OnCharacterDamagedEvent -= OnCharacterDamagedEventHandler;
            level.OnRunningChanged -= OnRunningChangedHandler;

            Destroy(level.gameObject);
            level = null;
        }

        private void OnRunningChangedHandler(bool running)
        {
            gameplayPage.SetHudVisible(running);
        }

        private async UniTaskVoid RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                gameplayPage.ResetHud();

                await gameplayPage.WaitForStartTapAsync(token);

                level.StartRun();

                RunOutcome outcome = await WaitForRunOutcomeAsync(token);

                ResultChoice choice = outcome == RunOutcome.Win
                    ? await ShowWinAsync(token)
                    : await ShowGameOverAsync(token);

                switch (choice)
                {
                    case ResultChoice.Home:
                        appStateManager.SetState(AppStateManager.AppState.MainMenu);
                        return;
                    case ResultChoice.Next:
                    case ResultChoice.Retry:
                        RebuildLevel();
                        break;
                }
            }

            async UniTask<RunOutcome> WaitForRunOutcomeAsync(CancellationToken token)
            {
                await UniTask.WaitUntil(() => level.IsFinishReached() || !level.Car.IsAlive, cancellationToken: token);

                return level.IsFinishReached() ? RunOutcome.Win : RunOutcome.Lose;
            }

            void RebuildLevel()
            {
                levelConfig = levelsConfig.Get(CurrentLevelIndex);

                DestroyLevel();
                BuildLevel();
            }
        }

        private async UniTask<ResultChoice> ShowWinAsync(CancellationToken token)
        {
            dataManager.UserData?.UserProgression?.OnLevelCompleted();
            dataManager.SaveUserData();

            await level.PlayOutroAsync(token);

            uiManager.OpenPopup<WinPopup>();
            ResultChoice choice = await uiManager.GetPopup<WinPopup>().WaitForChoiceAsync(token);
            uiManager.HidePopup<WinPopup>();

            return choice;
        }

        private async UniTask<ResultChoice> ShowGameOverAsync(CancellationToken token)
        {
            level.StopRun();

            uiManager.OpenPopup<GameOverPopup>();
            ResultChoice choice = await uiManager.GetPopup<GameOverPopup>().WaitForChoiceAsync(token);
            uiManager.HidePopup<GameOverPopup>();

            return choice;
        }

        private void OnCharacterDamagedEventHandler(float damage, CharacterType characterType, Vector3 worldPosition)
        {
            Vector3 viewportPosition = level.LevelCamera.WorldToViewportPoint(worldPosition);

            gameplayPage?.ShowDamageInfo(damage, characterType, viewportPosition);
        }
    }
}