using UnityEngine;
using UnityEngine.InputSystem;

namespace MeshFreeHandles
{
    /// <summary>
    /// Manages user input and coordinates interaction components,
    /// now supporting Local/Global handle space.
    /// </summary>
    public class HandleInteraction
    {
        private Camera mainCamera;
        private Transform target;

        // Sub-components
        private HandleHoverDetector hoverDetector;
        private IDragHandler translationHandler;
        private IDragHandler rotationHandler;
        private IDragHandler scaleHandler;
        private IDragHandler currentDragHandler;

        // Interaction state
        public int HoveredAxis { get; private set; } = -1;
        public bool IsDragging   { get; private set; }
        public int DraggedAxis   { get; private set; } = -1;

        public HandleInteraction(Camera camera)
        {
            mainCamera = camera;
            hoverDetector      = new HandleHoverDetector(camera);
            translationHandler = new TranslationDragHandler(camera);
            rotationHandler    = new RotationDragHandler(camera);
            scaleHandler       = new ScaleDragHandler(camera);
        }

        public void UpdateTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Updates hover and drag state, taking into account handle type and space.
        /// </summary>
        public void Update(float handleScale, HandleType handleType, HandleSpace handleSpace)
        {
            if (target == null || mainCamera == null) return;

            Vector2 mousePos     = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            bool    mousePressed = Mouse.current?.leftButton.wasPressedThisFrame  ?? false;
            bool    mouseReleased= Mouse.current?.leftButton.wasReleasedThisFrame ?? false;

            if (!IsDragging)
            {
                // Update hover state with space
                HoveredAxis = hoverDetector.GetHoveredAxis(mousePos, target, handleScale, handleType, handleSpace);

                // Start drag if mouse pressed on a handle
                if (mousePressed && HoveredAxis >= 0)
                    StartDrag(handleType, handleSpace, mousePos);
            }
            else
            {
                // Continue drag
                currentDragHandler?.UpdateDrag(mousePos);

                // End drag if mouse released
                if (mouseReleased)
                    EndDrag();
            }
        }

        private void StartDrag(HandleType handleType, HandleSpace handleSpace, Vector2 mousePos)
        {
            IsDragging  = true;
            DraggedAxis = HoveredAxis;

            // Choose handler
            switch (handleType)
            {
                case HandleType.Translation:
                    currentDragHandler = translationHandler;
                    break;
                case HandleType.Rotation:
                    currentDragHandler = rotationHandler;
                    break;
                case HandleType.Scale:
                    currentDragHandler = scaleHandler;
                    break;
                default:
                    return;
            }

            // Pass space into StartDrag
            currentDragHandler.StartDrag(target, DraggedAxis, mousePos, handleSpace);
        }

        private void EndDrag()
        {
            currentDragHandler?.EndDrag();
            currentDragHandler = null;
            IsDragging  = false;
            DraggedAxis = -1;
        }
    }
}