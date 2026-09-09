using UnityEngine;

namespace Driver.Extensions
{
    public static class UIExtensions
    {
        public static Vector2 ViewportToCanvas(this RectTransform canvasRectTransform, Vector3 viewportPosition)
        {
            return new Vector2(
                (viewportPosition.x - 0.5f) * canvasRectTransform.sizeDelta.x,
                (viewportPosition.y - 0.5f) * canvasRectTransform.sizeDelta.y);
        }
    }
}