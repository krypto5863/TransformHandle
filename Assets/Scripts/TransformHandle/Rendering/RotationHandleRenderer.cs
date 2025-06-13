using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Renders rotation handles with circles in either Local or Global space.
    /// </summary>
    public class RotationHandleRenderer : IProfileAwareRenderer
    {
        private readonly Color xAxisColor = Color.red;
        private readonly Color yAxisColor = Color.green;
        private readonly Color zAxisColor = Color.blue;
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        private readonly int circleSegments = 64;
        private readonly float baseThickness = 6f;
        private readonly float hoverThickness = 12f;

        public void Render(Transform target, float scale, int hoveredAxis, float alpha = 1f, HandleSpace handleSpace = HandleSpace.Local)
        {
            Vector3 position = target.position;
            Camera camera = Camera.main;

            Vector3 dirX = (handleSpace == HandleSpace.Local) ? target.right   : Vector3.right;
            Vector3 dirY = (handleSpace == HandleSpace.Local) ? target.up      : Vector3.up;
            Vector3 dirZ = (handleSpace == HandleSpace.Local) ? target.forward : Vector3.forward;

            // X-axis rotation circle
            DrawRotationCircle(position, dirX, xAxisColor, scale, 0, hoveredAxis, alpha, camera);
            
            // Y-axis rotation circle
            DrawRotationCircle(position, dirY, yAxisColor, scale, 1, hoveredAxis, alpha, camera);
            
            // Z-axis rotation circle
            DrawRotationCircle(position, dirZ, zAxisColor, scale, 2, hoveredAxis, alpha, camera);

            // Free rotation sphere
            DrawCameraFacingCircle(position, scale * 1.2f, alpha, camera);
        }

        public void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile, float alpha = 1f)
        {
            Vector3 position = target.position;
            Camera camera = Camera.main;

            // Render each axis based on profile settings
            for (int axis = 0; axis < 3; axis++)
            {
                Color color = GetAxisColor(axis);
                bool isHovered = (hoveredAxis == axis);

                // Check local space
                if (profile.IsAxisEnabled(HandleType.Rotation, axis, HandleSpace.Local))
                {
                    Vector3 normal = GetLocalAxisDirection(target, axis);
                    DrawRotationCircle(position, normal, color, scale, axis, hoveredAxis, alpha, camera);
                }

                // Check global space  
                if (profile.IsAxisEnabled(HandleType.Rotation, axis, HandleSpace.Global))
                {
                    Vector3 normal = GetGlobalAxisDirection(axis);
                    DrawRotationCircle(position, normal, color, scale, axis, hoveredAxis, alpha, camera);
                }
            }

            // Free rotation sphere
            DrawCameraFacingCircle(position, scale * 1.2f, alpha, camera);
        }

        private void DrawRotationCircle(Vector3 center, Vector3 normal, Color color, float radius,
                                        int axisIndex, int hoveredAxis, float alphaMultiplier, Camera camera)
        {
            float alpha = (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            alpha *= alphaMultiplier;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);

            // Skip if circle is edge-on
            Vector3 toCamera = (camera.transform.position - center).normalized;
            float edgeDot = Mathf.Abs(Vector3.Dot(normal, toCamera));
            if (edgeDot > 0.98f)
                return;

            // Determine tangent basis
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.sqrMagnitude < 0.1f)
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;

            float thickness = (hoveredAxis == axisIndex) ? hoverThickness : baseThickness;

            // Draw circle segments using ThickLineHelper
            for (int i = 0; i < circleSegments; i++)
            {
                float angleA = (i / (float)circleSegments) * 2f * Mathf.PI;
                float angleB = ((i + 1) / (float)circleSegments) * 2f * Mathf.PI;

                Vector3 pA = center + (tangent1 * Mathf.Cos(angleA) + tangent2 * Mathf.Sin(angleA)) * radius;
                Vector3 pB = center + (tangent1 * Mathf.Cos(angleB) + tangent2 * Mathf.Sin(angleB)) * radius;

                // Visibility check
                Vector3 mid = (pA + pB) * 0.5f;
                float dotMid = Vector3.Dot((mid - center).normalized, toCamera);
                if (dotMid < -0.1f)
                    continue;

                // Fade based on angle to camera
                float fade = Mathf.Clamp01((dotMid + 0.1f) / 0.2f);
                Color segmentColor = new Color(finalColor.r, finalColor.g, finalColor.b, finalColor.a * fade);
                
                // Use ThickLineHelper for consistent thickness
                ThickLineHelper.DrawThickLine(pA, pB, segmentColor, thickness);
            }
        }

        private void DrawCameraFacingCircle(Vector3 center, float radius, float alpha, Camera camera)
        {
            Vector3 normal = (camera.transform.position - center).normalized;
            float thickness = baseThickness * 0.8f;
            Color color = new Color(1f, 1f, 1f, 0.3f * alpha);
            ThickLineHelper.DrawThickCircle(center, normal, radius, color, circleSegments, thickness);
        }

        private Vector3 GetLocalAxisDirection(Transform target, int axis)
        {
            switch (axis)
            {
                case 0: return target.right;
                case 1: return target.up;
                case 2: return target.forward;
                default: return Vector3.zero;
            }
        }

        private Vector3 GetGlobalAxisDirection(int axis)
        {
            switch (axis)
            {
                case 0: return Vector3.right;
                case 1: return Vector3.up;
                case 2: return Vector3.forward;
                default: return Vector3.zero;
            }
        }

        private Color GetAxisColor(int axis)
        {
            switch (axis)
            {
                case 0: return Color.red;
                case 1: return Color.green;
                case 2: return Color.blue;
                default: return Color.white;
            }
        }
    }
}