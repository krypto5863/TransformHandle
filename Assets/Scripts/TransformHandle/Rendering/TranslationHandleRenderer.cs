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

        // Colors
        private readonly Color xAxisColor = Color.red;
        private readonly Color yAxisColor = Color.green;
        private readonly Color zAxisColor = Color.blue;
        
        // Alpha values
        private readonly float planeAlpha = 0.1f;         // Transparent when not hovered
        private readonly float planeHoverAlpha = 0.3f;    // More opaque when hovered
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        
        // Sizes
        private readonly float planeSize = PLANE_SIZE_MULTIPLIER;
        private readonly float baseThickness = 6f;
        private readonly float hoverThickness = 12f;

        public void Render(Transform target, float scale, int hoveredAxis, float alpha = 1f, HandleSpace handleSpace = HandleSpace.Local)
        {
            Vector3 position = target.position;

            Vector3 dirX = (handleSpace == HandleSpace.Local) ? target.right   : Vector3.right;
            Vector3 dirY = (handleSpace == HandleSpace.Local) ? target.up      : Vector3.up;
            Vector3 dirZ = (handleSpace == HandleSpace.Local) ? target.forward : Vector3.forward;

            // Render planes FIRST (behind axes)
            DrawTranslationPlanes(position, dirX, dirY, dirZ, scale, hoveredAxis, alpha);

            // Then render axes (on top)
            DrawAxis(position, dirX, xAxisColor, scale, 0, hoveredAxis, alpha);
            DrawAxis(position, dirY, yAxisColor, scale, 1, hoveredAxis, alpha);
            DrawAxis(position, dirZ, zAxisColor, scale, 2, hoveredAxis, alpha);

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

        private void DrawTranslationPlanes(Vector3 center, Vector3 dirX, Vector3 dirY, Vector3 dirZ, 
                                         float scale, int hoveredAxis, float alpha)
        {
            float size = scale * planeSize;
            Camera cam = Camera.main;
            Vector3 camForward = cam.transform.forward;

            // XY Plane (Z stays constant) - axis 4 - Blue color (Z axis)
            float dotX_XY = Vector3.Dot(dirX, -camForward);
            float dotY_XY = Vector3.Dot(dirY, -camForward);
            Vector3 offsetXY = Vector3.zero;
            if (dotX_XY > 0) offsetXY += dirX * size;
            if (dotY_XY > 0) offsetXY += dirY * size;
            DrawPlane(center + offsetXY, dirX, dirY, zAxisColor, size, 4, hoveredAxis, alpha);

            // XZ Plane (Y stays constant) - axis 5 - Green color (Y axis)
            float dotX_XZ = Vector3.Dot(dirX, -camForward);
            float dotZ_XZ = Vector3.Dot(dirZ, -camForward);
            Vector3 offsetXZ = Vector3.zero;
            if (dotX_XZ > 0) offsetXZ += dirX * size;
            if (dotZ_XZ > 0) offsetXZ += dirZ * size;
            DrawPlane(center + offsetXZ, dirX, dirZ, yAxisColor, size, 5, hoveredAxis, alpha);

            // YZ Plane (X stays constant) - axis 6 - Red color (X axis)
            float dotY_YZ = Vector3.Dot(dirY, -camForward);
            float dotZ_YZ = Vector3.Dot(dirZ, -camForward);
            Vector3 offsetYZ = Vector3.zero;
            if (dotY_YZ > 0) offsetYZ += dirY * size;
            if (dotZ_YZ > 0) offsetYZ += dirZ * size;
            DrawPlane(center + offsetYZ, dirY, dirZ, xAxisColor, size, 6, hoveredAxis, alpha);
        }

        private void RenderPlanesWithProfile(Transform target, Vector3 position, float scale, 
                                           int hoveredAxis, HandleProfile profile, float alpha)
        {
            float size = scale * planeSize;
            Camera cam = Camera.main;
            Vector3 camForward = cam.transform.forward;

            // XY Plane (axis 4) - Blue (Z axis color)
            if (profile.IsAxisEnabled(HandleType.Translation, 4, HandleSpace.Local))
            {
                Vector3 dirX = GetLocalAxisDirection(target, 0);
                Vector3 dirY = GetLocalAxisDirection(target, 1);
                Vector3 offset = CalculatePlaneOffset(dirX, dirY, size, camForward);
                DrawPlane(position + offset, dirX, dirY, zAxisColor, size, 4, hoveredAxis, alpha);
            }
            if (profile.IsAxisEnabled(HandleType.Translation, 4, HandleSpace.Global))
            {
                Vector3 offset = CalculatePlaneOffset(Vector3.right, Vector3.up, size, camForward);
                DrawPlane(position + offset, Vector3.right, Vector3.up, zAxisColor, size, 4, hoveredAxis, alpha);
            }

            // XZ Plane (axis 5) - Green (Y axis color)
            if (profile.IsAxisEnabled(HandleType.Translation, 5, HandleSpace.Local))
            {
                Vector3 dirX = GetLocalAxisDirection(target, 0);
                Vector3 dirZ = GetLocalAxisDirection(target, 2);
                Vector3 offset = CalculatePlaneOffset(dirX, dirZ, size, camForward);
                DrawPlane(position + offset, dirX, dirZ, yAxisColor, size, 5, hoveredAxis, alpha);
            }
            if (profile.IsAxisEnabled(HandleType.Translation, 5, HandleSpace.Global))
            {
                Vector3 offset = CalculatePlaneOffset(Vector3.right, Vector3.forward, size, camForward);
                DrawPlane(position + offset, Vector3.right, Vector3.forward, yAxisColor, size, 5, hoveredAxis, alpha);
            }

            // YZ Plane (axis 6) - Red (X axis color)
            if (profile.IsAxisEnabled(HandleType.Translation, 6, HandleSpace.Local))
            {
                Vector3 dirY = GetLocalAxisDirection(target, 1);
                Vector3 dirZ = GetLocalAxisDirection(target, 2);
                Vector3 offset = CalculatePlaneOffset(dirY, dirZ, size, camForward);
                DrawPlane(position + offset, dirY, dirZ, xAxisColor, size, 6, hoveredAxis, alpha);
            }
            if (profile.IsAxisEnabled(HandleType.Translation, 6, HandleSpace.Global))
            {
                Vector3 offset = CalculatePlaneOffset(Vector3.up, Vector3.forward, size, camForward);
                DrawPlane(position + offset, Vector3.up, Vector3.forward, xAxisColor, size, 6, hoveredAxis, alpha);
            }
        }

        private Vector3 CalculatePlaneOffset(Vector3 axis1, Vector3 axis2, float size, Vector3 camForward)
        {
            Vector3 offset = Vector3.zero;
            
            // Check if axis1 points towards camera
            if (Vector3.Dot(axis1, -camForward) > 0)
                offset += axis1 * size;
                
            // Check if axis2 points towards camera
            if (Vector3.Dot(axis2, -camForward) > 0)
                offset += axis2 * size;
                
            return offset;
        }

        private void DrawPlane(Vector3 center, Vector3 axis1, Vector3 axis2, Color color, 
                             float size, int planeIndex, int hoveredAxis, float alphaMultiplier)
        {
            bool isHovered = (hoveredAxis == planeIndex);
            float alpha = isHovered ? planeHoverAlpha : planeAlpha;
            alpha *= alphaMultiplier;

            Color fillColor = new Color(color.r, color.g, color.b, alpha);
            Color outlineColor = new Color(color.r, color.g, color.b, alpha * 2f); // Outline more visible

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

        private Color GetPlaneColor(int axis1, int axis2)
        {
            // Mix colors of the two axes
            Color c1 = GetAxisColor(axis1);
            Color c2 = GetAxisColor(axis2);
            return (c1 + c2) * 0.5f;
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