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
        private Vector2 dragStartMouseScreenPos; // Mouse position at drag start
        
        // Rotation data
        private Quaternion rotationStartOrientation;
        private Vector3 rotationAxis;
        private Vector2 rotationStartMousePos;
        
        public HandleInteraction(Camera camera)
        {
            mainCamera = camera;
        }
        
        public void UpdateTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void Update(float handleScale, HandleType handleType)
        {
            if (target == null || mainCamera == null) return;
            
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            bool mouseDown = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool mouseReleased = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
            
            if (!IsDragging)
            {
                UpdateHoverState(mousePos, handleScale, handleType);
                
                if (mousePressed && HoveredAxis >= 0)
                {
                    if (handleType == HandleType.Translation)
                        StartDrag();
                    else if (handleType == HandleType.Rotation)
                        StartRotation();
                }
            }
            else
            {
                if (handleType == HandleType.Translation)
                    UpdateDrag(mousePos);
                else if (handleType == HandleType.Rotation)
                    UpdateRotation(mousePos);
                
                if (mouseReleased)
                {
                    EndDrag();
                }
            }
        }
        
        private void UpdateHoverState(Vector2 mousePos, float handleScale, HandleType handleType)
        {
            Vector3 handleScreenPos = mainCamera.WorldToScreenPoint(target.position);
            
            if (handleScreenPos.z < 0)
            {
                HoveredAxis = -1;
                return;
            }
            
            if (handleType == HandleType.Translation)
                HoveredAxis = GetClosestAxis(mousePos, handleScale);
            else if (handleType == HandleType.Rotation)
                HoveredAxis = GetClosestRotationAxis(mousePos, handleScale);
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
            
            // Store the initial mouse position in screen space
            dragStartMouseScreenPos = Mouse.current.position.ReadValue();
        }
        
        private void UpdateDrag(Vector2 mousePos)
        {
            if (DraggedAxis < 0) return;
            
            // Get axis direction in world space
            Vector3 axisDirection = GetAxisDirection(DraggedAxis);
            
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
        
        private int GetClosestRotationAxis(Vector2 mousePos, float handleScale)
        {
            float closestDistance = float.MaxValue;
            int closestAxis = -1;
            
            Vector3[] axisNormals = {
                target.forward,  // X rotation (red circle)
                target.right,    // Y rotation (green circle)
                target.up        // Z rotation (blue circle)
            };
            
            for (int i = 0; i < 3; i++)
            {
                float distance = GetDistanceToCircle(mousePos, axisNormals[i], handleScale);
                if (distance < closestDistance && distance < 15f) // 15 pixel threshold for circles
                {
                    closestDistance = distance;
                    closestAxis = i;
                }
            }
            
            return closestAxis;
        }
        
        private float GetDistanceToCircle(Vector2 mousePos, Vector3 normal, float radius)
        {
            Vector3 center = target.position;
            
            // Calculate circle points in world space
            Vector3 tangent1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (tangent1.magnitude < 0.1f)
            {
                tangent1 = Vector3.Cross(normal, Vector3.right).normalized;
            }
            Vector3 tangent2 = Vector3.Cross(normal, tangent1).normalized;
            
            float minDistance = float.MaxValue;
            int segments = 32;
            
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * 2 * Mathf.PI;
                Vector3 worldPoint = center + (tangent1 * Mathf.Cos(angle) + tangent2 * Mathf.Sin(angle)) * radius;
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPoint);
                
                if (screenPoint.z > 0)
                {
                    float distance = Vector2.Distance(mousePos, new Vector2(screenPoint.x, screenPoint.y));
                    minDistance = Mathf.Min(minDistance, distance);
                }
            }
            
            return minDistance;
        }
        
        private void StartRotation()
        {
            IsDragging = true;
            DraggedAxis = HoveredAxis;
            rotationStartOrientation = target.rotation;
            rotationStartMousePos = Mouse.current.position.ReadValue();
            
            // Set rotation axis based on which circle is selected
            switch (DraggedAxis)
            {
                case 0: rotationAxis = target.forward; break;  // X rotation
                case 1: rotationAxis = target.right; break;    // Y rotation
                case 2: rotationAxis = target.up; break;        // Z rotation
            }
        }
        
        private void UpdateRotation(Vector2 mousePos)
        {
            if (DraggedAxis < 0) return;
            
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
    }
}