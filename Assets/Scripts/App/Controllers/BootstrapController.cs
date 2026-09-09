using System;
using Driver.Controllers.Interfaces;
using Driver.Managers;
using Driver.Managers.Interfaces;
using Driver.UI.Pages;
using UnityEngine;
using Zenject;

namespace Driver.Controllers
{
    public class BootstrapController : MonoBehaviour, IBootstrapController, IInitializable
    {
        [Inject] private IUIManager uiManager;
        [Inject] private IDataManager dataManager;
        [Inject] private AppStateManager appStateManager;

        private DateTime loadingStartDate;
        private int defaultSleepTimeout;

        public void Initialize()
        {
            Application.targetFrameRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

            defaultSleepTimeout = Screen.sleepTimeout;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            appStateManager.SetState(AppStateManager.AppState.Init, isOnOpen: true);

            dataManager.LoadUserData();
            dataManager.RegisterOnDataLoadedEvent(OnDataLoadedEventHandler);

            var loadingPage = uiManager.GetPage<LoadingPage>();
            loadingPage.OnLoadingCompleteEvent += OnLoadingCompleteEventHandler;

            loadingPage.StartLoadingAnimation();
        }

        private void OnDestroy()
        {
            Screen.sleepTimeout = defaultSleepTimeout;
        }

        private void SetMainState()
        {
            uiManager.GetPage<LoadingPage>().OnLoadingCompleteEvent -= OnLoadingCompleteEventHandler;

            appStateManager.SetState(AppStateManager.AppState.MainMenu);
        }

        private void OnDataLoadedEventHandler(DataManager.CachedData userData)
        {
            dataManager.UnregisterOnDataLoadedEvent(OnDataLoadedEventHandler);

            if (uiManager.GetPage<LoadingPage>().IsLoadingComplete)
            {
                SetMainState();
            }
        }

        private void OnLoadingCompleteEventHandler()
        {
            if (dataManager.IsDataLoaded)
            {
                SetMainState();
            }
        }
    }
}