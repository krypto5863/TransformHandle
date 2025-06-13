using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// ScriptableObject that defines handle configuration for transform manipulation
    /// </summary>
    [CreateAssetMenu(fileName = "HandleProfile", menuName = "MeshFreeHandles/Handle Profile")]
    public class HandleProfile : ScriptableObject
    {
        [Header("Translation Handles")]
        [SerializeField] private bool showTranslationLocalX = true;
        [SerializeField] private bool showTranslationLocalY = true;
        [SerializeField] private bool showTranslationLocalZ = true;
        [SerializeField] private bool showTranslationGlobalX = false;
        [SerializeField] private bool showTranslationGlobalY = false;
        [SerializeField] private bool showTranslationGlobalZ = false;

        [Header("Rotation Handles")]
        [SerializeField] private bool showRotationLocalX = true;
        [SerializeField] private bool showRotationLocalY = false;
        [SerializeField] private bool showRotationLocalZ = true;
        [SerializeField] private bool showRotationGlobalX = false;
        [SerializeField] private bool showRotationGlobalY = true;
        [SerializeField] private bool showRotationGlobalZ = false;

        [Header("Scale Handles")]
        [SerializeField] private bool showScaleLocalX = true;
        [SerializeField] private bool showScaleLocalY = true;
        [SerializeField] private bool showScaleLocalZ = true;
        [SerializeField] private bool showScaleGlobalX = false;
        [SerializeField] private bool showScaleGlobalY = false;
        [SerializeField] private bool showScaleGlobalZ = false;
        [SerializeField] private bool showUniformScale = true;

        /// <summary>
        /// Checks if a specific axis should be shown for the given handle type and space
        /// </summary>
        public bool IsAxisEnabled(HandleType handleType, int axis, HandleSpace space)
        {
            switch (handleType)
            {
                case HandleType.Translation:
                    if (space == HandleSpace.Local)
                    {
                        switch (axis)
                        {
                            case 0: return showTranslationLocalX;
                            case 1: return showTranslationLocalY;
                            case 2: return showTranslationLocalZ;
                        }
                    }
                    else // Global
                    {
                        switch (axis)
                        {
                            case 0: return showTranslationGlobalX;
                            case 1: return showTranslationGlobalY;
                            case 2: return showTranslationGlobalZ;
                        }
                    }
                    break;

                case HandleType.Rotation:
                    if (space == HandleSpace.Local)
                    {
                        switch (axis)
                        {
                            case 0: return showRotationLocalX;
                            case 1: return showRotationLocalY;
                            case 2: return showRotationLocalZ;
                        }
                    }
                    else // Global
                    {
                        switch (axis)
                        {
                            case 0: return showRotationGlobalX;
                            case 1: return showRotationGlobalY;
                            case 2: return showRotationGlobalZ;
                        }
                    }
                    break;

                case HandleType.Scale:
                    if (space == HandleSpace.Local)
                    {
                        switch (axis)
                        {
                            case 0: return showScaleLocalX;
                            case 1: return showScaleLocalY;
                            case 2: return showScaleLocalZ;
                            case 3: return showUniformScale;
                        }
                    }
                    else // Global
                    {
                        switch (axis)
                        {
                            case 0: return showScaleGlobalX;
                            case 1: return showScaleGlobalY;
                            case 2: return showScaleGlobalZ;
                            case 3: return showUniformScale;
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Gets if any axis is enabled for a given handle type in any space
        /// </summary>
        public bool HasAnyAxisEnabled(HandleType handleType)
        {
            switch (handleType)
            {
                case HandleType.Translation:
                    return showTranslationLocalX || showTranslationLocalY || showTranslationLocalZ ||
                           showTranslationGlobalX || showTranslationGlobalY || showTranslationGlobalZ;

                case HandleType.Rotation:
                    return showRotationLocalX || showRotationLocalY || showRotationLocalZ ||
                           showRotationGlobalX || showRotationGlobalY || showRotationGlobalZ;

                case HandleType.Scale:
                    return showScaleLocalX || showScaleLocalY || showScaleLocalZ ||
                           showScaleGlobalX || showScaleGlobalY || showScaleGlobalZ || showUniformScale;
            }
            return false;
        }

        /// <summary>
        /// Creates a default profile with standard local handles enabled
        /// </summary>
        public static HandleProfile CreateDefault()
        {
            var profile = CreateInstance<HandleProfile>();
            profile.name = "Default Handle Profile";
            return profile;
        }
    }
}