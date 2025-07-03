using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles the dragging logic for scale operations
    /// </summary>
    public class ScaleDragHandler : IDragHandler
    {
        private Camera mainCamera;
        private Transform target;
        private int draggedAxis;
        private HandleSpace handleSpace; // Not used for scale, but required by interface

        private Vector3 scaleStartValue;
        private Vector2 dragStartMousePos;
        private Vector3 dragAxisDirection;

        // Settings
        private readonly float scaleSpeed = 0.01f;

        public ScaleDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space)
        {
            this.target = target;
            this.draggedAxis = axis;
            this.handleSpace = space; // Ignored for scale

            scaleStartValue = target.localScale;
            dragStartMousePos = mousePos;

            // Store axis direction for axis-constrained scaling
            dragAxisDirection = GetScaleAxisMask(axis);
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null) return;

            // Calculate mouse movement along the handle direction
            Vector2 mouseDelta = mousePos - dragStartMousePos;
            
            // Project mouse movement onto screen-space axis for better control
            Vector3 handleScreenPos = mainCamera.WorldToScreenPoint(target.position);
            
            // Get screen direction of the handle
            Vector3 worldDir = GetWorldAxisDirection(draggedAxis);
            Vector3 screenEndPos = mainCamera.WorldToScreenPoint(target.position + worldDir);
            Vector2 screenDir = new Vector2(screenEndPos.x - handleScreenPos.x, 
                                           screenEndPos.y - handleScreenPos.y).normalized;

            // Project mouse delta onto axis direction
            float projectedDelta = Vector2.Dot(mouseDelta, screenDir);
            
            // Convert to scale factor
            float scaleFactor = 1f + (projectedDelta * scaleSpeed);
            
            // Apply scale based on axis
            if (draggedAxis == 3) // Center handle - uniform scale
            {
                target.localScale = scaleStartValue * scaleFactor;
            }
            else // Axis-constrained scale
            {
                Vector3 newScale = scaleStartValue;
                newScale.x *= Mathf.Lerp(1f, scaleFactor, dragAxisDirection.x);
                newScale.y *= Mathf.Lerp(1f, scaleFactor, dragAxisDirection.y);
                newScale.z *= Mathf.Lerp(1f, scaleFactor, dragAxisDirection.z);
                
                target.localScale = newScale;
            }
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
        }

        private Vector3 GetWorldAxisDirection(int axis)
        {
            switch (axis)
            {
                case 0: return target.right;
                case 1: return target.up;
                case 2: return target.forward;
                default: return Vector3.zero;
            }
        }

        private Vector3 GetScaleAxisMask(int axis)
        {
            switch (axis)
            {
                case 0: return Vector3.right;    // Scale only X
                case 1: return Vector3.up;       // Scale only Y
                case 2: return Vector3.forward;  // Scale only Z
                case 3: return Vector3.one;      // Scale all (uniform)
                default: return Vector3.one;
            }
        }
    }
}