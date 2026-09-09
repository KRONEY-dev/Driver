using Driver.UI.Base;

namespace Driver.UI.SurfaceInterfaces
{
    public interface ISurfaceInterface : IUIElement
    {
        public bool IsActive { get; }

        void Show(object data = null);
        void Hide();
    }
}