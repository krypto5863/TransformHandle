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
        private Vector2 dragStartScreenPos;
        private Vector3 dragStartTargetScreenPos;

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
            // Get axis direction using utils
            axisDirection = TranslationHandleUtils.GetAxisDirection(target, draggedAxis, space);
            
            // Store start positions
            dragStartScreenPos = mousePos;
            dragStartTargetScreenPos = mainCamera.WorldToScreenPoint(target.position);
        }

        private void StartPlaneDrag(Vector2 mousePos, HandleSpace space)
        {
            // Get plane normal using utils
            planeNormal = TranslationHandleUtils.GetPlaneNormal(draggedAxis, target, space);
            
            // Calculate the actual plane position (accounting for camera-facing offset)
            Vector3 camForward = mainCamera.transform.forward;
            float planeSize = TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER * GetHandleScale();
            
            var (axis1, axis2) = TranslationHandleUtils.GetPlaneAxes(target, draggedAxis, space);
            Vector3 offset = TranslationHandleUtils.CalculatePlaneOffset(axis1, axis2, planeSize, camForward);
            
            planeStartPosition = target.position + offset;

            // Calculate initial offset from plane center to hit point
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane dragPlane = new Plane(planeNormal, planeStartPosition);
            
            if (dragPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                initialOffset = target.position - hitPoint;
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
            // Calculate how much the mouse moved
            Vector2 mouseDelta = mousePos - dragStartScreenPos;
            
            // Project the axis direction to screen space
            Vector3 axisEndWorld = dragStartWorldPos + axisDirection;
            Vector3 axisEndScreen = mainCamera.WorldToScreenPoint(axisEndWorld);
            Vector2 axisScreenDirection = new Vector2(
                axisEndScreen.x - dragStartTargetScreenPos.x,
                axisEndScreen.y - dragStartTargetScreenPos.y
            ).normalized;
            
            // Calculate movement along the axis
            float screenMovement = Vector2.Dot(mouseDelta, axisScreenDirection);
            
            // Convert screen movement to world movement
            float distanceToCamera = Vector3.Distance(mainCamera.transform.position, dragStartWorldPos);
            float worldMovement = screenMovement * distanceToCamera * 0.001f;
            
            // Apply movement
            target.position = dragStartWorldPos + axisDirection * worldMovement;
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

        private float GetHandleScale()
        {
            // Estimate handle scale based on camera distance
            float distance = Vector3.Distance(mainCamera.transform.position, target.position);
            return distance * 0.1f; // Adjust multiplier as needed
        }
    }
}