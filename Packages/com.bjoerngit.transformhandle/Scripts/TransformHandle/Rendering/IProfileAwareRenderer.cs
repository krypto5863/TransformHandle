using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Interface for renderers that support HandleProfile-based rendering
    /// </summary>
    public interface IProfileAwareRenderer : IHandleRenderer
    {
        /// <summary>
        /// Renders handles based on a profile configuration
        /// </summary>
        void RenderWithProfile(Transform target, float scale, int hoveredAxis, HandleProfile profile);
    }
}