using System;
using Driver.UI.Pages;
using Driver.UI.Popups;
using Driver.UI.SurfaceInterfaces;
using Driver.Zenject.Installers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Driver.Managers.Interfaces
{
    public interface IUIManager
    {
        event Action OnPageChangedEvent;
        event Action AllPopupsClosed;

        Camera UICamera { get; }

        IPage CurrentPage { get; }

        Transform PagesParent { get; }
        Transform PopupsParent { get; }
        Transform SurfaceInterfaceParent { get; }
        EventSystem EventSystem { get; }

        void Init(GlobalInstaller globalInstaller);

        void AddPage(IPage page);
        void RemovePage(IPage page);

        void AddPopup(IPopup popup);
        void RemovePopup(IPopup popup);

        void AddSurfaceInterface(ISurfaceInterface surfaceInterface);
        void RemoveSurfaceInterface(ISurfaceInterface surfaceInterface);

        void SetPage<T>(object message = null) where T : IPage;
        void SetPreviousPage();
        void ShowSurfaceInterface<T>(object message = null) where T : ISurfaceInterface;
        void HideSurfaceInterface<T>() where T : ISurfaceInterface;
        void OpenPopup<T>(object message = null) where T : IPopup;
        void HidePopup<T>() where T : IPopup;

        void DefaultUIElementVisibilitySwitch(GameObject uiObject, bool isShow, Action callback = null);

        T GetPage<T>() where T : IPage;
        T GetSurfaceInterface<T>() where T : ISurfaceInterface;
        T GetPopup<T>() where T : IPopup;

        void OnPopupClose();
        bool TryHideBlackoutSurfaceInterface();

        bool IsAnyPopupOpen();
    }
}