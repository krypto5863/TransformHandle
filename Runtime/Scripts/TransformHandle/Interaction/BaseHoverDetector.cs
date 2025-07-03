using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Base class for handle hover detection with common functionality
    /// </summary>
    public abstract class BaseHoverDetector
    {
        protected Camera mainCamera;
        protected const float TRANSLATION_THRESHOLD = 10f;
        protected const float ROTATION_THRESHOLD = 15f;
        protected const float SCALE_THRESHOLD = 20f;

        public BaseHoverDetector(Camera camera)
        {
            mainCamera = camera;
        }

        public abstract int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace);
        public abstract int GetHoveredAxisWithProfile(Vector2 mousePos, Transform target, float handleScale, HandleProfile profile);

        protected Vector3 GetAxisDirection(Transform target, int axisIndex, HandleSpace space)
        {
            switch (axisIndex)
            {
                case 0: return space == HandleSpace.Local ? target.right : Vector3.right;
                case 1: return space == HandleSpace.Local ? target.up : Vector3.up;
                case 2: return space == HandleSpace.Local ? target.forward : Vector3.forward;
                default: return Vector3.zero;
            }
        }

        protected float DistancePointToLineSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 v = b - a;
            float len = v.magnitude;
            if (len < 0.001f) return Vector2.Distance(p, a);
            float t = Mathf.Clamp01(Vector2.Dot(p - a, v) / (len * len));
            Vector2 proj = a + v * t;
            return Vector2.Distance(p, proj);
        }

        protected bool IsPointBehindCamera(Vector3 worldPoint)
        {
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);
            return screenPoint.z < 0f;
        }
    }
}