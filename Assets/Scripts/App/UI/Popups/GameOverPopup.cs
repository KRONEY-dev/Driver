using UnityEngine;
using UnityEngine.UI;

namespace Driver.UI.Popups
{
    public class GameOverPopup : ResultPopup
    {
        [SerializeField] private Button retryButton;

        public override void Init()
        {
            base.Init();

            retryButton.onClick.AddListener(RetryButtonOnClickHandler);
        }

        private void RetryButtonOnClickHandler()
        {
            SubmitChoice(ResultChoice.Retry);
        }
    }
}