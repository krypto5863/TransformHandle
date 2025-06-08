using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Renders translation (movement) handles with arrows
    /// </summary>
    public class TranslationHandleRenderer : IHandleRenderer
    {
        private readonly Color xAxisColor = Color.red;
        private readonly Color yAxisColor = Color.green;
        private readonly Color zAxisColor = Color.blue;
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        private readonly float lineThickness = 6f; // Thickness for all lines
        
        public void Render(Transform target, float scale, int hoveredAxis, float alpha = 1f)
        {
            Vector3 position = target.position;
            
            DrawAxis(position, target.right, xAxisColor, scale, 0, hoveredAxis, alpha);
            DrawAxis(position, target.up, yAxisColor, scale, 1, hoveredAxis, alpha);
            DrawAxis(position, target.forward, zAxisColor, scale, 2, hoveredAxis, alpha);
            
            DrawCenterPoint(position, scale * 0.1f, alpha);
        }
        
        private void DrawAxis(Vector3 origin, Vector3 direction, Color color, float length, 
                            int axisIndex, int hoveredAxis, float alphaMultiplier)
        {
            float alpha = (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            alpha *= alphaMultiplier;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);
            
            Vector3 endPoint = origin + direction * length;
            
            // Draw main line with thickness
            float thickness = (hoveredAxis == axisIndex) ? lineThickness * 1.5f : lineThickness;
            ThickLineHelper.DrawThickLine(origin, endPoint, finalColor, thickness);
            
            // Draw arrow head
            DrawArrowHead(endPoint, direction, finalColor, length * 0.2f);
        }
        
        private void DrawArrowHead(Vector3 tip, Vector3 direction, Color color, float size)
        {
            Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpendicular1.magnitude < 0.1f)
            {
                perpendicular1 = Vector3.Cross(direction, Vector3.right).normalized;
            }
            Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;
            
            Vector3 arrowBase = tip - direction * size;
            
            float baseSize = size * 0.4f;
            Vector3[] basePoints = new Vector3[4];
            basePoints[0] = arrowBase + perpendicular1 * baseSize;
            basePoints[1] = arrowBase - perpendicular1 * baseSize;
            basePoints[2] = arrowBase + perpendicular2 * baseSize;
            basePoints[3] = arrowBase - perpendicular2 * baseSize;
            
            // Draw filled triangles for the arrow head
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);
            
            // Front face (4 triangles forming a pyramid)
            // Triangle 1
            GL.Vertex(tip);
            GL.Vertex(basePoints[0]);
            GL.Vertex(basePoints[2]);
            
            // Triangle 2
            GL.Vertex(tip);
            GL.Vertex(basePoints[2]);
            GL.Vertex(basePoints[1]);
            
            // Triangle 3
            GL.Vertex(tip);
            GL.Vertex(basePoints[1]);
            GL.Vertex(basePoints[3]);
            
            // Triangle 4
            GL.Vertex(tip);
            GL.Vertex(basePoints[3]);
            GL.Vertex(basePoints[0]);
            
            // Base (2 triangles)
            GL.Vertex(basePoints[0]);
            GL.Vertex(basePoints[1]);
            GL.Vertex(basePoints[2]);
            
            GL.Vertex(basePoints[1]);
            GL.Vertex(basePoints[3]);
            GL.Vertex(basePoints[2]);
            
            GL.End();
            
            // Draw outline for better definition
            GL.Begin(GL.LINES);
            GL.Color(color * 0.8f); // Slightly darker for outline
            
            // Lines from tip to base
            foreach (Vector3 basePoint in basePoints)
            {
                GL.Vertex(tip);
                GL.Vertex(basePoint);
            }
            
            // Base outline
            GL.Vertex(basePoints[0]);
            GL.Vertex(basePoints[2]);
            GL.Vertex(basePoints[2]);
            GL.Vertex(basePoints[1]);
            GL.Vertex(basePoints[1]);
            GL.Vertex(basePoints[3]);
            GL.Vertex(basePoints[3]);
            GL.Vertex(basePoints[0]);
            
            GL.End();
        }
        
        private void DrawCenterPoint(Vector3 center, float size, float alpha)
        {
            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.5f * alpha));
            
            GL.Vertex(center + Vector3.right * size);
            GL.Vertex(center - Vector3.right * size);
            GL.Vertex(center + Vector3.up * size);
            GL.Vertex(center - Vector3.up * size);
            GL.Vertex(center + Vector3.forward * size);
            GL.Vertex(center - Vector3.forward * size);
            
            GL.End();
        }
    }
}