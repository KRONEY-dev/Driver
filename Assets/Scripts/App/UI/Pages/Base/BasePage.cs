using Driver.UI.Base;
using UnityEngine;
using Zenject;

namespace Driver.UI.Pages
{
    public class BasePage : BaseUIElement, IPage, IInitializable
    {
        public override Transform Parent => UIManager.PagesParent;
        
        public void Initialize()
        {
            if (IsLocal)
            {
                UIManager.AddPage(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsLocal)
            {
                UIManager.RemovePage(this);
            }
        }

        public virtual void Init()
        {
            BaseInit();
        }

        public virtual void Show(object data)
        {
            if (!IsUniqueShowHide)
            {
                UIManager.DefaultUIElementVisibilitySwitch(gameObject, true, callback: OnPageShownHandler);
            }
        }
        protected virtual void OnPageShownHandler() { }

        public virtual void Hide()
        {
            if (!IsUniqueShowHide)
            {
                if (UIManager != null && gameObject != null)
                {
                    UIManager.DefaultUIElementVisibilitySwitch(gameObject, false, callback: OnPageHiddenHandler);
                }
            }
        }
        protected virtual void OnPageHiddenHandler() { }

        public virtual void Dispose() { }
    }
}