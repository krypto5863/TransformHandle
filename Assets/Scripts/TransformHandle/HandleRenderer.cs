using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Manages rendering of different handle types using GL
    /// </summary>
    public class HandleRenderer
    {
        private IHandleRenderer translationRenderer;
        private IHandleRenderer rotationRenderer;
        private Material lineMaterial;
        private Material lineMaterialOccluded;
        
        private readonly float occludedAlpha = 0.3f;
        
        public HandleRenderer()
        {
            CreateMaterials();
            
            // Initialize renderers
            translationRenderer = new TranslationHandleRenderer();
            rotationRenderer = new RotationHandleRenderer();
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
        
        public void Render(Transform target, float scale, int hoveredAxis, bool alwaysOnTop, HandleType handleType)
        {
            if (target == null || lineMaterial == null) return;
            
            IHandleRenderer renderer = GetRenderer(handleType);
            if (renderer == null) return;
            
            if (!alwaysOnTop)
            {
                // Draw occluded parts first
                lineMaterialOccluded.SetPass(0);
                GL.PushMatrix();
                GL.MultMatrix(Matrix4x4.identity);
                
                renderer.Render(target, scale, hoveredAxis, occludedAlpha);
                
                GL.PopMatrix();
            }
            
            // Draw visible parts
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            
            renderer.Render(target, scale, hoveredAxis);
            
            GL.PopMatrix();
        }
        
        private IHandleRenderer GetRenderer(HandleType type)
        {
            switch (type)
            {
                case HandleType.Translation:
                    return translationRenderer;
                case HandleType.Rotation:
                    return rotationRenderer;
                default:
                    return null;
            }
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