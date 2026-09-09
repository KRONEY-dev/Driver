using Driver.Managers;
using Driver.Managers.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Driver.UI.Pages
{
    public class HomePage : BasePage
    {
        [SerializeField] private TextMeshProUGUI currentLevelText;

        [SerializeField] private Button playButton;

        [Inject] private AppStateManager appStateManager;
        [Inject] private IDataManager dataManager;

        public override void Init()
        {
            base.Init();

            playButton.onClick.AddListener(PlayButtonOnClickHandler);
        }

        public override void Show(object data)
        {
            base.Show(data);

            var currentLevelIndex = dataManager.UserData.UserProgression.CurrentLevelIndex;
            currentLevelText.text = (currentLevelIndex + 1).ToString();
        }

        private void PlayButtonOnClickHandler()
        {
            appStateManager.SetState(AppStateManager.AppState.Level);
        }
    }
}