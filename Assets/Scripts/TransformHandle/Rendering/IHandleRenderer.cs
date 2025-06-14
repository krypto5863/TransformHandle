using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Interface for rendering transform handles with selectable space (Local or Global).
    /// </summary>
    public interface IHandleRenderer
    {
        /// <param name="target">The transform whose handles are rendered.</param>
        /// <param name="scale">Overall scale factor for size.</param>
        /// <param name="hoveredAxis">Index of the currently hovered axis, or -1 if none.</param>
        /// <param name="handleSpace">Whether to use local or global axes.</param>
        void Render(Transform target, float scale, int hoveredAxis, HandleSpace handleSpace = HandleSpace.Local);
    }
}