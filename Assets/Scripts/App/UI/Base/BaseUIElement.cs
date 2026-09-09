using Driver.Managers.Interfaces;
using UnityEngine;
using Zenject;

namespace Driver.UI.Base
{
    [DisallowMultipleComponent]
    public abstract class BaseUIElement : MonoBehaviour
    {
        public abstract Transform Parent { get; }

        protected virtual bool IsUniqueShowHide => false;

        [SerializeField] protected bool IsLocal;

        [Inject] protected IUIManager UIManager;
        [Inject] protected IDataManager DataManager;

        protected Canvas ParentCanvas { get; private set; }
        protected RectTransform ParentCanvasRectTransform { get; private set; }

        protected void BaseInit()
        {
            gameObject.SetActive(false);

            ParentCanvas = GetComponentInParent<Canvas>();
            ParentCanvasRectTransform = ParentCanvas != null ? ParentCanvas.GetComponent<RectTransform>() : null;
        }

        protected virtual void OnDestroy() { }
    }
}