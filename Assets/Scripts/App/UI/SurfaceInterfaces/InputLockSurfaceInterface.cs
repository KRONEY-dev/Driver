namespace Driver.UI.SurfaceInterfaces
{
    public class InputLockSurfaceInterface : BaseSurfaceInterface
    {
        protected override bool IsUniqueShowHide => true;

        public override void Show(object data)
        {
            base.Show(data);

            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            base.Hide();

            gameObject.SetActive(false);
        }
    }
}