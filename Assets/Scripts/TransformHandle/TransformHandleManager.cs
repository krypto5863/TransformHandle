using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Main component that manages transform handles in runtime,
    /// allowing setting between Translation/Rotation and toggling Local/Global modes.
    /// Methods are exposed for external key event binding.
    /// </summary>
    public class TransformHandleManager : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The transform to manipulate with the handles.")]
        public Transform targetTransform;

        [Header("Handle Settings")]
        [Tooltip("Maintain a constant on-screen size regardless of distance.")]
        public bool maintainConstantScreenSize = true;
        [Tooltip("Base size of the handles. Scales with distance if Maintain Constant Screen Size is enabled.")]
        public float handleSize = 1f;
        [Tooltip("Multiplier applied when maintaining constant screen size.")]
        public float screenSizeMultiplier = 0.1f;

        [Header("State")]
        [SerializeField]
        private HandleType handleType = HandleType.Translation;
        [SerializeField]
        private HandleSpace handleSpace = HandleSpace.Local;

        private Camera mainCamera;
        private HandleInteraction interaction;
        private HandleRenderer handleRenderer;

        void Awake()
        {
            mainCamera = Camera.main;
            interaction = new HandleInteraction(mainCamera);
            handleRenderer = new HandleRenderer();
        }

        void Update()
        {
            if (targetTransform == null || mainCamera == null) return;

            // Update interaction target every frame
            interaction.UpdateTarget(targetTransform);

            // Compute scale and update interaction
            float scale = GetHandleScale();
            interaction.Update(scale, handleType, handleSpace);
        }

        /// <summary>
        /// Sets the handle mode to Translation.
        /// </summary>
        public void SetTranslationMode()
        {
            handleType = HandleType.Translation;
        }

        /// <summary>
        /// Sets the handle mode to Rotation.
        /// </summary>
        public void SetRotationMode()
        {
            handleType = HandleType.Rotation;
        }

        /// <summary>
        /// Toggles between Local and Global axis space.
        /// </summary>
        public void ToggleHandleSpace()
        {
            handleSpace = (handleSpace == HandleSpace.Local)
                ? HandleSpace.Global
                : HandleSpace.Local;
        }

        void OnRenderObject()
        {
            if (targetTransform == null || mainCamera == null) return;

            #if UNITY_EDITOR
            if (UnityEditor.SceneView.currentDrawingSceneView != null) return;
            #endif

            Vector3 vp = mainCamera.WorldToViewportPoint(targetTransform.position);
            if (vp.z < 0f) return;

            float scale = GetHandleScale();
            handleRenderer.Render(
                targetTransform,
                scale,
                interaction.HoveredAxis,
                handleType,
                handleSpace);
        }

        private float GetHandleScale()
        {
            if (!maintainConstantScreenSize || mainCamera == null)
                return handleSize;

            float distance = Vector3.Distance(mainCamera.transform.position, targetTransform.position);
            return distance * screenSizeMultiplier * handleSize;
        }

        void OnDestroy()
        {
            handleRenderer?.Cleanup();
        }
    }
}
