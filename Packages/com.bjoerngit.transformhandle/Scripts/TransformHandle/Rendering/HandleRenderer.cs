using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Orchestrates rendering of different handle types using GL.
    /// Delegates actual drawing to specialized renderers.
    /// </summary>
    public class HandleRenderer
    {
        private IHandleRenderer translationRenderer;
        private IHandleRenderer rotationRenderer;
        private IHandleRenderer scaleRenderer;
        private Material lineMaterial;

        public HandleRenderer(Camera camera)
        {
            CreateMaterial();
            translationRenderer = new TranslationHandleRenderer(camera);
            rotationRenderer = new RotationHandleRenderer(camera);
            scaleRenderer = new ScaleHandleRenderer(camera);
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
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        /// <summary>
        /// Renders handles for the given transform.
        /// </summary>
        public void Render(Transform target, float scale, int hoveredAxis, HandleType handleType, HandleSpace handleSpace)
        {
            if (target == null || lineMaterial == null) return;

            IHandleRenderer renderer = GetRenderer(handleType);
            if (renderer == null) return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            renderer.Render(target, scale, hoveredAxis, handleSpace);

            GL.PopMatrix();
        }

        /// <summary>
        /// Renders handles based on a HandleProfile, supporting mixed local/global axes.
        /// </summary>
        public void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleType handleType, HandleProfile profile)
        {
            if (target == null || lineMaterial == null || profile == null) return;

            if (!profile.HasAnyAxisEnabled(handleType)) return;

            IHandleRenderer renderer = GetRenderer(handleType);
            if (renderer == null) return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            // Delegate to specialized renderer with profile
            if (renderer is IProfileAwareRenderer profileRenderer)
            {
                profileRenderer.RenderWithProfile(target, scale, hoveredAxis, profile);
            }
            else
            {
                // Fallback - shouldn't happen if all renderers implement IProfileAwareRenderer
                Debug.LogWarning($"{renderer.GetType().Name} doesn't support profile rendering");
            }

            GL.PopMatrix();
        }

        private IHandleRenderer GetRenderer(HandleType type)
        {
            switch (type)
            {
                case HandleType.Translation: return translationRenderer;
                case HandleType.Rotation: return rotationRenderer;
                case HandleType.Scale: return scaleRenderer;
                default: return null;
            }
        }

        public void Cleanup()
        {
            if (lineMaterial != null)
                Object.DestroyImmediate(lineMaterial);
        }
    }
}