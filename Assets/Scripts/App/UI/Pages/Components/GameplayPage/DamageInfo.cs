using Driver.Base.ObjectPool;
using Driver.Gameplay.Characters;
using System;
using TMPro;
using UnityEngine;

namespace Driver.UI.Pages.Components
{
    public class DamageInfo : MonoBehaviour, IMonoBehaviourObjectPoolItem<DamageInfo.ActivationData>
    {
        public readonly struct ActivationData
        {
            public readonly float Damage;
            public readonly CharacterType CharacterType;
            public readonly Vector2 AnchoredPosition;

            public ActivationData(float damage, CharacterType characterType, Vector2 anchoredPosition)
            {
                Damage = damage;
                CharacterType = characterType;
                AnchoredPosition = anchoredPosition;
            }
        }

        [SerializeField] private TextMeshProUGUI damageText;

        [Header("Lifetime")]
        [SerializeField] private float duration;
        [SerializeField, Range(0f, 1f)] private float fadePortion;

        [Header("Motion")]
        [SerializeField] private float riseSpeed;
        [SerializeField] private float gravity;

        [Header("Scale")]
        [SerializeField] private AnimationCurve scaleCurve;

        [Header("Colors")]
        [SerializeField] private Color enemyColor;
        [SerializeField] private Color carColor;

        private Action requestDeactivationEvent;

        private RectTransform rectTransform;
        private float elapsed;
        private Vector2 position;
        private Vector2 velocity;
        private Color color;

        public void Activate(ActivationData data, Action requestDeactivation)
        {
            this.requestDeactivationEvent = requestDeactivation;

            if (rectTransform == null)
                rectTransform = (RectTransform)transform;

            elapsed = 0f;
            position = data.AnchoredPosition;
            velocity = new Vector2(0, riseSpeed);

            color = data.CharacterType == CharacterType.Car ? carColor : enemyColor;

            damageText.text = Mathf.RoundToInt(data.Damage).ToString();
            damageText.color = color;

            rectTransform.anchoredPosition = position;
            rectTransform.localScale = Vector3.zero;

            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            if (t >= 1f)
            {
                requestDeactivationEvent?.Invoke();
                return;
            }

            velocity.y -= gravity * Time.deltaTime;
            position += velocity * Time.deltaTime;

            rectTransform.anchoredPosition = position;
            rectTransform.localScale = Vector3.one * scaleCurve.Evaluate(t);

            float fadeStart = 1f - fadePortion;
            color.a = t < fadeStart ? 1f : 1f - (t - fadeStart) / fadePortion;
            damageText.color = color;
        }
    }
}