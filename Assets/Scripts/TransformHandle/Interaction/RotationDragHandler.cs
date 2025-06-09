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

        private Vector2 centerScreen2D;
        private Vector2 ellipseTangent;

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

            // Get center in screen space
            Vector3 centerWorld = target.position;
            Vector3 centerScreen3D = mainCamera.WorldToScreenPoint(centerWorld);
            centerScreen2D = new Vector2(centerScreen3D.x, centerScreen3D.y);

            // Calculate ellipse tangent at click point
            CalculateEllipseTangent(mousePos, axis);
        }

        private void CalculateEllipseTangent(Vector2 clickPos, int axis)
        {
            Vector3 worldAxis = GetWorldRotationAxis(axis);

            // Compute two orthonormal tangents in the rotation plane
            Vector3 tangent1 = Vector3.Cross(worldAxis, Vector3.up).normalized;
            if (tangent1.magnitude < 0.1f)
                tangent1 = Vector3.Cross(worldAxis, Vector3.right).normalized;
            Vector3 tangent2 = Vector3.Cross(worldAxis, tangent1).normalized;

            // Find the angle on the circle that corresponds to the click position
            // We use the click direction from center to determine the angle
            Vector2 clickDir = (clickPos - centerScreen2D).normalized;
            
            // Find the best matching angle by projecting screen directions
            float bestAngle = 0f;
            float maxDot = -1f;
            
            // Sample fewer points for performance
            for (int i = 0; i < 36; i++) // 36 samples = every 10 degrees
            {
                float angle = i * 10f * Mathf.Deg2Rad;
                Vector3 pointOnCircle = target.position
                    + (tangent1 * Mathf.Cos(angle) + tangent2 * Mathf.Sin(angle));
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(pointOnCircle);
                Vector2 screenDir = new Vector2(screenPoint.x - centerScreen2D.x, 
                                               screenPoint.y - centerScreen2D.y).normalized;

                float dot = Vector2.Dot(clickDir, screenDir);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestAngle = angle;
                }
            }

            // Compute the 3D tangent direction at this angle
            Vector3 tangentDir3D = -tangent1 * Mathf.Sin(bestAngle) + tangent2 * Mathf.Cos(bestAngle);

            // Project the tangent to screen space
            Vector3 tangentEndWorld = target.position + tangentDir3D;
            Vector3 tangentEndScreen = mainCamera.WorldToScreenPoint(tangentEndWorld);

            ellipseTangent = new Vector2(
                tangentEndScreen.x - centerScreen2D.x,
                tangentEndScreen.y - centerScreen2D.y
            ).normalized;
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            // Calculate raw projection onto ellipse tangent
            Vector2 delta = mousePos - rotationStartMousePos;
            float projected = Vector2.Dot(delta, ellipseTangent);

            // Convert pixel movement to radians
            float pixelsPerRadian = GetHandleScale() * 50f;
            float angleRad = projected / pixelsPerRadian;

            // Determine rotation direction via 2D cross product of radius vectors
            Vector2 startDir = (rotationStartMousePos - centerScreen2D).normalized;
            Vector2 currentDir = (mousePos - centerScreen2D).normalized;
            float crossZ = startDir.x * currentDir.y - startDir.y * currentDir.x;
            float signCross = Mathf.Sign(crossZ);

            // Determine if axis faces camera or away for final sign
            Vector3 viewDir = (target.position - mainCamera.transform.position).normalized;
            Vector3 worldAxis = GetWorldRotationAxis(draggedAxis);
            float facing = Vector3.Dot(worldAxis, viewDir);
            float facingSign = facing < 0f ? -1f : 1f;

            // Apply sign corrections
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

        private float GetHandleScale()
        {
            float distance = Vector3.Distance(mainCamera.transform.position, target.position);
            return distance * 0.1f;
        }

        private Vector3 GetWorldRotationAxis(int axis)
        {
            switch (axis)
            {
                case 0: return rotationStartOrientation * Vector3.forward;
                case 1: return rotationStartOrientation * Vector3.right;
                case 2: return rotationStartOrientation * Vector3.up;
                default: return Vector3.up;
            }
        }
    }
}