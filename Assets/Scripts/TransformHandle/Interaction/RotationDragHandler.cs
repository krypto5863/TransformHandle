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
        private Vector2 rotationStartMousePos;

        // neu:
        private Vector2 centerScreen2D;
        private float pixelRadius;
        private Vector2 tangentScreenDir;

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

            // 1. Bildschirm-Mittelpunkt ermitteln
            Vector3 centerWorld = target.position;
            Vector3 centerScreen3D = mainCamera.WorldToScreenPoint(centerWorld);
            centerScreen2D = new Vector2(centerScreen3D.x, centerScreen3D.y);

            // 2. Klick-Radius und normalisierte Richtung
            Vector2 startDir = (rotationStartMousePos - centerScreen2D).normalized;
            pixelRadius = Vector2.Distance(rotationStartMousePos, centerScreen2D);

            // 3. Tangente (90° zur Radius-Richtung, CCW)
            tangentScreenDir = new Vector2(-startDir.y, startDir.x);

                        // —— Spiegeln, falls Achse vom Betrachter weg zeigt ——
            Vector3 worldAxis = GetWorldRotationAxis(draggedAxis);
            float facing = Vector3.Dot(worldAxis, mainCamera.transform.forward);
            if (facing < 0f)
                tangentScreenDir = -tangentScreenDir;
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null || draggedAxis < 0) return;
            if (pixelRadius < 0.1f) return; // Schutz vor Division durch Null

            // 4. Mausverschiebung entlang der Tangente projizieren
            Vector2 delta = mousePos - rotationStartMousePos;
            float projected = Vector2.Dot(delta, tangentScreenDir);

            // 5. Pixel-Abstand in Grad umrechnen
            float angle = (projected / pixelRadius) * Mathf.Rad2Deg;

            // 6. Welt-Rotationsachse (wie gehabt aus Start-Orientierung)
            Vector3 rotationAxis = GetWorldRotationAxis(draggedAxis);

            // 7. Rotation anwenden
            target.rotation = Quaternion.AngleAxis(angle, rotationAxis) * rotationStartOrientation;
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
        }

        private Vector3 GetWorldRotationAxis(int axis)
        {
            // Use the axes from the START orientation, not current
            switch (axis)
            {
                case 0: return rotationStartOrientation * Vector3.forward;  // X rotation (red circle)
                case 1: return rotationStartOrientation * Vector3.right;    // Y rotation (green circle)
                case 2: return rotationStartOrientation * Vector3.up;       // Z rotation (blue circle)
                default: return Vector3.up;
            }
        }
    }
}
