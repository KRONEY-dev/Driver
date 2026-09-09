using Driver.UI.Base;
using Driver.UI.SurfaceInterfaces;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Driver.UI.Popups
{
    public abstract class BasePopup : BaseUIElement, IPopup, IInitializable
    {
        public bool IsActive { get; private set; }

        public override Transform Parent => UIManager.PopupsParent;

        [SerializeField] protected Button CloseButton;
        [SerializeField] protected Button OutsideCloseButton;

        private bool isOutsideCloseButtonExist;

        public void Initialize()
        {
            if (IsLocal)
            {
                UIManager.AddPopup(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsLocal)
            {
                UIManager.RemovePopup(this);
            }
        }

        public virtual void Init()
        {
            BaseInit();

            isOutsideCloseButtonExist = OutsideCloseButton != null;

            CloseButton?.onClick.AddListener(CloseButtonOnClickHandler);
            OutsideCloseButton?.onClick.AddListener(OutsideCloseButtonOnClickHandler);
        }

        public virtual void Show(object data = null)
        {
            IsActive = true;

            transform.SetAsLastSibling();

            UIManager.ShowSurfaceInterface<BlackoutSurfaceInterface>();

            if (!IsUniqueShowHide)
            {
                BaseShowHideAnimation(true);
            }
        }

        public virtual void Hide()
        {
            IsActive = false;

            UIManager.OnPopupClose();

            if (!IsUniqueShowHide)
            {
                BaseShowHideAnimation(false);
            }
        }

        public virtual void SetIsActive(bool active)
        {
            IsActive = active;
        }

        public virtual void Dispose() { }

        protected void BaseShowHideAnimation(bool isShow)
        {
            UIManager.DefaultUIElementVisibilitySwitch(gameObject, isShow, isShow ? OnPopupCompletelyShown : OnPopupCompletelyHidden);
        }

        protected virtual void UpdateCloseButtonsInteractable(bool interactable)
        {
            CloseButton.interactable = interactable;

            if (isOutsideCloseButtonExist)
            {
                OutsideCloseButton.interactable = interactable;
            }
        }

        protected virtual void UpdateCloseButtonsActive(bool active)
        {
            CloseButton.gameObject.SetActive(active);

            if (isOutsideCloseButtonExist)
            {
                OutsideCloseButton.interactable = active;
            }
        }

        protected virtual void CloseButtonOnClickHandler()
        {
            Hide();
        }
        protected virtual void OutsideCloseButtonOnClickHandler()
        {
            CloseButtonOnClickHandler();
        }

        protected virtual void OnPopupCompletelyShown() { }
        protected virtual void OnPopupCompletelyHidden() { }
    }
}