using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Central manager for batched handle rendering
    /// </summary>
    public class HandleRenderManager
    {
        private static HandleRenderManager instance;
        public static HandleRenderManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new HandleRenderManager();
                return instance;
            }
        }

        // Shared batch renderer
        private BatchedHandleRenderer batcher = new BatchedHandleRenderer();

        // Handle renderers that share the batcher
        private TranslationHandleRenderer translationRenderer;
        private RotationHandleRenderer rotationRenderer;
        private ScaleHandleRenderer scaleRenderer;

        public HandleRenderManager()
        {
            // Initialize renderers with shared batcher
            translationRenderer = new TranslationHandleRenderer(batcher);
            rotationRenderer = new RotationHandleRenderer(batcher);
            scaleRenderer = new ScaleHandleRenderer(batcher);
        }

        /// <summary>
        /// Begin a new frame - clears all batches
        /// </summary>
        public void BeginFrame()
        {
            batcher.Clear();
        }

        /// <summary>
        /// Render translation handles
        /// </summary>
        public void RenderTranslationHandles(Transform target, float scale, int hoveredAxis, HandleSpace handleSpace = HandleSpace.Local)
        {
            translationRenderer.Render(target, scale, hoveredAxis, handleSpace);
        }

        /// <summary>
        /// Render rotation handles
        /// </summary>
        public void RenderRotationHandles(Transform target, float scale, int hoveredAxis, HandleSpace handleSpace = HandleSpace.Local)
        {
            rotationRenderer.Render(target, scale, hoveredAxis, handleSpace);
        }

        /// <summary>
        /// Render scale handles
        /// </summary>
        public void RenderScaleHandles(Transform target, float scale, int hoveredAxis, HandleSpace handleSpace = HandleSpace.Local)
        {
            scaleRenderer.Render(target, scale, hoveredAxis, handleSpace);
        }

        /// <summary>
        /// Render translation handles with profile
        /// </summary>
        public void RenderTranslationHandlesWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile)
        {
            translationRenderer.RenderWithProfile(target, scale, hoveredAxis, profile);
        }

        /// <summary>
        /// Render rotation handles with profile
        /// </summary>
        public void RenderRotationHandlesWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile)
        {
            rotationRenderer.RenderWithProfile(target, scale, hoveredAxis, profile);
        }

        /// <summary>
        /// Render scale handles with profile
        /// </summary>
        public void RenderScaleHandlesWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile)
        {
            scaleRenderer.RenderWithProfile(target, scale, hoveredAxis, profile);
        }

        /// <summary>
        /// End frame and render all collected geometry
        /// </summary>
        public void EndFrame()
        {
            batcher.Render();
        }
    }

    /// <summary>
    /// Updated base classes to accept shared batcher
    /// </summary>
    public abstract class BatchedHandleRendererBase : IProfileAwareRenderer
    {
        protected BatchedHandleRenderer batcher;

        public BatchedHandleRendererBase(BatchedHandleRenderer sharedBatcher)
        {
            this.batcher = sharedBatcher;
        }

        public abstract void Render(Transform target, float scale, int hoveredAxis, HandleSpace handleSpace = HandleSpace.Local);
        public abstract void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile);
    }
}