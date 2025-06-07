using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Responsible for rendering transform handle visuals using GL
    /// </summary>
    public class HandleRenderer
    {
        private Material lineMaterial;
        private Material lineMaterialOccluded;
        
        // Colors
        private readonly Color xAxisColor = Color.red;
        private readonly Color yAxisColor = Color.green;
        private readonly Color zAxisColor = Color.blue;
        private readonly float axisAlpha = 0.8f;
        private readonly float selectedAlpha = 1f;
        private readonly float occludedAlpha = 0.3f;
        
        public HandleRenderer()
        {
            CreateMaterials();
        }
        
        private void CreateMaterials()
        {
            // Main material
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            
            // Occluded material
            lineMaterialOccluded = new Material(shader);
            lineMaterialOccluded.hideFlags = HideFlags.HideAndDontSave;
            
            lineMaterialOccluded.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterialOccluded.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterialOccluded.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterialOccluded.SetInt("_ZWrite", 0);
            lineMaterialOccluded.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Greater);
        }
        
        public void Render(Transform target, float scale, int hoveredAxis, bool alwaysOnTop)
        {
            if (target == null || lineMaterial == null) return;
            
            Vector3 position = target.position;
            
            if (!alwaysOnTop)
            {
                // Draw occluded parts first
                lineMaterialOccluded.SetPass(0);
                GL.PushMatrix();
                GL.MultMatrix(Matrix4x4.identity);
                
                DrawAxis(position, target.right, xAxisColor, scale, 0, hoveredAxis, occludedAlpha);
                DrawAxis(position, target.up, yAxisColor, scale, 1, hoveredAxis, occludedAlpha);
                DrawAxis(position, target.forward, zAxisColor, scale, 2, hoveredAxis, occludedAlpha);
                
                GL.PopMatrix();
            }
            
            // Draw visible parts
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            
            DrawAxis(position, target.right, xAxisColor, scale, 0, hoveredAxis);
            DrawAxis(position, target.up, yAxisColor, scale, 1, hoveredAxis);
            DrawAxis(position, target.forward, zAxisColor, scale, 2, hoveredAxis);
            
            DrawCenterPoint(position, scale * 0.1f);
            
            GL.PopMatrix();
        }
        
        private void DrawAxis(Vector3 origin, Vector3 direction, Color color, float length, 
                            int axisIndex, int hoveredAxis, float alphaOverride = -1f)
        {
            float alpha = alphaOverride >= 0 ? alphaOverride : 
                         (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
            Color finalColor = new Color(color.r, color.g, color.b, alpha);
            
            Vector3 endPoint = origin + direction * length;
            
            // Draw main line
            GL.Begin(GL.LINES);
            GL.Color(finalColor);
            GL.Vertex(origin);
            GL.Vertex(endPoint);
            GL.End();
            
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
            
            GL.Begin(GL.LINES);
            GL.Color(color);
            
            float baseSize = size * 0.5f;
            Vector3[] basePoints = new Vector3[4];
            basePoints[0] = arrowBase + perpendicular1 * baseSize;
            basePoints[1] = arrowBase - perpendicular1 * baseSize;
            basePoints[2] = arrowBase + perpendicular2 * baseSize;
            basePoints[3] = arrowBase - perpendicular2 * baseSize;
            
            foreach (Vector3 basePoint in basePoints)
            {
                GL.Vertex(tip);
                GL.Vertex(basePoint);
            }
            
            // Base square
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
        
        private void DrawCenterPoint(Vector3 center, float size)
        {
            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.5f));
            
            GL.Vertex(center + Vector3.right * size);
            GL.Vertex(center - Vector3.right * size);
            GL.Vertex(center + Vector3.up * size);
            GL.Vertex(center - Vector3.up * size);
            GL.Vertex(center + Vector3.forward * size);
            GL.Vertex(center - Vector3.forward * size);
            
            GL.End();
        }
        
        public void Cleanup()
        {
            if (lineMaterial != null)
                Object.DestroyImmediate(lineMaterial);
            if (lineMaterialOccluded != null)
                Object.DestroyImmediate(lineMaterialOccluded);
        }
    }
}