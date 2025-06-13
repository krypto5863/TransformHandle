using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles the dragging logic for translation operations,
    /// supporting both single-axis and plane translations.
    /// </summary>
    public class TranslationDragHandler : IDragHandler
    {
        private Camera mainCamera;
        private Transform target;
        private int draggedAxis;
        private HandleSpace handleSpace;

        // Single axis drag
        private Vector3 dragStartWorldPos;
        private Vector3 axisDirection;
        private Vector3 dragPlaneNormal;
        private Vector3 dragStartHitPoint;

        // Plane drag
        private bool isDraggingPlane;
        private Vector3 planeNormal;
        private Vector3 planeStartPosition;
        private Vector3 initialOffset;

        public TranslationDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space)
        {
            this.target = target;
            this.draggedAxis = axis;
            this.handleSpace = space;
            
            dragStartWorldPos = target.position;
            isDraggingPlane = axis >= 4 && axis <= 6;

            if (isDraggingPlane)
            {
                StartPlaneDrag(mousePos, space);
            }
            else
            {
                StartAxisDrag(mousePos, space);
            }
        }

        private void StartAxisDrag(Vector2 mousePos, HandleSpace space)
        {
            // Get axis direction in world space
            axisDirection = GetAxisDirection(space);

            // Create drag plane
            Vector3 cameraToTarget = (target.position - mainCamera.transform.position).normalized;
            dragPlaneNormal = Vector3.Cross(axisDirection, cameraToTarget).normalized;
            
            if (dragPlaneNormal.sqrMagnitude < 0.01f)
            {
                // Axis is parallel to camera direction, use alternative plane
                Vector3 cameraRight = mainCamera.transform.right;
                dragPlaneNormal = Vector3.Cross(axisDirection, cameraRight).normalized;
            }

            // Ray-plane intersection for start position
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane dragPlane = new Plane(dragPlaneNormal, target.position);
            
            if (dragPlane.Raycast(ray, out float distance))
            {
                dragStartHitPoint = ray.GetPoint(distance);
            }
        }

        private void StartPlaneDrag(Vector2 mousePos, HandleSpace space)
        {
            // Determine plane normal based on axis index
            switch (draggedAxis)
            {
                case 4: // XY Plane - Z is constant
                    planeNormal = space == HandleSpace.Local ? target.forward : Vector3.forward;
                    break;
                case 5: // XZ Plane - Y is constant  
                    planeNormal = space == HandleSpace.Local ? target.up : Vector3.up;
                    break;
                case 6: // YZ Plane - X is constant
                    planeNormal = space == HandleSpace.Local ? target.right : Vector3.right;
                    break;
            }

            planeStartPosition = target.position;

            // Calculate initial offset from plane center to hit point
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane dragPlane = new Plane(planeNormal, planeStartPosition);
            
            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                initialOffset = planeStartPosition - hitPoint;
            }
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null) return;

            if (isDraggingPlane)
            {
                UpdatePlaneDrag(mousePos);
            }
            else
            {
                UpdateAxisDrag(mousePos);
            }
        }

        private void UpdateAxisDrag(Vector2 mousePos)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane dragPlane = new Plane(dragPlaneNormal, dragStartWorldPos);

            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 currentHitPoint = ray.GetPoint(distance);
                Vector3 dragDelta = currentHitPoint - dragStartHitPoint;
                
                // Project delta onto axis
                float axisMovement = Vector3.Dot(dragDelta, axisDirection);
                
                // Apply movement
                target.position = dragStartWorldPos + axisDirection * axisMovement;
            }
        }

        private void UpdatePlaneDrag(Vector2 mousePos)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane dragPlane = new Plane(planeNormal, planeStartPosition);

            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                
                // New position is hit point plus initial offset
                Vector3 newPosition = hitPoint + initialOffset;
                
                // The movement is already constrained to the plane by the ray-plane intersection
                target.position = newPosition;
            }
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
            isDraggingPlane = false;
        }

        private Vector3 GetAxisDirection(HandleSpace space)
        {
            switch (draggedAxis)
            {
                case 0: // X axis
                    return space == HandleSpace.Local ? target.right : Vector3.right;
                case 1: // Y axis
                    return space == HandleSpace.Local ? target.up : Vector3.up;
                case 2: // Z axis
                    return space == HandleSpace.Local ? target.forward : Vector3.forward;
                default:
                    return Vector3.zero;
            }
        }
    }
}