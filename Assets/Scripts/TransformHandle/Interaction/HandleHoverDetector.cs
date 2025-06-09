using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Responsible for detecting which handle axis is being hovered over
    /// </summary>
    public class HandleHoverDetector
    {
        private Camera mainCamera;

        public HandleHoverDetector(Camera camera)
        {
            mainCamera = camera;
        }

        public int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleType handleType)
        {
            if (target == null || mainCamera == null) return -1;

            Vector3 handleScreenPos = mainCamera.WorldToScreenPoint(target.position);
            if (handleScreenPos.z < 0) return -1; // Behind camera

            switch (handleType)
            {
                case HandleType.Translation:
                    return GetClosestTranslationAxis(mousePos, target, handleScale);
                case HandleType.Rotation:
                    return GetClosestRotationAxis(mousePos, target, handleScale);
                default:
                    return -1;
            }
        }

        private int GetClosestTranslationAxis(Vector2 mousePos, Transform target, float handleScale)
        {
            float closestDistance = float.MaxValue;
            int closestAxis = -1;

            Vector3[] axisDirections = {
                target.right,    // X
                target.up,       // Y
                target.forward   // Z
            };

            for (int i = 0; i < 3; i++)
            {
                float distance = GetDistanceToAxis(mousePos, target.position, axisDirections[i], handleScale);
                if (distance < closestDistance && distance < 10f) // 10 pixel threshold
                {
                    closestDistance = distance;
                    closestAxis = i;
                }
            }

            return closestAxis;
        }

        private float GetDistanceToAxis(Vector2 mousePos, Vector3 origin, Vector3 axisDirection, float handleScale)
        {
            Vector3 endPoint = origin + axisDirection * handleScale;

            Vector3 screenOrigin = mainCamera.WorldToScreenPoint(origin);
            Vector3 screenEnd = mainCamera.WorldToScreenPoint(endPoint);

            if (screenOrigin.z < 0 || screenEnd.z < 0) return float.MaxValue;

            Vector2 lineStart = new Vector2(screenOrigin.x, screenOrigin.y);
            Vector2 lineEnd = new Vector2(screenEnd.x, screenEnd.y);

            return DistancePointToLineSegment(mousePos, lineStart, lineEnd);
        }

        private int GetClosestRotationAxis(Vector2 mousePos, Transform target, float handleScale)
        {
            float closestDistance = float.MaxValue;
            int closestAxis = -1;

            Vector3[] axisNormals = {
                target.forward,  // X rotation (red circle)
                target.right,    // Y rotation (green circle)
                target.up        // Z rotation (blue circle)
            };

            for (int i = 0; i < 3; i++)
            {
                float distance = GetDistanceToCircle(mousePos, target.position, axisNormals[i], handleScale);
                if (distance < closestDistance && distance < 15f) // 15 pixel threshold for circles
                {
                    closestDistance = distance;
                    closestAxis = i;
                }
            }

            return closestAxis;
        }

        private float GetDistanceToCircle(Vector2 mousePos, Vector3 center, Vector3 normal, float radius)
        {
            // compute two orthonormal tangent directions in world space
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.magnitude < 0.1f)
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;

            float minDistance = float.MaxValue;
            int segments = 64;
            Vector3 camForward = mainCamera.transform.forward;

            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * 2 * Mathf.PI;
                Vector3 dirOnPlane = tangent1 * Mathf.Cos(angle) + tangent2 * Mathf.Sin(angle);
                Vector3 worldPoint = center + dirOnPlane * radius;

                // Only keep points on the front-facing half of the ellipse
                if (Vector3.Dot(dirOnPlane, camForward) >= 0f)
                    continue;

                // project to screen space and skip if behind camera
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);
                if (screenPoint.z <= 0f)
                    continue;

                // measure distance to mouse position
                float dist = Vector2.Distance(mousePos, new Vector2(screenPoint.x, screenPoint.y));
                if (dist < minDistance)
                    minDistance = dist;
            }

            return minDistance;
        }


        private float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.magnitude;
            if (lineLength < 0.001f) return Vector2.Distance(point, lineStart);

            float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, line) / (lineLength * lineLength));
            Vector2 projection = lineStart + t * line;

            return Vector2.Distance(point, projection);
        }
    }
}