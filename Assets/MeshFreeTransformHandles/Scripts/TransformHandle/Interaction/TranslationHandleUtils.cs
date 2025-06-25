using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Shared utility functions for translation handles to reduce code duplication
    /// </summary>
    public static class TranslationHandleUtils
    {
        /// <summary>
        /// Gets the axis direction based on index and space
        /// </summary>
        public static Vector3 GetAxisDirection(Transform target, int axis, HandleSpace space)
        {
            if (space == HandleSpace.Local)
            {
                switch (axis)
                {
                    case 0: return target.right;
                    case 1: return target.up;
                    case 2: return target.forward;
                    default: return Vector3.zero;
                }
            }
            else // Global
            {
                switch (axis)
                {
                    case 0: return Vector3.right;
                    case 1: return Vector3.up;
                    case 2: return Vector3.forward;
                    default: return Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Gets the two axis directions for a plane
        /// </summary>
        public static (Vector3 axis1, Vector3 axis2) GetPlaneAxes(Transform target, int planeIndex, HandleSpace space)
        {
            switch (planeIndex)
            {
                case 4: // XY Plane
                    return (GetAxisDirection(target, 0, space), GetAxisDirection(target, 1, space));
                case 5: // XZ Plane
                    return (GetAxisDirection(target, 0, space), GetAxisDirection(target, 2, space));
                case 6: // YZ Plane
                    return (GetAxisDirection(target, 1, space), GetAxisDirection(target, 2, space));
                default:
                    return (Vector3.zero, Vector3.zero);
            }
        }

        /// <summary>
        /// Gets the color for an axis or plane
        /// </summary>
        public static Color GetAxisColor(int axis)
        {
            switch (axis)
            {
                case 0: return Color.red;      // X
                case 1: return Color.green;    // Y
                case 2: return Color.blue;     // Z
                case 4: return Color.blue;     // XY Plane (Z color)
                case 5: return Color.green;    // XZ Plane (Y color)
                case 6: return Color.red;      // YZ Plane (X color)
                default: return Color.white;
            }
        }

        /// <summary>
        /// Calculates plane offset based on camera direction
        /// </summary>
        public static Vector3 CalculatePlaneOffset(Vector3 axis1, Vector3 axis2, float size, Vector3 camForward)
        {
            Vector3 offset = Vector3.zero;
            
            if (Vector3.Dot(axis1, -camForward) > 0)
                offset += axis1 * size;
                
            if (Vector3.Dot(axis2, -camForward) > 0)
                offset += axis2 * size;
                
            return offset;
        }

        /// <summary>
        /// Gets the plane normal for drag operations
        /// </summary>
        public static Vector3 GetPlaneNormal(int planeIndex, Transform target, HandleSpace space)
        {
            switch (planeIndex)
            {
                case 4: // XY Plane - Z is constant
                    return space == HandleSpace.Local ? target.forward : Vector3.forward;
                case 5: // XZ Plane - Y is constant  
                    return space == HandleSpace.Local ? target.up : Vector3.up;
                case 6: // YZ Plane - X is constant
                    return space == HandleSpace.Local ? target.right : Vector3.right;
                default:
                    return Vector3.up;
            }
        }

        /// <summary>
        /// Checks if a plane should be rendered/checked in a specific space
        /// </summary>
        public static bool IsPlaneEnabledInSpace(HandleProfile profile, int planeIndex, HandleSpace space)
        {
            return profile.IsAxisEnabled(HandleType.Translation, planeIndex, space);
        }

    }
}