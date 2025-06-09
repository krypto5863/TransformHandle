using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Manages rendering of different handle types using GL, always rendered on top.
    /// </summary>
    public class HandleRenderer
    {
        private IHandleRenderer translationRenderer;
        private IHandleRenderer rotationRenderer;
        private Material lineMaterial;

        public HandleRenderer()
        {
            CreateMaterial();
            translationRenderer = new TranslationHandleRenderer();
            rotationRenderer    = new RotationHandleRenderer();
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
        /// Cleanup material.
        /// </summary>
        public void Cleanup()
        {
            if (lineMaterial != null)
                Object.DestroyImmediate(lineMaterial);
        }
    }
}
