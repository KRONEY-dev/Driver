using Driver.UI.Popups;
using Driver.UI.SurfaceInterfaces;
using UnityEngine;

namespace Driver.Data.Scriptable
{
    [CreateAssetMenu(fileName = "UIData", menuName = "ScriptableObjects/UIData", order = 0)]
    public class UIData : ScriptableObject
    {
        [SerializeField] public BaseSurfaceInterface[] SurfaceInterfaces;
        [SerializeField] public BasePopup[] Popups;
    }
}