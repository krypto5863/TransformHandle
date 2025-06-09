using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Handles the dragging logic for rotation operations,
    /// respecting Local/Global handle space.
    /// </summary>
    public class RotationDragHandler : IDragHandler
    {
        private Camera mainCamera;
        private Transform target;
        private int draggedAxis;
        private HandleSpace handleSpace;

        private Quaternion rotationStartOrientation;
        private Vector2 rotationStartMousePos;
        private Vector2 centerScreen2D;
        private Vector2 ellipseTangent;

        private readonly float rotationResolutionMultiplier = 200f;

        public RotationDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space)
        {
            this.target = target;
            this.draggedAxis = axis;
            this.handleSpace = space;

            rotationStartOrientation = target.rotation;
            rotationStartMousePos = mousePos;

            Vector3 centerWorld = target.position;
            Vector3 centerScreen3D = mainCamera.WorldToScreenPoint(centerWorld);
            centerScreen2D = new Vector2(centerScreen3D.x, centerScreen3D.y);

            CalculateEllipseTangent(mousePos, axis);
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null || draggedAxis < 0) return;

            Vector2 delta = mousePos - rotationStartMousePos;
            float projected = Vector2.Dot(delta, ellipseTangent);

            // Convert pixel movement to radians
            float pixelsPerRadian = GetHandleScale() * rotationResolutionMultiplier;
            float angleRad = projected / pixelsPerRadian;

            // Determine rotation direction via cross of start/current
            Vector2 startDir   = (rotationStartMousePos - centerScreen2D).normalized;
            Vector2 currentDir = (mousePos - centerScreen2D).normalized;
            float crossZ = startDir.x * currentDir.y - startDir.y * currentDir.x;
            float signCross = Mathf.Sign(crossZ);

            // Determine if axis faces camera for sign correction
            Vector3 viewDir  = (target.position - mainCamera.transform.position).normalized;
            Vector3 worldAxis = GetWorldRotationAxis(draggedAxis, handleSpace);
            float facing = Vector3.Dot(worldAxis, viewDir);
            float facingSign = facing < 0f ? -1f : 1f;

            // Final angle
            angleRad = Mathf.Abs(angleRad) * signCross * facingSign;
            float angleDeg = angleRad * Mathf.Rad2Deg;

            // Apply rotation
            target.rotation = Quaternion.AngleAxis(angleDeg, worldAxis) * rotationStartOrientation;
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
        }

        private void CalculateEllipseTangent(Vector2 clickPos, int axis)
        {
            Vector3 normal = GetWorldRotationAxis(axis, handleSpace);
            Vector3 t1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (t1.sqrMagnitude < 0.1f)
                t1 = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 t2 = Vector3.Cross(normal, t1).normalized;

            // Find screen-space tangent at click angle
            Vector2 clickDir = (clickPos - centerScreen2D).normalized;
            float bestDot = -1f;
            float bestAngle = 0f;
            int samples = 36;
            for (int i = 0; i < samples; i++)
            {
                float ang = i * (360f / samples) * Mathf.Deg2Rad;
                Vector3 ptWorld = target.position + (t1 * Mathf.Cos(ang) + t2 * Mathf.Sin(ang));
                Vector3 ptScreen = mainCamera.WorldToScreenPoint(ptWorld);
                Vector2 screenDir = new Vector2(ptScreen.x - centerScreen2D.x, ptScreen.y - centerScreen2D.y).normalized;
                float dot = Vector2.Dot(clickDir, screenDir);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestAngle = ang;
                }
            }

            Vector3 tangent3D = -t1 * Mathf.Sin(bestAngle) + t2 * Mathf.Cos(bestAngle);
            Vector3 tangentEndScreen3D = mainCamera.WorldToScreenPoint(target.position + tangent3D);
            ellipseTangent = new Vector2(tangentEndScreen3D.x - centerScreen2D.x,
                                         tangentEndScreen3D.y - centerScreen2D.y).normalized;
        }

        private float GetHandleScale()
        {
            float dist = Vector3.Distance(mainCamera.transform.position, target.position);
            return dist * 0.1f;
        }

        private Vector3 GetWorldRotationAxis(int axisIndex, HandleSpace space)
        {
            if (space == HandleSpace.Local && target != null)
            {
                // Local axis in world coordinates
                switch (axisIndex)
                {
                    case 0: return rotationStartOrientation * Vector3.right;
                    case 1: return rotationStartOrientation * Vector3.up;
                    case 2: return rotationStartOrientation * Vector3.forward;
                }
            }
            // Global world axis
            switch (axisIndex)
            {
                case 0: return Vector3.right;
                case 1: return Vector3.up;
                case 2: return Vector3.forward;
                default: return Vector3.up;
            }
        }
    }
}