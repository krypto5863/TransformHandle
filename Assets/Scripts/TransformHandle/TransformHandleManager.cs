using UnityEngine;
using System;

namespace MeshFreeHandles
{
    /// <summary>
    /// Singleton manager that controls transform handles for any selected object.
    /// </summary>
    public class TransformHandleManager : MonoBehaviour
    {
        private static TransformHandleManager instance;
        
        /// <summary>
        /// Global instance of the Transform Handle Manager
        /// </summary>
        public static TransformHandleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<TransformHandleManager>();
                    
                    if (instance == null)
                    {
                        GameObject go = new GameObject("Transform Handle Manager");
                        instance = go.AddComponent<TransformHandleManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Target")]
        [Tooltip("The transform to manipulate with the handles.")]
        [SerializeField] private Transform targetTransform;

        [Header("Handle Settings")]
        [Tooltip("Maintain a constant on-screen size regardless of distance.")]
        public bool maintainConstantScreenSize = true;
        [Tooltip("Base size of the handles. Scales with distance if Maintain Constant Screen Size is enabled.")]
        public float handleSize = 1f;
        [Tooltip("Multiplier applied when maintaining constant screen size.")]
        public float screenSizeMultiplier = 0.1f;

        [Header("State")]
        [SerializeField] private HandleType handleType = HandleType.Translation;
        [SerializeField] private HandleSpace handleSpace = HandleSpace.Local;

        // Components
        private Camera mainCamera;
        private HandleInteraction interaction;
        private HandleRenderer handleRenderer;

        // Events
        public event Action<Transform> OnTargetChanged;
        public event Action<HandleType> OnHandleTypeChanged;
        public event Action<HandleSpace> OnHandleSpaceChanged;
        public event Action<Transform> OnTransformModified;

        // Properties
        public Transform CurrentTarget => targetTransform;
        public HandleType CurrentHandleType => handleType;
        public HandleSpace CurrentHandleSpace => handleSpace;
        public bool IsActive => targetTransform != null;
        public bool IsDragging => interaction?.IsDragging ?? false;

        public bool IsHovering => interaction?.HoveredAxis >= 0f;

        void Awake()
        {
            // Singleton enforcement
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize components
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

            // Check if transform was modified (for event firing)
            if (interaction.IsDragging)
            {
                OnTransformModified?.Invoke(targetTransform);
            }
        }

        /// <summary>
        /// Sets the target transform to manipulate
        /// </summary>
        public void SetTarget(Transform target)
        {
            if (targetTransform == target) return;

            targetTransform = target;
            OnTargetChanged?.Invoke(target);
        }

        /// <summary>
        /// Clears the current target
        /// </summary>
        public void ClearTarget()
        {
            SetTarget(null);
        }

        /// <summary>
        /// Sets the handle mode to Translation.
        /// </summary>
        public void SetTranslationMode()
        {
            if (handleType != HandleType.Translation)
            {
                handleType = HandleType.Translation;
                OnHandleTypeChanged?.Invoke(handleType);
            }
        }

        /// <summary>
        /// Sets the handle mode to Rotation.
        /// </summary>
        public void SetRotationMode()
        {
            if (handleType != HandleType.Rotation)
            {
                handleType = HandleType.Rotation;
                OnHandleTypeChanged?.Invoke(handleType);
            }
        }

        /// <summary>
        /// Sets the handle mode to Scale.
        /// </summary>
        public void SetScaleMode()
        {
            if (handleType != HandleType.Scale)
            {
                handleType = HandleType.Scale;
                OnHandleTypeChanged?.Invoke(handleType);
            }
        }

        /// <summary>
        /// Toggles between Local and Global axis space.
        /// </summary>
        public void ToggleHandleSpace()
        {
            handleSpace = (handleSpace == HandleSpace.Local)
                ? HandleSpace.Global
                : HandleSpace.Local;
            
            OnHandleSpaceChanged?.Invoke(handleSpace);
        }

        /// <summary>
        /// Sets the handle space directly
        /// </summary>
        public void SetHandleSpace(HandleSpace space)
        {
            if (handleSpace != space)
            {
                handleSpace = space;
                OnHandleSpaceChanged?.Invoke(handleSpace);
            }
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
            if (!maintainConstantScreenSize || mainCamera == null || targetTransform == null)
                return handleSize;

            float distance = Vector3.Distance(mainCamera.transform.position, targetTransform.position);
            return distance * screenSizeMultiplier * handleSize;
        }

        void OnDestroy()
        {
            handleRenderer?.Cleanup();
            
            if (instance == this)
            {
                instance = null;
            }
        }

        // Static convenience methods for easy access
        public static void ShowHandles(Transform target)
        {
            Instance.SetTarget(target);
        }

        public static void HideHandles()
        {
            Instance.ClearTarget();
        }

        public static void SetSpace(HandleSpace space)
        {
            Instance.SetHandleSpace(space);
        }
    }

    // Extension methods for intuitive API
    public static class TransformHandleExtensions
    {
        /// <summary>
        /// Shows transform handles for this transform
        /// </summary>
        public static void ShowHandles(this Transform transform)
        {
            TransformHandleManager.ShowHandles(transform);
        }

        /// <summary>
        /// Hides transform handles if this transform is currently selected
        /// </summary>
        public static void HideHandles(this Transform transform)
        {
            if (TransformHandleManager.Instance.CurrentTarget == transform)
            {
                TransformHandleManager.HideHandles();
            }
        }

        /// <summary>
        /// Checks if this transform currently has handles
        /// </summary>
        public static bool HasHandles(this Transform transform)
        {
            return TransformHandleManager.Instance.CurrentTarget == transform;
        }
    }
}