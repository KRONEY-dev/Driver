using System;
using System.Collections.Generic;
using Driver.UI.Pages;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;
using Driver.Managers.Interfaces;
using Driver.UI.SurfaceInterfaces;

namespace Driver.Managers
{
    public class AppStateManager
    {
        public enum AppState
        {
            None,

            Init,
            MainMenu,
            Level
        }

        public event Action AppSceneChangeStartedEvent;
        public event Action AppSceneChangeEndedEvent;

        public AppState CurrentState { get; private set; }

        [Inject] private IUIManager uiManager;

        private Dictionary<AppState, StateInfo> statesInfo;
        private string previousScene;
        private string currentScene;

        [Inject]
        private void Init()
        {
            CurrentState = AppState.None;

            statesInfo = new Dictionary<AppState, StateInfo>
            {
                {
                    AppState.Init, new StateInfo("Init", SetPage<LoadingPage>)
                },
                {
                    AppState.MainMenu, new StateInfo("MainMenu", SetPage<HomePage>)
                },
                {
                    AppState.Level, new StateInfo("Level", SetPage<GameplayPage>)
                }
            };
        }

        public void SetState(AppState state, object message = null, bool isOnOpen = false, Action onComplete = null)
        {
            if (statesInfo.TryGetValue(state, out StateInfo stateInfo))
            {
                uiManager.ShowSurfaceInterface<InputLockSurfaceInterface>();

                CurrentState = state;
                previousScene = currentScene;
                currentScene = stateInfo.SceneName;

                var afterSceneLoadedAction = stateInfo.AfterSceneLoaded;

                if (CurrentState == AppState.Init && isOnOpen)
                {
                    afterSceneLoadedAction?.Invoke(message);

                    uiManager.HideSurfaceInterface<InputLockSurfaceInterface>();
                }
                else
                {
                    AppSceneChangeStartedEvent?.Invoke();

                    SceneManager.LoadSceneAsync(currentScene, LoadSceneMode.Additive)!.completed += _ =>
                    {
                        SceneManager.UnloadSceneAsync(previousScene)!.completed += _ =>
                        {
                            
                            SceneContext sceneContext = Object.FindFirstObjectByType<SceneContext>();
                            
                            if (sceneContext != null && sceneContext.IsKernelCreated)
                            {
                                if (sceneContext.KernelWasInitialize)
                                {
                                    ContinueChangeState();
                                }
                                else
                                {
                                    sceneContext.PostKernelInitialize -= ContinueChangeState;
                                    sceneContext.PostKernelInitialize += ContinueChangeState;
                                }
                            }
                            else
                            {
                                ContinueChangeState();
                            }

                            void ContinueChangeState()
                            {
                                if (sceneContext && sceneContext.KernelWasInitialize)
                                {
                                    sceneContext.PostKernelInitialize -= ContinueChangeState;
                                }

                                afterSceneLoadedAction?.Invoke(message);
                                onComplete?.Invoke();

                                AppSceneChangeEndedEvent?.Invoke();

                                uiManager.HideSurfaceInterface<InputLockSurfaceInterface>();
                            }
                        };
                    };
                }
            }
        }

        private void SetPage<T>(object message) where T : IPage
        {
            uiManager.SetPage<T>(message);
        }

        private readonly struct StateInfo
        {
            public readonly string SceneName;
            public readonly Action<object> AfterSceneLoaded;

            public StateInfo(string sceneName, Action<object> afterSceneLoaded = null)
            {
                SceneName = sceneName;
                AfterSceneLoaded = afterSceneLoaded;
            }
        }
    }
}