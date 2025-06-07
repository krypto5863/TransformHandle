using UnityEngine;
using TransformHandle;

/// <summary>
/// Main component that manages transform handles in runtime
/// </summary>
public class TransformHandleManager : MonoBehaviour
{
    [Header("Target")]
    public Transform targetTransform;
    
    [Header("Handle Settings")]
    public float handleSize = 1f;
    public float screenSizeMultiplier = 0.1f;
    public bool maintainConstantScreenSize = true;
    
    [Header("Render Settings")]
    public bool alwaysOnTop = true;
    
    // Core components
    private HandleRenderer renderer;
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
        renderer = new HandleRenderer();
        interaction = new HandleInteraction(mainCamera);
    }
    
    void Update()
    {
        if (targetTransform == null || mainCamera == null) return;
        
        // Update interaction target
        interaction.UpdateTarget(targetTransform);
        
        // Update interaction with current handle scale
        float scale = GetHandleScale();
        interaction.Update(scale);
    }
    
    void OnRenderObject()
    {
        if (targetTransform == null || mainCamera == null || renderer == null) return;
        
        // Only render in Game View (not in Scene View)
        #if UNITY_EDITOR
        if (UnityEditor.SceneView.currentDrawingSceneView != null) return;
        #endif
        
        // Check if handle is in front of camera
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetTransform.position);
        if (viewportPos.z < 0) return;
        
        // Render handles
        float scale = GetHandleScale();
        renderer.Render(targetTransform, scale, interaction.HoveredAxis, alwaysOnTop);
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
        renderer?.Cleanup();
    }
}