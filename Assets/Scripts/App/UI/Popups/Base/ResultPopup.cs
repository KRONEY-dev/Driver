using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Driver.UI.Popups
{
    public enum ResultChoice
    {
        Home,
        Retry,
        Next
    }

    public abstract class ResultPopup : BasePopup
    {
        [SerializeField] private Button goHomeButton;

        private UniTaskCompletionSource<ResultChoice> choiceSource;

        public override void Init()
        {
            base.Init();

            goHomeButton.onClick.AddListener(GoHomeButtonOnClickHandler);
        }

        public UniTask<ResultChoice> WaitForChoiceAsync(CancellationToken token)
        {
            choiceSource = new UniTaskCompletionSource<ResultChoice>();

            return choiceSource.Task.AttachExternalCancellation(token);
        }

        protected void SubmitChoice(ResultChoice choice)
        {
            choiceSource?.TrySetResult(choice);
        }

        private void GoHomeButtonOnClickHandler()
        {
            SubmitChoice(ResultChoice.Home);
        }
    }
}