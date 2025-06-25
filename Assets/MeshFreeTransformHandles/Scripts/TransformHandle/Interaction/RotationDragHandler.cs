using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles the dragging logic for rotation operations,
    /// respecting Local/Global handle space and supporting free rotation.
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

        // Incremental rotation system
        private Quaternion incrementQ;
        private Vector3 startAxisWorld;
        private float pixelsPerIncrement = 30f;  // 30px → one step
        private const float incrementAngle = 15f; // 15° per step for smoother control

        // Free rotation specific
        private bool isFreeRotation;
        private Vector2 lastMousePos;

        public RotationDragHandler(Camera camera)
        {
            mainCamera = camera;
        }

        public void StartDrag(Transform target, int axis, Vector2 mousePos, HandleSpace space)
        {
            this.target = target;
            this.draggedAxis = axis;
            this.isFreeRotation = (axis == 3);

            // Store start rotation and mouse position
            rotationStartOrientation = target.rotation;
            rotationStartMousePos = mousePos;
            lastMousePos = mousePos;

            // Calculate screen center of rotation
            Vector3 centerWorld = target.position;
            Vector3 centerScreen3D = mainCamera.WorldToScreenPoint(centerWorld);
            centerScreen2D = new Vector2(centerScreen3D.x, centerScreen3D.y);

            if (isFreeRotation)
            {
                // Free rotation uses camera-facing axis
                startAxisWorld = (mainCamera.transform.position - target.position).normalized;
            }
            else
            {
                // Regular axis rotation
                if (space == HandleSpace.Local)
                {
                    switch (axis)
                    {
                        case 0: startAxisWorld = target.right; break;
                        case 1: startAxisWorld = target.up; break;
                        case 2: startAxisWorld = target.forward; break;
                    }
                }
                else
                {
                    // Global axes
                    switch (axis)
                    {
                        case 0: startAxisWorld = Vector3.right; break;
                        case 1: startAxisWorld = Vector3.up; break;
                        case 2: startAxisWorld = Vector3.forward; break;
                    }
                }
            }

            // Calculate ellipse tangent for ALL rotations (including free rotation)
            CalculateEllipseTangent(mousePos, axis);
            
            // Prepare incremental quaternion around the axis
            incrementQ = Quaternion.AngleAxis(incrementAngle, startAxisWorld);
        }

        public void UpdateDrag(Vector2 mousePos)
        {
            if (target == null || draggedAxis < 0) return;

            // Use the same tangent-based rotation for all axes (including free rotation)
            UpdateAxisRotation(mousePos);
            
            lastMousePos = mousePos;
        }

        private void UpdateAxisRotation(Vector2 mousePos)
        {
            // Project mouse delta onto tangent
            Vector2 delta = mousePos - rotationStartMousePos;
            float proj = Vector2.Dot(delta, ellipseTangent);

            // Calculate number of steps (can be negative)
            float steps = proj / pixelsPerIncrement;

            // Delta quaternion as "incrementQ ^ steps"
            Quaternion deltaQ = QuaternionPow(incrementQ, steps);

            // Apply to start rotation
            target.rotation = deltaQ * rotationStartOrientation;
        }

        public void EndDrag()
        {
            target = null;
            draggedAxis = -1;
            isFreeRotation = false;
        }

        private void CalculateEllipseTangent(Vector2 clickPos, int axis)
        {
            // Use the stored start axis
            Vector3 normal = startAxisWorld;

            Vector3 t1 = Vector3.Cross(normal, Vector3.up).normalized;
            if (t1.sqrMagnitude < 0.1f)
                t1 = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 t2 = Vector3.Cross(normal, t1).normalized;

            // Find screen-space tangent at click angle
            Vector2 clickDir = (clickPos - centerScreen2D).normalized;
            float bestDot = -1f;
            float bestAngle = 0f;
            int samples = 36;
            float radius = GetHandleScale();
            
            // Use larger radius for free rotation
            if (axis == 3)
                radius *= 1.2f;

            for (int i = 0; i < samples; i++)
            {
                float ang = i * (360f / samples) * Mathf.Deg2Rad;
                Vector3 ptWorld = target.position + (t1 * Mathf.Cos(ang) + t2 * Mathf.Sin(ang)) * radius;
                Vector3 ptScreen = mainCamera.WorldToScreenPoint(ptWorld);
                
                if (ptScreen.z <= 0) continue;
                
                Vector2 screenDir = new Vector2(ptScreen.x - centerScreen2D.x, ptScreen.y - centerScreen2D.y).normalized;
                float dot = Vector2.Dot(clickDir, screenDir);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestAngle = ang;
                }
            }

            Vector3 tangent3D = -t1 * Mathf.Sin(bestAngle) + t2 * Mathf.Cos(bestAngle);
            Vector3 tangentEndScreen3D = mainCamera.WorldToScreenPoint(target.position + tangent3D * radius);
            ellipseTangent = new Vector2(tangentEndScreen3D.x - centerScreen2D.x,
                                         tangentEndScreen3D.y - centerScreen2D.y).normalized;
        }

        private float GetHandleScale()
        {
            float dist = Vector3.Distance(mainCamera.transform.position, target.position);
            return dist * 0.1f;
        }

        // Helper function: raises quaternion to power t
        private static Quaternion QuaternionPow(Quaternion q, float t)
        {
            // Normalize to positive w
            if (q.w < 0f)
                q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
            
            float alpha = Mathf.Acos(Mathf.Clamp(q.w, -1f, 1f));
            
            // For very small angles, use linear approximation
            if (Mathf.Abs(alpha) < 0.0001f)
                return new Quaternion(q.x * t, q.y * t, q.z * t, Mathf.Cos(alpha * t)).normalized;

            float newAlpha = alpha * t;
            float mult = Mathf.Sin(newAlpha) / Mathf.Sin(alpha);
            return new Quaternion(q.x * mult, q.y * mult, q.z * mult, Mathf.Cos(newAlpha));
        }
    }
}