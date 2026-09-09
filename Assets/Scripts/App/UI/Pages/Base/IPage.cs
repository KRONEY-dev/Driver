using Driver.UI.Base;

namespace Driver.UI.Pages
{
    public interface IPage : IUIElement
    {
        void Show(object data);
        void Hide();
    }
}