using UnityEngine;
using UnityEngine.InputSystem;

namespace TransformHandle
{
    /// <summary>
    /// Handles user input and interaction with transform handles
    /// </summary>
    public class HandleInteraction
    {
        private Camera mainCamera;
        private Transform target;
        
        // Interaction state
        public int HoveredAxis { get; private set; } = -1; // -1 = none, 0 = X, 1 = Y, 2 = Z
        public bool IsDragging { get; private set; }
        public int DraggedAxis { get; private set; } = -1;
        
        // Drag data
        private Vector3 dragStartPosition;
        private Vector3 dragStartMousePosition;
        private Plane dragPlane;
        
        public HandleInteraction(Camera camera)
        {
            mainCamera = camera;
        }
        
        public void UpdateTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void Update(float handleScale)
        {
            if (target == null || mainCamera == null) return;
            
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            bool mouseDown = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool mouseReleased = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
            
            if (!IsDragging)
            {
                UpdateHoverState(mousePos, handleScale);
                
                if (mousePressed && HoveredAxis >= 0)
                {
                    StartDrag();
                }
            }
            else
            {
                UpdateDrag(mousePos);
                
                if (mouseReleased)
                {
                    EndDrag();
                }
            }
        }
        
        private void UpdateHoverState(Vector2 mousePos, float handleScale)
        {
            Vector3 handleScreenPos = mainCamera.WorldToScreenPoint(target.position);
            
            if (handleScreenPos.z < 0)
            {
                HoveredAxis = -1;
                return;
            }
            
            HoveredAxis = GetClosestAxis(mousePos, handleScale);
        }
        
        private int GetClosestAxis(Vector2 mousePos, float handleScale)
        {
            float closestDistance = float.MaxValue;
            int closestAxis = -1;
            
            Vector3[] axisDirections = {
                target.right,    // X
                target.up,       // Y
                target.forward   // Z
            };
            
            for (int i = 0; i < 3; i++)
            {
                float distance = GetDistanceToAxis(mousePos, axisDirections[i], handleScale);
                if (distance < closestDistance && distance < 10f) // 10 pixel threshold
                {
                    closestDistance = distance;
                    closestAxis = i;
                }
            }
            
            return closestAxis;
        }
        
        private float GetDistanceToAxis(Vector2 mousePos, Vector3 axisDirection, float handleScale)
        {
            Vector3 origin = target.position;
            Vector3 endPoint = origin + axisDirection * handleScale;
            
            Vector3 screenOrigin = mainCamera.WorldToScreenPoint(origin);
            Vector3 screenEnd = mainCamera.WorldToScreenPoint(endPoint);
            
            if (screenOrigin.z < 0 || screenEnd.z < 0) return float.MaxValue;
            
            Vector2 lineStart = new Vector2(screenOrigin.x, screenOrigin.y);
            Vector2 lineEnd = new Vector2(screenEnd.x, screenEnd.y);
            
            return DistancePointToLineSegment(mousePos, lineStart, lineEnd);
        }
        
        private float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.magnitude;
            if (lineLength < 0.001f) return Vector2.Distance(point, lineStart);
            
            float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, line) / (lineLength * lineLength));
            Vector2 projection = lineStart + t * line;
            
            return Vector2.Distance(point, projection);
        }
        
        private void StartDrag()
        {
            IsDragging = true;
            DraggedAxis = HoveredAxis;
            dragStartPosition = target.position;
            
            // Get mouse position
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            
            // Create drag plane
            Vector3 axisDirection = GetAxisDirection(DraggedAxis);
            Vector3 planeNormal = Vector3.Cross(axisDirection, mainCamera.transform.right);
            if (planeNormal.magnitude < 0.1f)
            {
                planeNormal = Vector3.Cross(axisDirection, mainCamera.transform.up);
            }
            planeNormal.Normalize();
            
            dragPlane = new Plane(planeNormal, target.position);
            
            // Store initial mouse position on plane
            float enter;
            if (dragPlane.Raycast(ray, out enter))
            {
                dragStartMousePosition = ray.GetPoint(enter);
            }
        }
        
        private void UpdateDrag(Vector2 mousePos)
        {
            if (DraggedAxis < 0) return;
            
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            float enter;
            
            if (dragPlane.Raycast(ray, out enter))
            {
                Vector3 currentMousePosition = ray.GetPoint(enter);
                Vector3 mouseDelta = currentMousePosition - dragStartMousePosition;
                
                // Project delta onto axis
                Vector3 axisDirection = GetAxisDirection(DraggedAxis);
                float distance = Vector3.Dot(mouseDelta, axisDirection);
                
                // Apply movement
                target.position = dragStartPosition + axisDirection * distance;
            }
        }
        
        private void EndDrag()
        {
            IsDragging = false;
            DraggedAxis = -1;
        }
        
        private Vector3 GetAxisDirection(int axis)
        {
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