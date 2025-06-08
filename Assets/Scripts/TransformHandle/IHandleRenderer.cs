using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Interface for different handle renderer types
    /// </summary>
    public interface IHandleRenderer
    {
        void Render(Transform target, float scale, int hoveredAxis, float alpha = 1f);
    }
}