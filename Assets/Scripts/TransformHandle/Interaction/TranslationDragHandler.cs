using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Handles the dragging logic for translation (movement) operations
    /// </summary>
    public class TranslationDragHandler : IDragHandler
    {
        private Camera mainCamera;
        private Transform target;
        private int draggedAxis;

        private Vector3 dragStartPosition;
        private Vector2 dragStartMouseScreenPos;

        public TranslationDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos)
        {
            this.target = target;
            this.draggedAxis = axis;

            dragStartPosition = target.position;
            dragStartMouseScreenPos = mousePos;
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null || draggedAxis < 0) return;

            // Get axis direction in world space
            Vector3 axisDirection = GetAxisDirection(draggedAxis);

            // Convert axis direction to screen space
            Vector3 handleScreenPos = mainCamera.WorldToScreenPoint(target.position);
            Vector3 axisEndScreen = mainCamera.WorldToScreenPoint(target.position + axisDirection);

            // Get screen space axis direction
            Vector2 axisScreenDir = new Vector2(axisEndScreen.x - handleScreenPos.x,
                                                axisEndScreen.y - handleScreenPos.y).normalized;

            // Calculate mouse delta from the initial click position
            Vector2 mouseDelta = mousePos - dragStartMouseScreenPos;

            // Project mouse delta onto screen space axis direction
            float projectedDistance = Vector2.Dot(mouseDelta, axisScreenDir);

            // Convert screen distance to world distance
            float distanceToCamera = Vector3.Distance(mainCamera.transform.position, dragStartPosition);
            float worldUnitsPerPixel = (2.0f * distanceToCamera * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad)) / Screen.height;

            // Apply movement along the axis
            target.position = dragStartPosition + axisDirection * (projectedDistance * worldUnitsPerPixel);
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
        }

        private Vector3 GetAxisDirection(int axis)
        {
            if (target == null) return Vector3.zero;

            switch (axis)
            {
                case 0: return target.right;
                case 1: return target.up;
                case 2: return target.forward;
                default: return Vector3.zero;
            }
        }
    }
}