using UnityEngine;
using UnityEngine.InputSystem;

namespace TransformHandle
{
    /// <summary>
    /// Main component that manages transform handles in runtime,
    /// allowing toggling between Translation/Rotation and Local/Global modes.
    /// </summary>
    public class TransformHandleManager : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The transform to manipulate with the handles.")]
        public Transform targetTransform;

        [Header("Handle Type")]
        [Tooltip("Select Translation or Rotation handles.")]
        public HandleType handleType = HandleType.Translation;

        [Header("Handle Space")]
        [Tooltip("Select Local (object space) or Global (world space) axes.")]
        public HandleSpace handleSpace = HandleSpace.Local;

        [Header("Handle Settings")]
        [Tooltip("Base size of the handles. Scales with distance if Maintain Constant Screen Size is enabled.")]
        public float handleSize = 1f;
        [Tooltip("Maintain a constant on-screen size regardless of distance.")]
        public bool maintainConstantScreenSize = true;
        [Tooltip("Multiplier applied when maintaining constant screen size.")]
        public float screenSizeMultiplier = 0.1f;
        [Tooltip("Render handles on top of all geometry.")]
        public bool alwaysOnTop = true;

        [Header("Input Keys")]
        [Tooltip("Key to toggle between Translation and Rotation handles.")]
        public Key toggleHandleTypeKey = Key.H;
        [Tooltip("Key to toggle between Local and Global handle space.")]
        public Key toggleHandleSpaceKey = Key.G;

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

            // Toggle input
            HandleTypeSwitching();
            HandleSpaceSwitching();

            // Update interaction target every frame
            interaction.UpdateTarget(targetTransform);

            // Compute scale and update interaction
            float scale = GetHandleScale();
            interaction.Update(scale, handleType, handleSpace);
        }

        /// <summary>
        /// Toggles between Translation and Rotation handle modes.
        /// </summary>
        private void HandleTypeSwitching()
        {
            if (Keyboard.current?[toggleHandleTypeKey].wasPressedThisFrame ?? false)
            {
                handleType = (handleType == HandleType.Translation)
                    ? HandleType.Rotation
                    : HandleType.Translation;
            }
        }

        /// <summary>
        /// Toggles between Local and Global axis space.
        /// </summary>
        private void HandleSpaceSwitching()
        {
            if (Keyboard.current?[toggleHandleSpaceKey].wasPressedThisFrame ?? false)
            {
                handleSpace = (handleSpace == HandleSpace.Local)
                    ? HandleSpace.Global
                    : HandleSpace.Local;
            }
        }

        void OnRenderObject()
        {
            if (targetTransform == null || mainCamera == null) return;

            // Only render in Game View
            #if UNITY_EDITOR
            if (UnityEditor.SceneView.currentDrawingSceneView != null) return;
            #endif

            // Cull if behind camera
            Vector3 vp = mainCamera.WorldToViewportPoint(targetTransform.position);
            if (vp.z < 0f) return;

            float scale = GetHandleScale();
            handleRenderer.Render(
                targetTransform,
                scale,
                interaction.HoveredAxis,
                alwaysOnTop,
                handleType,
                handleSpace);
        }

        /// <summary>
        /// Computes the handle scale, optionally maintaining a constant screen size.
        /// </summary>
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
