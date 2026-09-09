using Driver.UI.Base;
using UnityEngine;
using Zenject;

namespace Driver.UI.SurfaceInterfaces
{
    public class BaseSurfaceInterface : BaseUIElement, ISurfaceInterface, IInitializable
    {
        public bool IsActive { get; private set; }

        public override Transform Parent => UIManager.SurfaceInterfaceParent;

        public void Initialize()
        {
            if (IsLocal)
            {
                UIManager.AddSurfaceInterface(this);
            }
        }

        protected override void OnDestroy()
        {
            if (IsLocal)
            {
                base.OnDestroy();

                UIManager.RemoveSurfaceInterface(this);
            }
        }

        public virtual void Init()
        {
            BaseInit();
        }

        public virtual void Show(object data)
        {
            IsActive = true;

            if (!IsUniqueShowHide)
            {
                UIManager.DefaultUIElementVisibilitySwitch(gameObject, true);
            }
        }

        public virtual void Hide()
        {
            IsActive = false;

            if (!IsUniqueShowHide)
            {
                UIManager.DefaultUIElementVisibilitySwitch(gameObject, false);
            }
        }

        public virtual void Dispose() { }
    }
}