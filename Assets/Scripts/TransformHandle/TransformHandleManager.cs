using UnityEngine;

namespace TransformHandle
{
    /// <summary>
    /// Main component that manages transform handles in runtime
    /// </summary>
    public class TransformHandleManager : MonoBehaviour
    {
        [Header("Target")]
        public Transform targetTransform;
        
        [Header("Handle Type")]
        public HandleType handleType = HandleType.Translation;
        
        [Header("Handle Settings")]
        public float handleSize = 1f;
        public float screenSizeMultiplier = 0.1f;
        public bool maintainConstantScreenSize = true;
        
        [Header("Render Settings")]
        public bool alwaysOnTop = true;
        
        // Core components
        private HandleRenderer handleRenderer;
        private HandleInteraction interaction;
        private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }
        
        // Initialize components
        handleRenderer = new HandleRenderer();
        interaction = new HandleInteraction(mainCamera);
    }
    
    void Update()
    {
        if (targetTransform == null || mainCamera == null) return;
        
        // Update interaction target
        interaction.UpdateTarget(targetTransform);
        
        // Update interaction with current handle scale and type
        float scale = GetHandleScale();
        interaction.Update(scale, handleType);
        
        // Handle input for switching handle types (optional)
        HandleTypeSwithching();
    }
    
    void HandleTypeSwithching()
    {
        // Example: Use W for Translation, E for Rotation (like Unity Editor)
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.wKey.wasPressedThisFrame)
            {
                handleType = TransformHandle.HandleType.Translation;
            }
            else if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                handleType = TransformHandle.HandleType.Rotation;
            }
        }
    }
    
    void OnRenderObject()
    {
        if (targetTransform == null || mainCamera == null || handleRenderer == null) return;
        
        // Only render in Game View (not in Scene View)
        #if UNITY_EDITOR
        if (UnityEditor.SceneView.currentDrawingSceneView != null) return;
        #endif
        
        // Check if handle is in front of camera
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetTransform.position);
        if (viewportPos.z < 0) return;
        
        // Render handles
        float scale = GetHandleScale();
        handleRenderer.Render(targetTransform, scale, interaction.HoveredAxis, alwaysOnTop, handleType);
    }
    
    float GetHandleScale()
    {
        if (!maintainConstantScreenSize) return handleSize;
        
        // Calculate scale based on camera distance
        float distance = Vector3.Distance(mainCamera.transform.position, targetTransform.position);
        return distance * screenSizeMultiplier * handleSize;
    }
    
    void OnDestroy()
    {
        handleRenderer?.Cleanup();
    }
}
}