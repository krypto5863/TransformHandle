using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Renders translation (movement) handles with arrows in either Local or Global space.
    /// </summary>
    public class TranslationHandleRenderer : IProfileAwareRenderer
    {
        // Public constants
        public const float PLANE_SIZE_MULTIPLIER = 0.3f;

        // Alpha values
        private readonly float planeAlpha = 0.1f;         // Transparent when not hovered
        private readonly float planeHoverAlpha = 0.3f;    // More opaque when hovered
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        
        // Sizes
        private readonly float baseThickness = 6f;
        private readonly float hoverThickness = 12f;

        public void Render(Transform target, float scale, int hoveredAxis, float alpha = 1f, HandleSpace handleSpace = HandleSpace.Local)
        {
            Vector3 position = target.position;

            // Render planes FIRST (behind axes)
            RenderPlanes(position, target, scale, hoveredAxis, alpha, handleSpace);

            // Then render axes (on top)
            for (int axis = 0; axis < 3; axis++)
            {
                Vector3 direction = TranslationHandleUtils.GetAxisDirection(target, axis, handleSpace);
                Color color = TranslationHandleUtils.GetAxisColor(axis);
                DrawAxis(position, direction, color, scale, axis, hoveredAxis, alpha);
            }

            DrawCenterPoint(position, scale * 0.1f, alpha);
        }

        public void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile, float alpha = 1f)
        {
            Vector3 position = target.position;

            // First render planes (behind)
            RenderPlanesWithProfile(target, position, scale, hoveredAxis, profile, alpha);

            // Then render axes (on top)
            for (int axis = 0; axis < 3; axis++)
            {
                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (profile.IsAxisEnabled(HandleType.Translation, axis, space))
                    {
                        Vector3 direction = TranslationHandleUtils.GetAxisDirection(target, axis, space);
                        Color color = TranslationHandleUtils.GetAxisColor(axis);
                        DrawAxis(position, direction, color, scale, axis, hoveredAxis, alpha);
                    }
                }
            }

            // Always draw center point
            DrawCenterPoint(position, scale * 0.1f, alpha);
        }

        private void RenderPlanes(Vector3 position, Transform target, float scale, int hoveredAxis, float alpha, HandleSpace space)
        {
            float size = scale * PLANE_SIZE_MULTIPLIER;
            Camera cam = Camera.main;
            Vector3 camForward = cam.transform.forward;

            for (int planeIndex = 4; planeIndex <= 6; planeIndex++)
            {
                var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, planeIndex, space);
                Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, size, camForward);
                Color color = TranslationHandleUtils.GetAxisColor(planeIndex);
                
                DrawPlane(position + offset, axis1, axis2, color, size, planeIndex, hoveredAxis, alpha);
            }
        }

        private void RenderPlanesWithProfile(Transform target, Vector3 position, float scale, 
                                           int hoveredAxis, HandleProfile profile, float alpha)
        {
            float size = scale * PLANE_SIZE_MULTIPLIER;
            Camera cam = Camera.main;
            Vector3 camForward = cam.transform.forward;

            for (int planeIndex = 4; planeIndex <= 6; planeIndex++)
            {
                foreach (HandleSpace space in System.Enum.GetValues(typeof(HandleSpace)))
                {
                    if (profile.IsAxisEnabled(HandleType.Translation, planeIndex, space))
                    {
                        var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, planeIndex, space);
                        Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, size, camForward);
                        Color color = TranslationHandleUtils.GetAxisColor(planeIndex);
                        
                        DrawPlane(position + offset, axis1, axis2, color, size, planeIndex, hoveredAxis, alpha);
                    }
                }
            }
        }

        private void DrawAxis(Vector3 origin, Vector3 direction, Color color, float length,
                              int axisIndex, int hoveredAxis, float alphaMultiplier)
        {
            float alpha = (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            alpha *= alphaMultiplier;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);

            Vector3 endPoint = origin + direction * length;

            float thickness = (hoveredAxis == axisIndex) ? hoverThickness : baseThickness;
            ThickLineHelper.DrawThickLine(origin, endPoint, finalColor, thickness);

            DrawArrowHead(endPoint, direction, finalColor, length * 0.2f);
        }

        private void DrawArrowHead(Vector3 tip, Vector3 direction, Color color, float size)
        {
            Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpendicular1.sqrMagnitude < 0.1f)
                perpendicular1 = Vector3.Cross(direction, Vector3.right).normalized;
            Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;

            Vector3 arrowBase = tip - direction * size;
            float baseSize = size * 0.4f;

            Vector3[] basePoints = new Vector3[]
            {
                arrowBase + perpendicular1 * baseSize,
                arrowBase - perpendicular1 * baseSize,
                arrowBase + perpendicular2 * baseSize,
                arrowBase - perpendicular2 * baseSize
            };

            // Draw filled arrow head
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);
            for (int i = 0; i < 4; i++)
            {
                int i1 = i;
                int i2 = (i + 1) % 4;
                GL.Vertex(tip);
                GL.Vertex(basePoints[i1]);
                GL.Vertex(basePoints[i2]);
            }
            // Base
            GL.Vertex(basePoints[0]); GL.Vertex(basePoints[1]); GL.Vertex(basePoints[2]);
            GL.Vertex(basePoints[1]); GL.Vertex(basePoints[3]); GL.Vertex(basePoints[2]);
            GL.End();

            // Outline
            GL.Begin(GL.LINES);
            GL.Color(color * 0.8f);
            foreach (Vector3 bp in basePoints)
            {
                GL.Vertex(tip);
                GL.Vertex(bp);
            }
            GL.Vertex(basePoints[0]); GL.Vertex(basePoints[1]);
            GL.Vertex(basePoints[1]); GL.Vertex(basePoints[3]);
            GL.Vertex(basePoints[3]); GL.Vertex(basePoints[2]);
            GL.Vertex(basePoints[2]); GL.Vertex(basePoints[0]);
            GL.End();
        }

        private void DrawPlane(Vector3 center, Vector3 axis1, Vector3 axis2, Color color, 
                             float size, int planeIndex, int hoveredAxis, float alphaMultiplier)
        {
            bool isHovered = (hoveredAxis == planeIndex);
            float alpha = isHovered ? planeHoverAlpha : planeAlpha;
            alpha *= alphaMultiplier;

            Color fillColor = new Color(color.r, color.g, color.b, alpha);
            Color outlineColor = new Color(color.r, color.g, color.b, alpha * 2f);

            // Calculate corners - plane is already offset, so corners are relative to center
            Vector3[] corners = new Vector3[4];
            corners[0] = center;
            corners[1] = center - axis1 * size;
            corners[2] = center - axis1 * size - axis2 * size;
            corners[3] = center - axis2 * size;

            // Fill
            GL.Begin(GL.QUADS);
            GL.Color(fillColor);
            GL.Vertex(corners[0]);
            GL.Vertex(corners[1]);
            GL.Vertex(corners[2]);
            GL.Vertex(corners[3]);
            GL.End();

            // Outline
            GL.Begin(GL.LINES);
            GL.Color(outlineColor);
            GL.Vertex(corners[0]); GL.Vertex(corners[1]);
            GL.Vertex(corners[1]); GL.Vertex(corners[2]);
            GL.Vertex(corners[2]); GL.Vertex(corners[3]);
            GL.Vertex(corners[3]); GL.Vertex(corners[0]);
            GL.End();
        }

        private void DrawCenterPoint(Vector3 center, float size, float alpha)
        {
            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.5f * alpha));
            GL.Vertex(center + Vector3.right * size); GL.Vertex(center - Vector3.right * size);
            GL.Vertex(center + Vector3.up * size);    GL.Vertex(center - Vector3.up * size);
            GL.Vertex(center + Vector3.forward * size); GL.Vertex(center - Vector3.forward * size);
            GL.End();
        }
    }
}