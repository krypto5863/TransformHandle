using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Handles hover detection specifically for translation handles
    /// </summary>
    public class TranslationHoverDetector : BaseHoverDetector
    {
        public TranslationHoverDetector(Camera camera) : base(camera) { }

        public override int GetHoveredAxis(Vector2 mousePos, Transform target, float handleScale, HandleSpace handleSpace)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            // Check regular axes first (0-2)
            for (int i = 0; i < 3; i++)
            {
                Vector3 dir = GetAxisDirection(target, i, handleSpace);
                float dist = GetDistanceToAxis(mousePos, target.position, dir, handleScale);
                
                if (dist < minDist && dist < TRANSLATION_THRESHOLD)
                {
                    minDist = dist;
                    axis = i;
                }
            }

            // Check plane handles (4-6)
            float planeDist;
            float planeScale = handleScale * TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER;
            Camera cam = Camera.main;
            Vector3 camForward = cam.transform.forward;
            
            // XY Plane
            Vector3 dirX = GetAxisDirection(target, 0, handleSpace);
            Vector3 dirY = GetAxisDirection(target, 1, handleSpace);
            Vector3 offsetXY = CalculatePlaneOffset(dirX, dirY, planeScale, camForward);
            planeDist = GetDistanceToPlane(mousePos, target.position + offsetXY, dirX, dirY, planeScale);
            if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
            {
                minDist = planeDist;
                axis = 4;
            }

            // XZ Plane
            Vector3 dirZ = GetAxisDirection(target, 2, handleSpace);
            Vector3 offsetXZ = CalculatePlaneOffset(dirX, dirZ, planeScale, camForward);
            planeDist = GetDistanceToPlane(mousePos, target.position + offsetXZ, dirX, dirZ, planeScale);
            if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
            {
                minDist = planeDist;
                axis = 5;
            }

            // YZ Plane
            Vector3 offsetYZ = CalculatePlaneOffset(dirY, dirZ, planeScale, camForward);
            planeDist = GetDistanceToPlane(mousePos, target.position + offsetYZ, dirY, dirZ, planeScale);
            if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
            {
                minDist = planeDist;
                axis = 6;
            }

            return axis;
        }

        public override int GetHoveredAxisWithProfile(Vector2 mousePos, Transform target, float handleScale, HandleProfile profile)
        {
            float minDist = float.MaxValue;
            int axis = -1;

            for (int i = 0; i < 3; i++)
            {
                // Check local space
                if (profile.IsAxisEnabled(HandleType.Translation, i, HandleSpace.Local))
                {
                    float dist = GetDistanceToAxisInSpace(mousePos, target, i, handleScale, HandleSpace.Local);
                    if (dist < minDist && dist < TRANSLATION_THRESHOLD)
                    {
                        minDist = dist;
                        axis = i;
                    }
                }

                // Check global space
                if (profile.IsAxisEnabled(HandleType.Translation, i, HandleSpace.Global))
                {
                    float dist = GetDistanceToAxisInSpace(mousePos, target, i, handleScale, HandleSpace.Global);
                    if (dist < minDist && dist < TRANSLATION_THRESHOLD)
                    {
                        minDist = dist;
                        axis = i;
                    }
                }
            }

            // Check plane handles
            CheckPlanesWithProfile(mousePos, target, handleScale, profile, ref minDist, ref axis);

            return axis;
        }

        private void CheckPlanesWithProfile(Vector2 mousePos, Transform target, float handleScale, 
                                           HandleProfile profile, ref float minDist, ref int axis)
        {
            float planeSize = handleScale * TranslationHandleRenderer.PLANE_SIZE_MULTIPLIER;
            float planeDist;
            Camera cam = Camera.main;
            Vector3 camForward = cam.transform.forward;

            // XY Plane (axis 4)
            if (profile.IsAxisEnabled(HandleType.Translation, 4, HandleSpace.Local))
            {
                Vector3 dirX = GetAxisDirection(target, 0, HandleSpace.Local);
                Vector3 dirY = GetAxisDirection(target, 1, HandleSpace.Local);
                Vector3 offset = CalculatePlaneOffset(dirX, dirY, planeSize, camForward);
                planeDist = GetDistanceToPlane(mousePos, target.position + offset, dirX, dirY, planeSize);
                if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
                {
                    minDist = planeDist;
                    axis = 4;
                }
            }
            if (profile.IsAxisEnabled(HandleType.Translation, 4, HandleSpace.Global))
            {
                Vector3 offset = CalculatePlaneOffset(Vector3.right, Vector3.up, planeSize, camForward);
                planeDist = GetDistanceToPlane(mousePos, target.position + offset, Vector3.right, Vector3.up, planeSize);
                if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
                {
                    minDist = planeDist;
                    axis = 4;
                }
            }

            // XZ Plane (axis 5)
            if (profile.IsAxisEnabled(HandleType.Translation, 5, HandleSpace.Local))
            {
                Vector3 dirX = GetAxisDirection(target, 0, HandleSpace.Local);
                Vector3 dirZ = GetAxisDirection(target, 2, HandleSpace.Local);
                Vector3 offset = CalculatePlaneOffset(dirX, dirZ, planeSize, camForward);
                planeDist = GetDistanceToPlane(mousePos, target.position + offset, dirX, dirZ, planeSize);
                if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
                {
                    minDist = planeDist;
                    axis = 5;
                }
            }
            if (profile.IsAxisEnabled(HandleType.Translation, 5, HandleSpace.Global))
            {
                Vector3 offset = CalculatePlaneOffset(Vector3.right, Vector3.forward, planeSize, camForward);
                planeDist = GetDistanceToPlane(mousePos, target.position + offset, Vector3.right, Vector3.forward, planeSize);
                if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
                {
                    minDist = planeDist;
                    axis = 5;
                }
            }

            // YZ Plane (axis 6)
            if (profile.IsAxisEnabled(HandleType.Translation, 6, HandleSpace.Local))
            {
                Vector3 dirY = GetAxisDirection(target, 1, HandleSpace.Local);
                Vector3 dirZ = GetAxisDirection(target, 2, HandleSpace.Local);
                Vector3 offset = CalculatePlaneOffset(dirY, dirZ, planeSize, camForward);
                planeDist = GetDistanceToPlane(mousePos, target.position + offset, dirY, dirZ, planeSize);
                if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
                {
                    minDist = planeDist;
                    axis = 6;
                }
            }
            if (profile.IsAxisEnabled(HandleType.Translation, 6, HandleSpace.Global))
            {
                Vector3 offset = CalculatePlaneOffset(Vector3.up, Vector3.forward, planeSize, camForward);
                planeDist = GetDistanceToPlane(mousePos, target.position + offset, Vector3.up, Vector3.forward, planeSize);
                if (planeDist < minDist && planeDist < TRANSLATION_THRESHOLD)
                {
                    minDist = planeDist;
                    axis = 6;
                }
            }
        }

        private float GetDistanceToAxisInSpace(Vector2 mousePos, Transform target, int axisIndex, float scale, HandleSpace space)
        {
            Vector3 dir = GetAxisDirection(target, axisIndex, space);
            return GetDistanceToAxis(mousePos, target.position, dir, scale);
        }

        private float GetDistanceToAxis(Vector2 mousePos, Vector3 origin, Vector3 direction, float scale)
        {
            Vector3 endPoint = origin + direction * scale;
            
            if (IsPointBehindCamera(origin) || IsPointBehindCamera(endPoint))
                return float.MaxValue;

            Vector3 originScreen = mainCamera.WorldToScreenPoint(origin);
            Vector3 endScreen = mainCamera.WorldToScreenPoint(endPoint);

            return DistancePointToLineSegment(
                mousePos, 
                new Vector2(originScreen.x, originScreen.y), 
                new Vector2(endScreen.x, endScreen.y)
            );
        }

        private float GetDistanceToPlane(Vector2 mousePos, Vector3 center, Vector3 axis1, Vector3 axis2, float size)
        {
            // Calculate plane corners - matching the renderer's corner calculation
            Vector3[] corners = new Vector3[4];
            corners[0] = center;
            corners[1] = center - axis1 * size;
            corners[2] = center - axis1 * size - axis2 * size;
            corners[3] = center - axis2 * size;

            // Check if any corner is behind camera
            for (int i = 0; i < 4; i++)
            {
                if (IsPointBehindCamera(corners[i]))
                    return float.MaxValue;
            }

            // Convert to screen space
            Vector2[] screenCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(corners[i]);
                screenCorners[i] = new Vector2(screenPos.x, screenPos.y);
            }

            // Point in polygon test
            if (IsPointInQuad(mousePos, screenCorners))
            {
                return 0f; // Inside the plane
            }

            // Otherwise, distance to edges
            float minDist = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                float dist = DistancePointToLineSegment(mousePos, screenCorners[i], screenCorners[next]);
                minDist = Mathf.Min(minDist, dist);
            }

            return minDist;
        }

        private bool IsPointInQuad(Vector2 point, Vector2[] quad)
        {
            // Simple point-in-polygon test using cross products
            bool sign = false;
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                Vector2 edge = quad[next] - quad[i];
                Vector2 toPoint = point - quad[i];
                float cross = edge.x * toPoint.y - edge.y * toPoint.x;
                
                if (i == 0)
                    sign = cross > 0;
                else if ((cross > 0) != sign)
                    return false;
            }
            return true;
        }

        private Vector3 CalculatePlaneOffset(Vector3 axis1, Vector3 axis2, float size, Vector3 camForward)
        {
            Vector3 offset = Vector3.zero;
            
            // Check if axis1 points towards camera
            if (Vector3.Dot(axis1, -camForward) > 0)
                offset += axis1 * size;
                
            // Check if axis2 points towards camera
            if (Vector3.Dot(axis2, -camForward) > 0)
                offset += axis2 * size;
                
            return offset;
        }
    }
}