using UnityEngine;

namespace MeshFreeHandles
{
    /// <summary>
    /// Component that holds a reference to a HandleProfile for customizing handle behavior per object
    /// </summary>
    public class HandleProfileHolder : MonoBehaviour
    {
        [SerializeField] private HandleProfile profile;

        /// <summary>
        /// The handle profile for this object
        /// </summary>
        public HandleProfile Profile
        {
            get => profile;
            set => profile = value;
        }

        /// <summary>
        /// Checks if this holder has a valid profile assigned
        /// </summary>
        public bool HasProfile => profile != null;

        private void OnValidate()
        {
            // Notify the handle manager if this object is currently selected
            if (Application.isPlaying && 
                TransformHandleManager.Instance != null && 
                TransformHandleManager.Instance.CurrentTarget == transform)
            {
                // Force the handle manager to refresh with the new profile
                TransformHandleManager.Instance.RefreshProfile();
            }
        }
    }
}