using Driver.UI.Base;

namespace Driver.UI.Popups
{
    public interface IPopup : IUIElement
    {
        public bool IsActive { get; }

        void Show(object data = null);
        void Hide();
    }
}