
namespace Driver.Zenject.Installers
{
    public class MainMenuInstaller : SceneInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            InstallBindingUIComponents();
        }
    }
}