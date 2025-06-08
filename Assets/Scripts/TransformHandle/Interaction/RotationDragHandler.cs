using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Handles the dragging logic for rotation operations
    /// </summary>
    public class RotationDragHandler : IDragHandler
    {
        private Camera mainCamera;
        private Transform target;
        private int draggedAxis;

        private Quaternion rotationStartOrientation;
        private Vector3 rotationAxis;
        private Vector2 rotationStartMousePos;

        public RotationDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos)
        {
            this.target = target;
            this.draggedAxis = axis;

            rotationStartOrientation = target.rotation;
            rotationStartMousePos = mousePos;

            // Set rotation axis based on which circle is selected
            switch (axis)
            {
                case 0: rotationAxis = target.forward; break;  // X rotation
                case 1: rotationAxis = target.right; break;    // Y rotation
                case 2: rotationAxis = target.up; break;        // Z rotation
            }
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null || draggedAxis < 0) return;

            // Calculate rotation based on mouse movement
            Vector3 center = target.position;
            Vector3 centerScreen = mainCamera.WorldToScreenPoint(center);

            if (centerScreen.z < 0) return;

            Vector2 centerScreen2D = new Vector2(centerScreen.x, centerScreen.y);
            Vector2 startDir = (rotationStartMousePos - centerScreen2D).normalized;
            Vector2 currentDir = (mousePos - centerScreen2D).normalized;

            // Calculate angle
            float angle = Vector2.SignedAngle(startDir, currentDir);

            // Apply rotation
            target.rotation = rotationStartOrientation * Quaternion.AngleAxis(angle, rotationAxis);
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
        }
    }
}