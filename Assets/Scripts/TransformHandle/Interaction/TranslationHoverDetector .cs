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

            return axis;
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
    }
}