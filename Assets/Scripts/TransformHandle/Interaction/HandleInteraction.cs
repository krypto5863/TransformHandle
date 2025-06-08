using UnityEngine;
using UnityEngine.InputSystem;

namespace TransformHandle
{
    /// <summary>
    /// Manages user input and coordinates interaction components
    /// </summary>
    public class HandleInteraction
    {
        private Camera mainCamera;
        private Transform target;
        
        // Sub-components
        private HandleHoverDetector hoverDetector;
        private IDragHandler translationHandler;
        private IDragHandler rotationHandler;
        private IDragHandler currentDragHandler;
        
        // Interaction state
        public int HoveredAxis { get; private set; } = -1;
        public bool IsDragging { get; private set; }
        public int DraggedAxis { get; private set; } = -1;
        
        public HandleInteraction(Camera camera)
        {
            mainCamera = camera;
            
            // Initialize sub-components
            hoverDetector = new HandleHoverDetector(camera);
            translationHandler = new TranslationDragHandler(camera);
            rotationHandler = new RotationDragHandler(camera);
        }
        
        public void UpdateTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void Update(float handleScale, HandleType handleType)
        {
            if (target == null || mainCamera == null) return;
            
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool mouseReleased = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
            
            if (!IsDragging)
            {
                // Update hover state
                HoveredAxis = hoverDetector.GetHoveredAxis(mousePos, target, handleScale, handleType);
                
                // Start drag if mouse pressed on a handle
                if (mousePressed && HoveredAxis >= 0)
                {
                    StartDrag(handleType, mousePos);
                }
            }
            else
            {
                // Update current drag
                currentDragHandler?.UpdateDrag(mousePos);
                
                // End drag if mouse released
                if (mouseReleased)
                {
                    EndDrag();
                }
            }
        }
        
        private void StartDrag(HandleType handleType, Vector2 mousePos)
        {
            IsDragging = true;
            DraggedAxis = HoveredAxis;
            
            // Select appropriate drag handler
            switch (handleType)
            {
                case HandleType.Translation:
                    currentDragHandler = translationHandler;
                    break;
                case HandleType.Rotation:
                    currentDragHandler = rotationHandler;
                    break;
                default:
                    return;
            }
            
            currentDragHandler.StartDrag(target, DraggedAxis, mousePos);
        }
        
        private void EndDrag()
        {
            currentDragHandler?.EndDrag();
            currentDragHandler = null;
            
            IsDragging = false;
            DraggedAxis = -1;
        }
    }
}