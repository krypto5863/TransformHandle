using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles the dragging logic for translation (movement) operations,
    /// respecting Local/Global handle space.
    /// </summary>
    public class TranslationDragHandler : IDragHandler
    {
        private Camera mainCamera;
        private Transform target;
        private int draggedAxis;
        private HandleSpace handleSpace;

        private Vector3 dragStartPosition;
        private Vector2 dragStartMouseScreenPos;

        public TranslationDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space)
        {
            this.target = target;
            this.draggedAxis = axis;
            this.handleSpace = space;

            dragStartPosition = target.position;
            dragStartMouseScreenPos = mousePos;
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null || draggedAxis < 0) return;

            // Determine axis direction based on space
            Vector3 axisDirection = GetAxisDirection(draggedAxis, handleSpace);

            // Project axis to screen
            Vector3 screenOrigin = mainCamera.WorldToScreenPoint(dragStartPosition);
            Vector3 screenEnd    = mainCamera.WorldToScreenPoint(dragStartPosition + axisDirection);

            Vector2 axisScreenDir = (new Vector2(screenEnd.x, screenEnd.y) - new Vector2(screenOrigin.x, screenOrigin.y)).normalized;

            // Mouse movement
            Vector2 mouseDelta = mousePos - dragStartMouseScreenPos;
            float projectedDistance = Vector2.Dot(mouseDelta, axisScreenDir);

            // Convert screen delta to world units
            float distToCam = Vector3.Distance(mainCamera.transform.position, dragStartPosition);
            float worldPerPixel = (2f * distToCam * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad)) / Screen.height;

            target.position = dragStartPosition + axisDirection * (projectedDistance * worldPerPixel);
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
        }

        private Vector3 GetAxisDirection(int axisIndex, HandleSpace space)
        {
            switch (axisIndex)
            {
                case 0: return space == HandleSpace.Local ? target.right   : Vector3.right;
                case 1: return space == HandleSpace.Local ? target.up      : Vector3.up;
                case 2: return space == HandleSpace.Local ? target.forward : Vector3.forward;
                default: return Vector3.zero;
            }
        }
    }
}
