using System.Collections.Generic;
using Driver.Managers.Interfaces;
using Driver.UI.Pages;
using Driver.UI.Popups;
using Driver.UI.SurfaceInterfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace Driver.Zenject.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private List<Canvas> canvases;

        [SerializeField] private List<BasePage> basePages;
        [SerializeField] private List<BasePopup> basePopups;
        [SerializeField] private List<BaseSurfaceInterface> surfaceInterfaces;
        [SerializeField] private int intitializationOrderForUI;

        public override void InstallBindings()
        {
            Camera mainUICamera = Container.Resolve<IUIManager>().UICamera;

            canvases.ForEach(it => it.worldCamera = mainUICamera);

            StackUiCamera(mainUICamera);
        }

        private void StackUiCamera(Camera uiCamera)
        {
            if (sceneCamera == null || uiCamera == null)
                return;

            uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;

            List<Camera> stack = sceneCamera.GetUniversalAdditionalCameraData().cameraStack;
            if (!stack.Contains(uiCamera))
                stack.Add(uiCamera);
        }

        protected virtual void InstallBindingUIComponents()
        {
            InstallBindingPages();
            InstallBindingPopups();
            InstallBindingSurfaceInterfaces();
        }

        protected virtual void InstallBindingPages()
        {
            foreach (BasePage baseUI in basePages)
            {
                Container.Bind<IInitializable>()
                         .FromInstance(baseUI)
                         .AsTransient()
                         .NonLazy();

                Container.BindInitializableExecutionOrder(baseUI.GetType(), intitializationOrderForUI);
            }
        }

        protected virtual void InstallBindingPopups()
        {
            foreach (BasePopup baseUI in basePopups)
            {
                Container.Bind<IInitializable>()
                         .FromInstance(baseUI)
                         .AsTransient()
                         .NonLazy();

                Container.BindInitializableExecutionOrder(baseUI.GetType(), intitializationOrderForUI);
            }
        }

        protected virtual void InstallBindingSurfaceInterfaces()
        {
            foreach (BaseSurfaceInterface surfaceInterfaces in surfaceInterfaces)
            {
                Container.Bind<IInitializable>()
                         .FromInstance(surfaceInterfaces)
                         .AsTransient()
                         .NonLazy();

                Container.BindInitializableExecutionOrder(surfaceInterfaces.GetType(), intitializationOrderForUI);
            }
        }
    }
}