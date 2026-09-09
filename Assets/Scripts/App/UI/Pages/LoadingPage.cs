using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Driver.UI.Pages
{
    public class LoadingPage : BasePage
    {
        private const float LoadingAnimationDuration = 3f;

        public event Action OnLoadingCompleteEvent;

        public bool IsLoadingComplete { get; private set; }

        [SerializeField] private Image loagingBarImage;

        private Coroutine currentLoadingCoroutine;

        public override void Init()
        {
            base.Init();

            ResetLoadingAnimation();
        }

        public override void Show(object data)
        {
            base.Show(data);

            IsLoadingComplete = false;
        }

        public void StartLoadingAnimation()
        {
            ResetLoadingAnimation();

            currentLoadingCoroutine = StartCoroutine(LoadingCoroutine());
        }

        private IEnumerator LoadingCoroutine()
        {
            float startFill = loagingBarImage.fillAmount;
            float elapsed = 0f;

            while (elapsed < LoadingAnimationDuration)
            {
                elapsed += Time.deltaTime;

                float t = elapsed / LoadingAnimationDuration;
                loagingBarImage.fillAmount = Mathf.Lerp(startFill, 1f, t);

                yield return null;
            }

            loagingBarImage.fillAmount = 1f;

            IsLoadingComplete = true;
            OnLoadingCompleteEvent?.Invoke();
        }

        private void ResetLoadingAnimation()
        {
            IsLoadingComplete = false;

            loagingBarImage.fillAmount = 0f;

            if (currentLoadingCoroutine != null)
            {
                StopCoroutine(currentLoadingCoroutine);
                currentLoadingCoroutine = null;
            }
        }
    }
}