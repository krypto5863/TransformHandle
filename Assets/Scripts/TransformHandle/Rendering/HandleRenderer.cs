using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Manages rendering of different handle types using GL, with support for Local/Global space.
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
            translationRenderer = new TranslationHandleRenderer();
            rotationRenderer    = new RotationHandleRenderer();
        }

        private void CreateMaterials()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");

            lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);

            lineMaterialOccluded = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lineMaterialOccluded.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterialOccluded.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterialOccluded.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterialOccluded.SetInt("_ZWrite", 0);
            lineMaterialOccluded.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Greater);
        }

        /// <summary>
        /// Renders handles for the given transform.
        /// </summary>
        /// <param name="target">Transform whose handles are drawn.</param>
        /// <param name="scale">Scale factor for handle size.</param>
        /// <param name="hoveredAxis">Index of hovered axis (-1 if none).</param>
        /// <param name="alwaysOnTop">If true, draws handles in front of all geometry.</param>
        /// <param name="handleType">Translation or Rotation handles.</param>
        /// <param name="handleSpace">Local or Global axis space.</param>
        public void Render(Transform target, float scale, int hoveredAxis, bool alwaysOnTop, HandleType handleType, HandleSpace handleSpace)
        {
            if (target == null || lineMaterial == null)
                return;

            IHandleRenderer renderer = GetRenderer(handleType);
            if (renderer == null)
                return;

            // Configure depth test based on alwaysOnTop
            if (alwaysOnTop)
                lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            else
                lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);

            // Draw occluded parts first if not always on top
            if (!alwaysOnTop)
            {
                lineMaterialOccluded.SetPass(0);
                GL.PushMatrix();
                GL.MultMatrix(Matrix4x4.identity);

                renderer.Render(target, scale, hoveredAxis, occludedAlpha, handleSpace);

                GL.PopMatrix();
            }

            // Draw visible parts
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            renderer.Render(target, scale, hoveredAxis, 1f, handleSpace);

            GL.PopMatrix();
        }

        private IHandleRenderer GetRenderer(HandleType type)
        {
            switch (type)
            {
                case HandleType.Translation: return translationRenderer;
                case HandleType.Rotation:    return rotationRenderer;
                default:                     return null;
            }
        }

        /// <summary>
        /// Cleanup materials.
        /// </summary>
        public void Cleanup()
        {
            if (lineMaterial != null)        Object.DestroyImmediate(lineMaterial);
            if (lineMaterialOccluded != null) Object.DestroyImmediate(lineMaterialOccluded);
        }
    }
}
