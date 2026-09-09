using System.Threading;
using Cysharp.Threading.Tasks;
using Driver.Base.ObjectPool;
using Driver.Extensions;
using Driver.Gameplay.Characters;
using Driver.UI.Pages.Components;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Driver.UI.Pages
{
    public class GameplayPage : BasePage
    {
        [SerializeField] private GameObject hudObject;
        [SerializeField] private GameObject startHintObject;

        [Header("Health bar")]
        [SerializeField] private RectTransform healthBarTransform;
        [SerializeField] private Image healthBarImage;

        [Header("Level progress")]
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private float progressDistanceDivider;

        [Header("Damage numbers")]
        [SerializeField] private DamageInfo damageInfoPrefab;
        [SerializeField] private Transform damageInfosParent;
        [SerializeField] private int damageInfosPoolSize;

        private MonoBehaviourObjectPool<DamageInfo, DamageInfo.ActivationData> damageInfoPool;

        public override void Init()
        {
            base.Init();

            damageInfoPool = new MonoBehaviourObjectPool<DamageInfo, DamageInfo.ActivationData>(
                new MonoBehaviourPoolInitData(damageInfoPrefab.gameObject, damageInfosPoolSize, damageInfosParent));
        }

        public void SetHudVisible(bool visible)
        {
            if (hudObject != null)
                hudObject.SetActive(visible);
        }

        public async UniTask WaitForStartTapAsync(CancellationToken token)
        {
            SetStartHintActive(true);

            await UniTask.WaitUntil(WasTapped, cancellationToken: token);

            SetStartHintActive(false);

            static bool WasTapped()
            {
                Pointer pointer = Pointer.current;

                return pointer != null && pointer.press.wasPressedThisFrame;
            }
        }

        public void ResetHud()
        {
            healthBarImage.fillAmount = 1f;
            progressFillImage.fillAmount = 0f;
            progressText.text = "0m";
        }

        public void SetHealthNormalized(float normalized)
        {
            healthBarImage.fillAmount = Mathf.Clamp01(normalized);
        }

        public void SetHealthBarViewportPosition(Vector3 viewportPosition)
        {
            healthBarTransform.anchoredPosition = ParentCanvasRectTransform.ViewportToCanvas(viewportPosition);
        }

        public void SetProgress(float progressByDistance, float distanceMeters)
        {
            progressFillImage.fillAmount = progressByDistance;
            progressText.text = $"{Mathf.RoundToInt(distanceMeters / progressDistanceDivider)}m";

            RectTransform fill = progressFillImage.rectTransform;
            float fillTopY = fill.localPosition.y - fill.rect.height * 0.5f + fill.rect.height * progressByDistance;

            RectTransform text = progressText.rectTransform;
            text.anchoredPosition = new Vector2(text.anchoredPosition.x, fillTopY);
        }

        public void ShowDamageInfo(float damage, CharacterType characterType, Vector3 viewportPosition)
        {
            Vector2 anchoredPosition = ParentCanvasRectTransform.ViewportToCanvas(viewportPosition);

            damageInfoPool.ReleseObject(new DamageInfo.ActivationData(damage, characterType, anchoredPosition));
        }

        private void SetStartHintActive(bool active)
        {
            if (startHintObject != null)
            {
                startHintObject.SetActive(active);
            }
        }
    }
}