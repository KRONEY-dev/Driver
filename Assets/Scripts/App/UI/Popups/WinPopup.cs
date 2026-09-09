using UnityEngine;
using UnityEngine.UI;

namespace Driver.UI.Popups
{
    public class WinPopup : ResultPopup
    {
        [SerializeField] private Button nextLevelButton;

        public override void Init()
        {
            base.Init();

            nextLevelButton.onClick.AddListener(NextLevelButtonOnClickHandler);
        }

        private void NextLevelButtonOnClickHandler()
        {
            SubmitChoice(ResultChoice.Next);
        }
    }
}