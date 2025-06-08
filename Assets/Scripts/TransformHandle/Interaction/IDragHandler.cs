using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Interface for different drag handling strategies
    /// </summary>
    public interface IDragHandler
    {
        void StartDrag(Transform target, int axis, Vector2 mousePos);
        void UpdateDrag(Vector2 mousePos);
        void EndDrag();
    }
}