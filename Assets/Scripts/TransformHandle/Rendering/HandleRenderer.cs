using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Manages rendering of different handle types using GL, always rendered on top.
    /// </summary>
    public class HandleRenderer
    {
        private IHandleRenderer translationRenderer;
        private IHandleRenderer rotationRenderer;
        private IHandleRenderer scaleRenderer;
        private Material lineMaterial;

        public HandleRenderer()
        {
            CreateMaterial();
            translationRenderer = new TranslationHandleRenderer();
            rotationRenderer    = new RotationHandleRenderer();
            scaleRenderer       = new ScaleHandleRenderer();
        }

        private void CreateMaterial()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull",    (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite",  0);
            // Always render on top
            lineMaterial.SetInt("_ZTest",   (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        /// <summary>
        /// Renders handles for the given transform, always on top.
        /// </summary>
        /// <param name="target">Transform whose handles are drawn.</param>
        /// <param name="scale">Scale factor for handle size.</param>
        /// <param name="hoveredAxis">Index of hovered axis (-1 if none).</param>
        /// <param name="handleType">Translation or Rotation handles.</param>
        /// <param name="handleSpace">Local or Global axis space.</param>
        public void Render(Transform target, float scale, int hoveredAxis, HandleType handleType, HandleSpace handleSpace)
        {
            if (target == null || lineMaterial == null)
                return;

            IHandleRenderer renderer = GetRenderer(handleType);
            if (renderer == null)
                return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            // Draw directly with full opacity
            renderer.Render(target, scale, hoveredAxis, 1f, handleSpace);

            GL.PopMatrix();
        }

        /// <summary>
        /// Renders handles based on a HandleProfile, supporting mixed local/global axes.
        /// </summary>
        /// <param name="target">Transform whose handles are drawn.</param>
        /// <param name="scale">Scale factor for handle size.</param>
        /// <param name="hoveredAxis">Index of hovered axis (-1 if none).</param>
        /// <param name="handleType">Current handle type (Translation/Rotation/Scale).</param>
        /// <param name="profile">Handle profile with per-axis configuration.</param>
        public void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleType handleType, HandleProfile profile)
        {
            if (target == null || lineMaterial == null || profile == null)
                return;

            // Skip if no axes are enabled for this handle type
            if (!profile.HasAnyAxisEnabled(handleType))
                return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            // We need to render axes individually based on profile settings
            switch (handleType)
            {
                case HandleType.Translation:
                    RenderTranslationWithProfile(target, scale, hoveredAxis, profile);
                    break;
                case HandleType.Rotation:
                    RenderRotationWithProfile(target, scale, hoveredAxis, profile);
                    break;
                case HandleType.Scale:
                    RenderScaleWithProfile(target, scale, hoveredAxis, profile);
                    break;
            }

            GL.PopMatrix();
        }

        private void RenderTranslationWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile)
        {
            Vector3 position = target.position;
            TranslationHandleRenderer renderer = translationRenderer as TranslationHandleRenderer;
            
            // Check and render each axis in its configured space
            for (int axis = 0; axis < 3; axis++)
            {
                // Check local space
                if (profile.IsAxisEnabled(HandleType.Translation, axis, HandleSpace.Local))
                {
                    Vector3 direction = GetLocalAxisDirection(target, axis);
                    Color color = GetAxisColor(axis);
                    bool isHovered = (hoveredAxis == axis);
                    DrawTranslationAxis(renderer, position, direction, color, scale, axis, isHovered);
                }
                
                // Check global space
                if (profile.IsAxisEnabled(HandleType.Translation, axis, HandleSpace.Global))
                {
                    Vector3 direction = GetGlobalAxisDirection(axis);
                    Color color = GetAxisColor(axis);
                    bool isHovered = (hoveredAxis == axis);
                    DrawTranslationAxis(renderer, position, direction, color, scale, axis, isHovered);
                }
            }

            // Always draw center point
            DrawCenterPoint(position, scale * 0.1f);
        }

        private void RenderRotationWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile)
        {
            Vector3 position = target.position;
            Camera camera = Camera.main;
            
            // Check and render each axis in its configured space
            for (int axis = 0; axis < 3; axis++)
            {
                // Check local space
                if (profile.IsAxisEnabled(HandleType.Rotation, axis, HandleSpace.Local))
                {
                    Vector3 normal = GetLocalAxisDirection(target, axis);
                    Color color = GetAxisColor(axis);
                    bool isHovered = (hoveredAxis == axis);
                    DrawRotationCircle(position, normal, color, scale, axis, isHovered, camera);
                }
                
                // Check global space
                if (profile.IsAxisEnabled(HandleType.Rotation, axis, HandleSpace.Global))
                {
                    Vector3 normal = GetGlobalAxisDirection(axis);
                    Color color = GetAxisColor(axis);
                    bool isHovered = (hoveredAxis == axis);
                    DrawRotationCircle(position, normal, color, scale, axis, isHovered, camera);
                }
            }

            // Free rotation sphere
            DrawCameraFacingCircle(position, scale * 1.2f, camera);
        }

        private void RenderScaleWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile)
        {
            Vector3 position = target.position;
            
            // For scale, typically use local axes, but check profile settings
            for (int axis = 0; axis < 3; axis++)
            {
                // Check local space
                if (profile.IsAxisEnabled(HandleType.Scale, axis, HandleSpace.Local))
                {
                    Vector3 direction = GetLocalAxisDirection(target, axis);
                    Color color = GetAxisColor(axis);
                    bool isHovered = (hoveredAxis == axis);
                    DrawScaleAxis(position, direction, color, scale, axis, isHovered);
                }
                
                // Check global space (less common for scale)
                if (profile.IsAxisEnabled(HandleType.Scale, axis, HandleSpace.Global))
                {
                    Vector3 direction = GetGlobalAxisDirection(axis);
                    Color color = GetAxisColor(axis);
                    bool isHovered = (hoveredAxis == axis);
                    DrawScaleAxis(position, direction, color, scale, axis, isHovered);
                }
            }

            // Uniform scale handle (axis index 3)
            if (profile.IsAxisEnabled(HandleType.Scale, 3, HandleSpace.Local) || 
                profile.IsAxisEnabled(HandleType.Scale, 3, HandleSpace.Global))
            {
                DrawCenterScaleHandle(position, scale * 0.135f, hoveredAxis == 3);
            }
        }

        // Helper methods for individual axis rendering
        private void DrawTranslationAxis(TranslationHandleRenderer renderer, Vector3 origin, Vector3 direction, 
                                       Color color, float length, int axisIndex, bool isHovered)
        {
            float alpha = isHovered ? 1f : 0.8f;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);
            Vector3 endPoint = origin + direction * length;

            float thickness = isHovered ? 9f : 6f;
            ThickLineHelper.DrawThickLine(origin, endPoint, finalColor, thickness);

            // Arrow head
            DrawArrowHead(endPoint, direction, finalColor, length * 0.2f);
        }

        private void DrawRotationCircle(Vector3 center, Vector3 normal, Color color, float radius,
                                      int axisIndex, bool isHovered, Camera camera)
        {
            float alpha = isHovered ? 1f : 0.8f;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);

            // Skip if circle is edge-on
            Vector3 toCamera = (camera.transform.position - center).normalized;
            float edgeDot = Mathf.Abs(Vector3.Dot(normal, toCamera));
            if (edgeDot > 0.98f) return;

            // Draw the circle
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.sqrMagnitude < 0.1f)
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;

            float thickness = isHovered ? 9f : 6f;
            int segments = 64;

            GL.Begin(GL.LINES);
            for (int i = 0; i < segments; i++)
            {
                float angleA = (i / (float)segments) * 2f * Mathf.PI;
                float angleB = ((i + 1) / (float)segments) * 2f * Mathf.PI;

                Vector3 pA = center + (tangent1 * Mathf.Cos(angleA) + tangent2 * Mathf.Sin(angleA)) * radius;
                Vector3 pB = center + (tangent1 * Mathf.Cos(angleB) + tangent2 * Mathf.Sin(angleB)) * radius;

                // Simple visibility check
                Vector3 mid = (pA + pB) * 0.5f;
                float dotMid = Vector3.Dot((mid - center).normalized, toCamera);
                if (dotMid < -0.1f) continue;

                float fade = Mathf.Clamp01((dotMid + 0.1f) / 0.2f);
                Color segmentColor = new Color(finalColor.r, finalColor.g, finalColor.b, finalColor.a * fade);
                GL.Color(segmentColor);

                // Draw thick segment
                for (int t = 0; t < Mathf.Max(1, Mathf.RoundToInt(thickness)); t++)
                {
                    float offset = (t - (thickness - 1) * 0.5f) * 0.001f;
                    Vector3 offVec = toCamera * offset;
                    GL.Vertex(pA + offVec);
                    GL.Vertex(pB + offVec);
                }
            }
            GL.End();
        }

        private void DrawScaleAxis(Vector3 origin, Vector3 direction, Color color, float length,
                                 int axisIndex, bool isHovered)
        {
            float alpha = isHovered ? 1f : 0.8f;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);
            Vector3 endPoint = origin + direction * length;

            float thickness = isHovered ? 9f : 6f;
            ThickLineHelper.DrawThickLine(origin, endPoint, finalColor, thickness);

            // Draw box at end
            float boxSize = length * 0.09f;
            if (isHovered) boxSize *= 1.3f;
            DrawBox(endPoint, direction, boxSize, finalColor);
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
            GL.End();
        }

        private void DrawBox(Vector3 center, Vector3 forward, float size, Color color)
        {
            Vector3 up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(forward, up).normalized;
            up = Vector3.Cross(right, forward).normalized;

            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                float x = (i & 1) == 0 ? -1 : 1;
                float y = (i & 2) == 0 ? -1 : 1;
                float z = (i & 4) == 0 ? -1 : 1;
                corners[i] = center + (right * x + up * y + forward * z) * size * 0.5f;
            }

            // Draw filled faces
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);
            // Front face
            GL.Vertex(corners[0]); GL.Vertex(corners[1]); GL.Vertex(corners[2]);
            GL.Vertex(corners[1]); GL.Vertex(corners[3]); GL.Vertex(corners[2]);
            // Other faces...
            GL.End();

            // Draw edges
            GL.Begin(GL.LINES);
            GL.Color(color * 0.8f);
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

        private void DrawCenterPoint(Vector3 center, float size)
        {
            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.5f));
            GL.Vertex(center + Vector3.right * size); GL.Vertex(center - Vector3.right * size);
            GL.Vertex(center + Vector3.up * size); GL.Vertex(center - Vector3.up * size);
            GL.Vertex(center + Vector3.forward * size); GL.Vertex(center - Vector3.forward * size);
            GL.End();
        }

        private void DrawCenterScaleHandle(Vector3 center, float size, bool isHovered)
        {
            Color color = new Color(1f, 1f, 1f, isHovered ? 1f : 0.8f);
            Camera cam = Camera.main;
            Vector3 forward = (cam.transform.position - center).normalized;
            
            DrawBox(center, forward, size, color);
            
            // Draw connecting lines
            float lineSize = size * 2f;
            GL.Begin(GL.LINES);
            GL.Color(color * 0.5f);
            GL.Vertex(center - Vector3.right * lineSize); GL.Vertex(center + Vector3.right * lineSize);
            GL.Vertex(center - Vector3.up * lineSize); GL.Vertex(center + Vector3.up * lineSize);
            GL.Vertex(center - Vector3.forward * lineSize); GL.Vertex(center + Vector3.forward * lineSize);
            GL.End();
        }

        private void DrawCameraFacingCircle(Vector3 center, float radius, Camera camera)
        {
            Vector3 normal = (camera.transform.position - center).normalized;
            float thickness = 6f * 0.8f;
            Color color = new Color(1f, 1f, 1f, 0.3f);
            ThickLineHelper.DrawThickCircle(center, normal, radius, color, 64, thickness);
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

        private IHandleRenderer GetRenderer(HandleType type)
        {
            switch (type)
            {
                case HandleType.Translation: return translationRenderer;
                case HandleType.Rotation:    return rotationRenderer;
                case HandleType.Scale:       return scaleRenderer;
                default:                     return null;
            }
        }

        /// <summary>
        /// Cleanup material.
        /// </summary>
        public void Cleanup()
        {
            if (lineMaterial != null)
                Object.DestroyImmediate(lineMaterial);
        }
    }
}