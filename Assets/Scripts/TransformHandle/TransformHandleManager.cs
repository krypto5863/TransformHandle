using UnityEngine;
using UnityEngine.InputSystem;

public class TransformHandleManager : MonoBehaviour
{
    [Header("Target")]
    public Transform targetTransform;
    
    [Header("Handle Settings")]
    public float handleSize = 1f;
    public float screenSizeMultiplier = 0.1f; // Handle size relative to screen size
    public bool maintainConstantScreenSize = true;
    
    [Header("Colors")]
    public Color xAxisColor = Color.red;
    public Color yAxisColor = Color.green;
    public Color zAxisColor = Color.blue;
    public float axisAlpha = 0.8f;
    public float selectedAlpha = 1f;
    
    [Header("Render Settings")]
    public bool alwaysOnTop = true;
    public float occludedAlpha = 0.3f; // Alpha when occluded
    
    // Material for GL rendering
    private Material lineMaterial;
    private Material lineMaterialOccluded; // For occluded parts
    private Camera mainCamera;
    
    // Interaction
    private int hoveredAxis = -1; // -1 = none, 0 = X, 1 = Y, 2 = Z
    private bool isDragging = false;
    private int draggedAxis = -1;
    
    void Start()
    {
        CreateLineMaterial();
        CreateOccludedMaterial();
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
        }
    }
    
    void CreateLineMaterial()
    {
        // Use a shader that always renders on top
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterial.SetInt("_ZWrite", 0);
        
        if (alwaysOnTop)
        {
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }
    }
    
    void CreateOccludedMaterial()
    {
        // Material for occluded parts
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterialOccluded = new Material(shader);
        lineMaterialOccluded.hideFlags = HideFlags.HideAndDontSave;
        
        lineMaterialOccluded.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterialOccluded.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterialOccluded.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterialOccluded.SetInt("_ZWrite", 0);
        lineMaterialOccluded.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Greater); // Only when occluded
    }
    
    void Update()
    {
        if (targetTransform == null || mainCamera == null) return;
        
        // Handle interaction (simplified for this example)
        UpdateHoverState();
    }
    
    void UpdateHoverState()
    {
        if (isDragging) return;
        
        // Use the new Input System
        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Vector3 handleScreenPos = mainCamera.WorldToScreenPoint(targetTransform.position);
        
        if (handleScreenPos.z < 0) // Handle is behind the camera
        {
            hoveredAxis = -1;
            return;
        }
        
        // Very simplified proximity detection
        float distance = Vector2.Distance(mousePos, handleScreenPos);
        if (distance < 100f) // 100 pixel radius
        {
            // Here you could implement more accurate axis detection
            hoveredAxis = 0; // For demo: always X-axis
        }
        else
        {
            hoveredAxis = -1;
        }
    }
    
    float GetHandleScale()
    {
        if (!maintainConstantScreenSize) return handleSize;
        
        // Calculate scale based on camera distance
        float distance = Vector3.Distance(mainCamera.transform.position, targetTransform.position);
        return distance * screenSizeMultiplier * handleSize;
    }
    
    void OnRenderObject()
    {
        if (targetTransform == null || mainCamera == null || lineMaterial == null) return;
        
        // Check if handle is in front of camera
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetTransform.position);
        if (viewportPos.z < 0) return;
        
        float scale = GetHandleScale();
        Vector3 position = targetTransform.position;
        
        if (!alwaysOnTop)
        {
            // Draw occluded parts first with reduced transparency
            lineMaterialOccluded.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            
            DrawAxis(position, targetTransform.right, xAxisColor, scale, 0, occludedAlpha);
            DrawAxis(position, targetTransform.up, yAxisColor, scale, 1, occludedAlpha);
            DrawAxis(position, targetTransform.forward, zAxisColor, scale, 2, occludedAlpha);
            
            GL.PopMatrix();
        }
        
        // Draw visible parts
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);
        
        // Draw the three axes
        DrawAxis(position, targetTransform.right, xAxisColor, scale, 0);
        DrawAxis(position, targetTransform.up, yAxisColor, scale, 1);
        DrawAxis(position, targetTransform.forward, zAxisColor, scale, 2);
        
        // Optional: Draw center point
        DrawCenterPoint(position, scale * 0.1f);
        
        GL.PopMatrix();
    }
    
    void DrawAxis(Vector3 origin, Vector3 direction, Color color, float length, int axisIndex, float alphaOverride = -1f)
    {
        // Set alpha based on hover state or override
        float alpha = alphaOverride >= 0 ? alphaOverride : 
                     (hoveredAxis == axisIndex) ? selectedAlpha : axisAlpha;
        Color finalColor = new Color(color.r, color.g, color.b, alpha);
        
        Vector3 endPoint = origin + direction * length;
        
        // Draw main line
        GL.Begin(GL.LINES);
        GL.Color(finalColor);
        GL.Vertex(origin);
        GL.Vertex(endPoint);
        GL.End();
        
        // Draw arrow head
        DrawArrowHead(endPoint, direction, finalColor, length * 0.2f);
    }
    
    void DrawArrowHead(Vector3 tip, Vector3 direction, Color color, float size)
    {
        // Calculate two orthogonal vectors to the direction
        Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up).normalized;
        if (perpendicular1.magnitude < 0.1f) // If direction is parallel to up
        {
            perpendicular1 = Vector3.Cross(direction, Vector3.right).normalized;
        }
        Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;
        
        // Base of the arrow head
        Vector3 arrowBase = tip - direction * size;
        
        GL.Begin(GL.LINES);
        GL.Color(color);
        
        // 4 lines from tip to base (pyramid)
        float baseSize = size * 0.5f;
        Vector3[] basePoints = new Vector3[4];
        basePoints[0] = arrowBase + perpendicular1 * baseSize;
        basePoints[1] = arrowBase - perpendicular1 * baseSize;
        basePoints[2] = arrowBase + perpendicular2 * baseSize;
        basePoints[3] = arrowBase - perpendicular2 * baseSize;
        
        // Lines from tip to base
        foreach (Vector3 basePoint in basePoints)
        {
            GL.Vertex(tip);
            GL.Vertex(basePoint);
        }
        
        // Base square
        GL.Vertex(basePoints[0]);
        GL.Vertex(basePoints[2]);
        
        GL.Vertex(basePoints[2]);
        GL.Vertex(basePoints[1]);
        
        GL.Vertex(basePoints[1]);
        GL.Vertex(basePoints[3]);
        
        GL.Vertex(basePoints[3]);
        GL.Vertex(basePoints[0]);
        
        GL.End();
    }
    
    void DrawCenterPoint(Vector3 center, float size)
    {
        // Small cube at center
        GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 1f, 1f, 0.5f));
        
        // Simplified point as cross
        GL.Vertex(center + Vector3.right * size);
        GL.Vertex(center - Vector3.right * size);
        
        GL.Vertex(center + Vector3.up * size);
        GL.Vertex(center - Vector3.up * size);
        
        GL.Vertex(center + Vector3.forward * size);
        GL.Vertex(center - Vector3.forward * size);
        
        GL.End();
    }
    
    void OnDestroy()
    {
        if (lineMaterial != null)
        {
            DestroyImmediate(lineMaterial);
        }
        if (lineMaterialOccluded != null)
        {
            DestroyImmediate(lineMaterialOccluded);
        }
    }
}