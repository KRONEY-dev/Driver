using UnityEngine;

namespace Driver.Extensions
{
    public static class CameraExtensions
    {
        public static bool IsInView(this Camera camera, Vector3 worldPosition)
        {
            if (camera == null)
                return true;

            Vector3 viewportPoint = camera.WorldToViewportPoint(worldPosition);

            return viewportPoint.z > 0f
                && viewportPoint.x is >= 0f and <= 1f
                && viewportPoint.y is >= 0f and <= 1f;
        }
    }
}