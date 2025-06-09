using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Responsible for detecting which handle axis is being hovered over,
    /// now supporting Local/Global handle space.
    /// </summary>
    public class HandleHoverDetector
    {
        private Camera mainCamera;

        public HandleHoverDetector(Camera camera)
        {
            mainCamera = camera;
        }

        /// <summary>
        /// Returns the index of the hovered axis, or -1 if none.
        /// </summary>
        public int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleType handleType, HandleSpace handleSpace)
        {
            if (target == null || mainCamera == null)
                return -1;

            Vector3 screenCenter = mainCamera.WorldToScreenPoint(target.position);
            if (screenCenter.z < 0f)
                return -1; // Behind camera

            switch (handleType)
            {
                case HandleType.Translation:
                    return GetClosestTranslationAxis(mousePos, target, handleScale, handleSpace);
                case HandleType.Rotation:
                    return GetClosestRotationAxis(mousePos, target, handleScale, handleSpace);
                default:
                    return -1;
            }
        }

        private int GetClosestTranslationAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace)
        {
            float minDist = float.MaxValue;
            int   axis   = -1;

            for (int i = 0; i < 3; i++)
            {
                Vector3 dir = GetAxisDirection(target, i, handleSpace);
                float dist = GetDistanceToAxis(mousePos, target.position, dir, handleScale);
                if (dist < minDist && dist < 10f)
                {
                    minDist = dist;
                    axis    = i;
                }
            }

            return axis;
        }

        private int GetClosestRotationAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace)
        {
            float minDist = float.MaxValue;
            int   axis   = -1;

            for (int i = 0; i < 3; i++)
            {
                Vector3 normal = GetRotationNormal(target, i, handleSpace);
                float dist = GetDistanceToCircle(mousePos, target.position, normal, handleScale);
                if (dist < minDist && dist < 15f)
                {
                    minDist = dist;
                    axis    = i;
                }
            }

            return axis;
        }

        private Vector3 GetAxisDirection(Transform target, int axisIndex, HandleSpace space)
        {
            switch (axisIndex)
            {
                case 0: // X
                    return space == HandleSpace.Local ? target.right   : Vector3.right;
                case 1: // Y
                    return space == HandleSpace.Local ? target.up      : Vector3.up;
                case 2: // Z
                    return space == HandleSpace.Local ? target.forward : Vector3.forward;
                default:
                    return Vector3.zero;
            }
        }

        private Vector3 GetRotationNormal(Transform target, int axisIndex, HandleSpace space)
        {
            switch (axisIndex)
            {
                case 0: // X-rotation circle lies in YZ plane, so normal is X direction
                    return GetAxisDirection(target, 0, space);
                case 1: // Y-rotation normal
                    return GetAxisDirection(target, 1, space);
                case 2: // Z-rotation normal
                    return GetAxisDirection(target, 2, space);
                default:
                    return Vector3.up;
            }
        }

        private float GetDistanceToAxis(Vector2 mousePos, Vector3 origin, Vector3 direction, float scale)
        {
            Vector3 end = origin + direction * scale;
            Vector3 s0 = mainCamera.WorldToScreenPoint(origin);
            Vector3 s1 = mainCamera.WorldToScreenPoint(end);

            if (s0.z < 0f || s1.z < 0f)
                return float.MaxValue;

            return DistancePointToLineSegment(mousePos, new Vector2(s0.x, s0.y), new Vector2(s1.x, s1.y));
        }

        private float GetDistanceToCircle(Vector2 mousePos, Vector3 center, Vector3 normal, float radius)
        {
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 t1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (t1.sqrMagnitude < 0.1f)
                t1 = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 t2 = Vector3.Cross(normal, t1).normalized;

            float minDist = float.MaxValue;
            int   segs    = 64;

            for (int i = 0; i < segs; i++)
            {
                float ang = i / (float)segs * Mathf.PI * 2f;
                Vector3 dir = t1 * Mathf.Cos(ang) + t2 * Mathf.Sin(ang);

                // Only front-facing
                if (Vector3.Dot(dir, camForward) < 0f)
                {
                    Vector3 worldPt = center + dir * radius;
                    Vector3 sp = mainCamera.WorldToScreenPoint(worldPt);
                    if (sp.z > 0f)
                    {
                        float d = Vector2.Distance(mousePos, new Vector2(sp.x, sp.y));
                        if (d < minDist)
                            minDist = d;
                    }
                }
            }

            return minDist;
        }

        private float DistancePointToLineSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 v = b - a;
            float len = v.magnitude;
            if (len < 0.001f) return Vector2.Distance(p, a);
            float t = Mathf.Clamp01(Vector2.Dot(p - a, v) / (len * len));
            Vector2 proj = a + v * t;
            return Vector2.Distance(p, proj);
        }
    }
}
