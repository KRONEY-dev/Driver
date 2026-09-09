using Driver.Data.Scriptable;
using Driver.Extensions;
using Driver.UI.Base;
using Driver.UI.Pages;
using Driver.UI.Popups;
using Driver.UI.SurfaceInterfaces;
using Driver.Zenject.Installers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using Driver.Managers.Interfaces;

namespace Driver.Managers
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        public event Action OnPageChangedEvent;
        public event Action AllPopupsClosed;

        [SerializeField] private UIData uIData;

        public Camera UICamera => uiCamera;
        [SerializeField] private Camera uiCamera;

        public Transform PagesParent => pagesParent;
        [SerializeField] private Transform pagesParent;

        public Transform SurfaceInterfaceParent => surfaceInterfaceParent;
        [SerializeField] private Transform surfaceInterfaceParent;
        [SerializeField] private BaseSurfaceInterface inputLockSurfaceInterface;

        public Transform PopupsParent => popupsParent;
        [SerializeField] private Transform popupsParent;

        public EventSystem EventSystem => eventSystem;
        [SerializeField] private EventSystem eventSystem;

        public IPage CurrentPage { get; private set; }

        private List<IPage> pages;
        private List<ISurfaceInterface> surfaceInterfaces;
        private List<IPopup> popups;

        private IPage previousPage;

        [Inject]
        public void Construct()
        {
            pages = new List<IPage>();

            EventSystem.enabled = true;
        }

        public void Init(GlobalInstaller globalInstaller)
        {
            UIElementsInit(globalInstaller);
        }

        public void SetPage<T>(object message = null) where T : IPage
        {
            SetPageBase(typeof(T), message: message);
        }

        public void SetPreviousPage()
        {
            if (previousPage != null)
            {
                SetPageBase(previousPage.GetType());
            }
        }

        public void ShowSurfaceInterface<T>(object message = null) where T : ISurfaceInterface
        {
            if (surfaceInterfaces.Find(it => it.GetType() == typeof(T)).IsActive)
                return;

            foreach (ISurfaceInterface surfaceInterface in surfaceInterfaces)
            {
                if (surfaceInterface is T)
                {
                    surfaceInterface.Show(message);
                    break;
                }
            }
        }

        public void HideSurfaceInterface<T>() where T : ISurfaceInterface
        {
            if (!surfaceInterfaces.Find(it => it.GetType() == typeof(T)).IsActive)
                return;

            foreach (ISurfaceInterface surfaceInterface in surfaceInterfaces)
            {
                if (surfaceInterface is T)
                {
                    surfaceInterface.Hide();
                    break;
                }
            }
        }

        public void OpenPopup<T>(object message = null) where T : IPopup
        {
            var popupType = typeof(T);

            Debug.Log($"OpenPopup {popupType}");

            IPopup popup =  popups.Find(it => it.GetType() == popupType);
            if (popup == null)
            {
                Debug.LogError("Popup isn't exists");
                return;
            }
            else if (popup.IsActive)
            {
                Debug.LogError($"Popup {popup.GetType().Name} already active");
                return;
            }

            popup.Show(message);
        }

        public void HidePopup<T>() where T : IPopup
        {
            if (!popups.Find(it => it.GetType() == typeof(T)).IsActive)
                return;

            foreach (IPopup popup in popups)
            {
                if (popup is T)
                {
                    popup.Hide();
                    break;
                }
            }
        }

        public T GetPage<T>() where T : IPage
        {
            return GetUIElement<IPage, T>(pages);
        }

        public T GetSurfaceInterface<T>() where T : ISurfaceInterface
        {
            return GetUIElement<ISurfaceInterface, T>(surfaceInterfaces);
        }

        public T GetPopup<T>() where T : IPopup
        {
            return GetUIElement<IPopup, T>(popups);
        }

        public void OnPopupClose()
        {
            if (TryHideBlackoutSurfaceInterface())
            {
                AllPopupsClosed?.Invoke();
            }
        }

        public bool TryHideBlackoutSurfaceInterface()
        {
            if (IsAnyPopupOpen())
                return false;

            HideSurfaceInterface<BlackoutSurfaceInterface>();
            return true;
        }

        public bool IsAnyPopupOpen()
        {
            foreach (var popup in popups)
            {
                if (popup.IsActive)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddPage(IPage page)
        {
            page.Init();
            pages.Add(page);
        }

        public void RemovePage(IPage page)
        {
            page.Dispose();
            pages.Remove(page);

            if (CurrentPage == page)
            {
                CurrentPage = null;
            }
        }

        public void AddPopup(IPopup popup)
        {
            popup.Init();
            popups.Add(popup);
        }

        public void RemovePopup(IPopup popup)
        {
            popup.Dispose();
            popups.Remove(popup);
        }

        public void AddSurfaceInterface(ISurfaceInterface surfaceInterface)
        {
            surfaceInterfaces.Add(surfaceInterface);

            surfaceInterface.Init();
        }

        public void RemoveSurfaceInterface(ISurfaceInterface surfaceInterface)
        {
            surfaceInterfaces.Remove(surfaceInterface);
        }

        public void DefaultUIElementVisibilitySwitch(GameObject uiObject, bool isShow, Action callback = null)
        {
            uiObject.SetActive(isShow);
            callback?.Invoke();
        }

        private void SetPageBase(Type pageType, object message = null)
        {
            Debug.Log($"SetPage {pageType}");

            bool isCurrentPageExist = CurrentPage != null;

            if (isCurrentPageExist && CurrentPage.GetType() == pageType)
            {
                Debug.LogError("Page already active");
                return;
            }

            IPage nextPage;
            if (TryLoadNextPage())
            {
                if (isCurrentPageExist)
                {
                    CurrentPage.Hide();

                    previousPage = CurrentPage;
                }

                CurrentPage = nextPage;

                OnPageChangedEvent?.Invoke();
            }
            else
            {
                Debug.LogError("Page isn't exists");
                return;
            }

            bool TryLoadNextPage()
            {
                Type tempPageType = null;
                foreach (IPage page in pages)
                {
                    tempPageType = page.GetType();
                    if (tempPageType == pageType)
                    {
                        nextPage = page;

                        page.Show(message);

                        return true;
                    }
                }

                nextPage = null;
                return false;
            }
        }

        private void UIElementsInit(GlobalInstaller globalInstaller)
        {
            surfaceInterfaces = new List<ISurfaceInterface>();
            for (int i = 0; i < uIData.SurfaceInterfaces.Length; i++)
            {
                globalInstaller.BindFromPrefab<ISurfaceInterface>(uIData.SurfaceInterfaces[i], surfaceInterfaceParent, (surfaceInterface) =>
                {
                    surfaceInterface.Init();
                    surfaceInterfaces.Add(surfaceInterface);
                });
            }
            inputLockSurfaceInterface.Init();
            surfaceInterfaces.Add(inputLockSurfaceInterface);

            popups = new List<IPopup>();
            for (int i = 0; i < uIData.Popups.Length; i++)
            {
                globalInstaller.BindFromPrefab<IPopup>(uIData.Popups[i], popupsParent, (popup) =>
                {
                    popup.Init();
                    popups.Add(popup);
                });
            }
        }

        private I GetUIElement<T, I>(List<T> uIElementsList) where T : IUIElement
        {
            foreach (var uIElementItem in uIElementsList)
            {
                if (uIElementItem is I item)
                {
                    return item;
                }
            }

            return default;
        }
    }
}