using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Renders rotation handles with circles
    /// </summary>
    public class RotationHandleRenderer : IHandleRenderer
    {
        private readonly Color xAxisColor = Color.red;
        private readonly Color yAxisColor = Color.green;
        private readonly Color zAxisColor = Color.blue;
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        private readonly int circleSegments = 64;
        
        public void Render(Transform target, float scale, int hoveredAxis, float alpha = 1f)
        {
            Vector3 position = target.position;
            Camera camera = Camera.main;
            
            // Draw rotation circles for each axis
            DrawRotationCircle(position, target.right, yAxisColor, scale, 1, hoveredAxis, alpha, camera); // Y-axis (green circle)
            DrawRotationCircle(position, target.up, zAxisColor, scale, 2, hoveredAxis, alpha, camera);    // Z-axis (blue circle)
            DrawRotationCircle(position, target.forward, xAxisColor, scale, 0, hoveredAxis, alpha, camera); // X-axis (red circle)
            
            // Draw free rotation sphere (white circle facing camera)
            DrawCameraFacingCircle(position, scale * 1.2f, alpha);
        }
        
        private void DrawRotationCircle(Vector3 center, Vector3 normal, Color color, float radius, 
                                      int axisIndex, int hoveredAxis, float alphaMultiplier, Camera camera)
        {
            float alpha = (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            alpha *= alphaMultiplier;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);
            
            // Calculate two perpendicular vectors to the normal
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.magnitude < 0.1f)
            {
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            }
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;
            
            // Check if circle is edge-on to camera (hide if too thin)
            Vector3 toCamera = (camera.transform.position - center).normalized;
            float dot = Mathf.Abs(Vector3.Dot(normal, toCamera));
            if (dot > 0.98f) return; // Circle is too edge-on, don't draw
            
            GL.Begin(GL.LINES);
            GL.Color(finalColor);
            
            // Draw circle
            for (int i = 0; i < circleSegments; i++)
            {
                float angle1 = (i / (float)circleSegments) * 2 * Mathf.PI;
                float angle2 = ((i + 1) / (float)circleSegments) * 2 * Mathf.PI;
                
                Vector3 point1 = center + (tangent1 * Mathf.Cos(angle1) + tangent2 * Mathf.Sin(angle1)) * radius;
                Vector3 point2 = center + (tangent1 * Mathf.Cos(angle2) + tangent2 * Mathf.Sin(angle2)) * radius;
                
                GL.Vertex(point1);
                GL.Vertex(point2);
            }
            
            GL.End();
        }
        
        private void DrawCameraFacingCircle(Vector3 center, float radius, float alpha)
        {
            Camera camera = Camera.main;
            Vector3 normal = (camera.transform.position - center).normalized;
            
            // Calculate perpendicular vectors
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.magnitude < 0.1f)
            {
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            }
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;
            
            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.3f * alpha));
            
            // Draw outer circle
            for (int i = 0; i < circleSegments; i++)
            {
                float angle1 = (i / (float)circleSegments) * 2 * Mathf.PI;
                float angle2 = ((i + 1) / (float)circleSegments) * 2 * Mathf.PI;
                
                Vector3 point1 = center + (tangent1 * Mathf.Cos(angle1) + tangent2 * Mathf.Sin(angle1)) * radius;
                Vector3 point2 = center + (tangent1 * Mathf.Cos(angle2) + tangent2 * Mathf.Sin(angle2)) * radius;
                
                GL.Vertex(point1);
                GL.Vertex(point2);
            }
            
            GL.End();
        }
    }
}