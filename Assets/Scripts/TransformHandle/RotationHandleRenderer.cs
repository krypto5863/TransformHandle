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
        private readonly float lineThickness = 3f; // Thickness for all circles
        
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
            
            // Check if circle is edge-on to camera (hide if too thin)
            Vector3 toCamera = (camera.transform.position - center).normalized;
            float dot = Mathf.Abs(Vector3.Dot(normal, toCamera));
            if (dot > 0.98f) return; // Circle is too edge-on, don't draw
            
            // Draw thick circle
            float thickness = (hoveredAxis == axisIndex) ? lineThickness * 1.5f : lineThickness;
            ThickLineHelper.DrawThickCircle(center, normal, radius, finalColor, circleSegments, thickness);
        }
        
        private void DrawCameraFacingCircle(Vector3 center, float radius, float alpha)
        {
            Camera camera = Camera.main;
            Vector3 normal = (camera.transform.position - center).normalized;
            
            GL.Color(new Color(1f, 1f, 1f, 0.3f * alpha));
            ThickLineHelper.DrawThickCircle(center, normal, radius, 
                new Color(1f, 1f, 1f, 0.3f * alpha), circleSegments, lineThickness * 0.8f);
        }
    }
}