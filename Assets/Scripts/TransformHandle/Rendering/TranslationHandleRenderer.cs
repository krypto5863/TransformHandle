using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Renders translation (movement) handles with arrows in either Local or Global space.
    /// </summary>
    public class TranslationHandleRenderer : IProfileAwareRenderer
    {
        private readonly Color xAxisColor = Color.red;
        private readonly Color yAxisColor = Color.green;
        private readonly Color zAxisColor = Color.blue;
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        private readonly float baseThickness = 6f;
        private readonly float hoverThickness = 12f;

        public void Render(Transform target, float scale, int hoveredAxis, float alpha = 1f, HandleSpace handleSpace = HandleSpace.Local)
        {
            Vector3 position = target.position;

            Vector3 dirX = (handleSpace == HandleSpace.Local) ? target.right   : Vector3.right;
            Vector3 dirY = (handleSpace == HandleSpace.Local) ? target.up      : Vector3.up;
            Vector3 dirZ = (handleSpace == HandleSpace.Local) ? target.forward : Vector3.forward;

            DrawAxis(position, dirX, xAxisColor, scale, 0, hoveredAxis, alpha);
            DrawAxis(position, dirY, yAxisColor, scale, 1, hoveredAxis, alpha);
            DrawAxis(position, dirZ, zAxisColor, scale, 2, hoveredAxis, alpha);

            DrawCenterPoint(position, scale * 0.1f, alpha);
        }

        public void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile, float alpha = 1f)
        {
            Vector3 position = target.position;

            // Render each axis based on profile settings
            for (int axis = 0; axis < 3; axis++)
            {
                Color color = GetAxisColor(axis);

                // Check local space
                if (profile.IsAxisEnabled(HandleType.Translation, axis, HandleSpace.Local))
                {
                    Vector3 direction = GetLocalAxisDirection(target, axis);
                    DrawAxis(position, direction, color, scale, axis, hoveredAxis, alpha);
                }

                // Check global space
                if (profile.IsAxisEnabled(HandleType.Translation, axis, HandleSpace.Global))
                {
                    Vector3 direction = GetGlobalAxisDirection(axis);
                    DrawAxis(position, direction, color, scale, axis, hoveredAxis, alpha);
                }
            }

            // Always draw center point
            DrawCenterPoint(position, scale * 0.1f, alpha);
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

        private void DrawCenterPoint(Vector3 center, float size, float alpha)
        {
            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.5f * alpha));
            GL.Vertex(center + Vector3.right * size); GL.Vertex(center - Vector3.right * size);
            GL.Vertex(center + Vector3.up * size);    GL.Vertex(center - Vector3.up * size);
            GL.Vertex(center + Vector3.forward * size); GL.Vertex(center - Vector3.forward * size);
            GL.End();
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