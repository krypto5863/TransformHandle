using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Renders scale handles with boxes at axis ends and a center handle for uniform scaling
    /// </summary>
    public class ScaleHandleRenderer : IProfileAwareRenderer
    {
        private readonly Color xAxisColor = Color.red;
        private readonly Color yAxisColor = Color.green;
        private readonly Color zAxisColor = Color.blue;
        private readonly Color centerColor = Color.white;
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        private readonly float baseThickness = 6f;
        private readonly float hoverThickness = 12f;
        private readonly float boxSize = 0.09f;

        public void Render(Transform target, float scale, int hoveredAxis, float alpha = 1f, HandleSpace handleSpace = HandleSpace.Local)
        {
            Vector3 position = target.position;

            // For scale, we always use local axes regardless of handle space setting
            Vector3 dirX = target.right;
            Vector3 dirY = target.up;
            Vector3 dirZ = target.forward;

            // Draw axis lines and boxes
            DrawScaleAxis(position, dirX, xAxisColor, scale, 0, hoveredAxis, alpha);
            DrawScaleAxis(position, dirY, yAxisColor, scale, 1, hoveredAxis, alpha);
            DrawScaleAxis(position, dirZ, zAxisColor, scale, 2, hoveredAxis, alpha);

            // Draw center handle for uniform scaling (axis index 3)
            DrawCenterHandle(position, scale * boxSize * 1.5f, hoveredAxis == 3, alpha);
        }

        public void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile, float alpha = 1f)
        {
            Vector3 position = target.position;

            // Render each axis based on profile settings
            for (int axis = 0; axis < 3; axis++)
            {
                Color color = GetAxisColor(axis);

                // Check local space (most common for scale)
                if (profile.IsAxisEnabled(HandleType.Scale, axis, HandleSpace.Local))
                {
                    Vector3 direction = GetLocalAxisDirection(target, axis);
                    DrawScaleAxis(position, direction, color, scale, axis, hoveredAxis, alpha);
                }

                // Check global space (less common for scale)
                if (profile.IsAxisEnabled(HandleType.Scale, axis, HandleSpace.Global))
                {
                    Vector3 direction = GetGlobalAxisDirection(axis);
                    DrawScaleAxis(position, direction, color, scale, axis, hoveredAxis, alpha);
                }
            }

            // Uniform scale handle (axis index 3)
            if (profile.IsAxisEnabled(HandleType.Scale, 3, HandleSpace.Local) || 
                profile.IsAxisEnabled(HandleType.Scale, 3, HandleSpace.Global))
            {
                DrawCenterHandle(position, scale * boxSize * 1.5f, hoveredAxis == 3, alpha);
            }
        }

        private void DrawScaleAxis(Vector3 origin, Vector3 direction, Color color, float length,
                                   int axisIndex, int hoveredAxis, float alphaMultiplier)
        {
            float alpha = (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            alpha *= alphaMultiplier;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);

            Vector3 endPoint = origin + direction * length;

            // Draw line
            float thickness = (hoveredAxis == axisIndex) ? hoverThickness : baseThickness;
            ThickLineHelper.DrawThickLine(origin, endPoint, finalColor, thickness);

            // Draw box at end
            float currentBoxSize = length * boxSize;
            if (hoveredAxis == axisIndex) currentBoxSize *= 1.3f; // Bigger when hovered
            
            DrawBox(endPoint, direction, currentBoxSize, finalColor);
        }

        private void DrawBox(Vector3 center, Vector3 forward, float size, Color color)
        {
            // Calculate box orientation
            Vector3 up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(forward, up).normalized;
            up = Vector3.Cross(right, forward).normalized;

            // Box corners
            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                float x = (i & 1) == 0 ? -1 : 1;
                float y = (i & 2) == 0 ? -1 : 1;
                float z = (i & 4) == 0 ? -1 : 1;
                corners[i] = center + (right * x + up * y + forward * z) * size * 0.5f;
            }

            // Draw filled faces FIRST (so they appear behind the edges)
            DrawBoxFaces(corners, color);

            // Draw box edges on top
            GL.Begin(GL.LINES);
            GL.Color(color * 0.8f); // Slightly darker edges for definition

            // Bottom face
            GL.Vertex(corners[0]); GL.Vertex(corners[1]);
            GL.Vertex(corners[1]); GL.Vertex(corners[3]);
            GL.Vertex(corners[3]); GL.Vertex(corners[2]);
            GL.Vertex(corners[2]); GL.Vertex(corners[0]);

            // Top face
            GL.Vertex(corners[4]); GL.Vertex(corners[5]);
            GL.Vertex(corners[5]); GL.Vertex(corners[7]);
            GL.Vertex(corners[7]); GL.Vertex(corners[6]);
            GL.Vertex(corners[6]); GL.Vertex(corners[4]);

            // Vertical edges
            GL.Vertex(corners[0]); GL.Vertex(corners[4]);
            GL.Vertex(corners[1]); GL.Vertex(corners[5]);
            GL.Vertex(corners[2]); GL.Vertex(corners[6]);
            GL.Vertex(corners[3]); GL.Vertex(corners[7]);

            GL.End();
        }

        private void DrawBoxFaces(Vector3[] corners, Color color)
        {
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            // Front face (0,1,2,3)
            GL.Vertex(corners[0]); GL.Vertex(corners[1]); GL.Vertex(corners[2]);
            GL.Vertex(corners[1]); GL.Vertex(corners[3]); GL.Vertex(corners[2]);

            // Back face (5,4,6,7)
            GL.Vertex(corners[5]); GL.Vertex(corners[4]); GL.Vertex(corners[6]);
            GL.Vertex(corners[5]); GL.Vertex(corners[6]); GL.Vertex(corners[7]);

            // Left face (4,0,2,6)
            GL.Vertex(corners[4]); GL.Vertex(corners[0]); GL.Vertex(corners[2]);
            GL.Vertex(corners[4]); GL.Vertex(corners[2]); GL.Vertex(corners[6]);

            // Right face (1,5,7,3)
            GL.Vertex(corners[1]); GL.Vertex(corners[5]); GL.Vertex(corners[7]);
            GL.Vertex(corners[1]); GL.Vertex(corners[7]); GL.Vertex(corners[3]);

            // Top face (4,5,1,0)
            GL.Vertex(corners[4]); GL.Vertex(corners[5]); GL.Vertex(corners[1]);
            GL.Vertex(corners[4]); GL.Vertex(corners[1]); GL.Vertex(corners[0]);

            // Bottom face (2,3,7,6)
            GL.Vertex(corners[2]); GL.Vertex(corners[3]); GL.Vertex(corners[7]);
            GL.Vertex(corners[2]); GL.Vertex(corners[7]); GL.Vertex(corners[6]);

            GL.End();
        }

        private void DrawCenterHandle(Vector3 center, float size, bool isHovered, float alpha)
        {
            Color color = new Color(centerColor.r, centerColor.g, centerColor.b, 
                                   (isHovered ? selectedAlpha : axisAlpha) * alpha);
            
            // Draw a special center box that's always camera-facing
            Camera cam = Camera.main;
            Vector3 forward = (cam.transform.position - center).normalized;
            
            DrawBox(center, forward, size, color);
            
            // Draw connecting lines to make it clear it's the center
            float lineSize = size * 2f;
            GL.Begin(GL.LINES);
            GL.Color(color * 0.5f);
            GL.Vertex(center - Vector3.right * lineSize); GL.Vertex(center + Vector3.right * lineSize);
            GL.Vertex(center - Vector3.up * lineSize); GL.Vertex(center + Vector3.up * lineSize);
            GL.Vertex(center - Vector3.forward * lineSize); GL.Vertex(center + Vector3.forward * lineSize);
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