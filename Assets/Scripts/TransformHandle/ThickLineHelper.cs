using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Helper class to draw thick lines using GL
    /// </summary>
    public static class ThickLineHelper
    {
        /// <summary>
        /// Draws a thick line by rendering multiple parallel lines
        /// </summary>
        public static void DrawThickLine(Vector3 start, Vector3 end, Color color, float thickness = 3f)
        {
            // Calculate perpendicular vector for offset
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Camera.main.transform.forward).normalized;
            
            // If the line is parallel to camera forward, use up vector instead
            if (perpendicular.magnitude < 0.1f)
            {
                perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            }
            
            int lineCount = Mathf.Max(1, Mathf.RoundToInt(thickness));
            float step = thickness / lineCount;
            
            GL.Begin(GL.LINES);
            GL.Color(color); // Set color inside GL.Begin()
            
            for (int i = 0; i < lineCount; i++)
            {
                float offset = (i - (lineCount - 1) * 0.5f) * step * 0.001f; // 0.001f converts to world units
                Vector3 offsetVector = perpendicular * offset;
                
                GL.Vertex(start + offsetVector);
                GL.Vertex(end + offsetVector);
            }
            
            GL.End();
        }
        
        /// <summary>
        /// Draws a thick circle by rendering multiple parallel circles
        /// </summary>
        public static void DrawThickCircle(Vector3 center, Vector3 normal, float radius, Color color, 
                                         int segments = 64, float thickness = 3f)
        {
            // Calculate tangent vectors
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.magnitude < 0.1f)
            {
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            }
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;
            
            int lineCount = Mathf.Max(1, Mathf.RoundToInt(thickness));
            float radiusStep = thickness * 0.001f / lineCount; // Convert to world units
            
            GL.Begin(GL.LINES);
            GL.Color(color); // Set color inside GL.Begin()
            
            for (int t = 0; t < lineCount; t++)
            {
                float currentRadius = radius + (t - (lineCount - 1) * 0.5f) * radiusStep;
                
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = (i / (float)segments) * 2 * Mathf.PI;
                    float angle2 = ((i + 1) / (float)segments) * 2 * Mathf.PI;
                    
                    Vector3 point1 = center + (tangent1 * Mathf.Cos(angle1) + tangent2 * Mathf.Sin(angle1)) * currentRadius;
                    Vector3 point2 = center + (tangent1 * Mathf.Cos(angle2) + tangent2 * Mathf.Sin(angle2)) * currentRadius;
                    
                    GL.Vertex(point1);
                    GL.Vertex(point2);
                }
            }
            
            GL.End();
        }
    }
}