using UnityEngine;

public class GLDemo : MonoBehaviour
{
    // Material wird benötigt für GL rendering
    private Material lineMaterial;
    
    // Demo-Variablen
    private float rotationAngle = 0f;
    private Vector2 mousePosition;
    
    void Start()
    {
        // Erstelle ein einfaches unlit Material
        CreateLineMaterial();
    }
    
    void CreateLineMaterial()
    {
        // Shader für Linien ohne Beleuchtung
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        
        // Setze Render-Einstellungen
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterial.SetInt("_ZWrite", 0);
    }
    
    void Update()
    {
        // Rotation für Animation
        rotationAngle += Time.deltaTime * 50f;
        
        // Mausposition für Interaktivität
        mousePosition = Input.mousePosition;
    }
    
    // Wird nach allen Kameras gerendert
    void OnRenderObject()
    {
        // Aktiviere unser Material
        lineMaterial.SetPass(0);
        
        // Speichere aktuelle Matrix
        GL.PushMatrix();
        
        // Wechsle zu Pixel-Koordinaten (0,0 = links unten)
        GL.LoadPixelMatrix();
        
        // Zeichne verschiedene Beispiele
        DrawGrid();
        DrawColorfulLines();
        DrawRotatingTriangle();
        DrawMouseFollower();
        DrawCoordinateSystem();
        
        // Stelle ursprüngliche Matrix wieder her
        GL.PopMatrix();
    }
    
    void DrawGrid()
    {
        GL.Begin(GL.LINES);
        GL.Color(new Color(0.5f, 0.5f, 0.5f, 0.3f));
        
        // Vertikale Linien
        for (int x = 0; x < Screen.width; x += 50)
        {
            GL.Vertex3(x, 0, 0);
            GL.Vertex3(x, Screen.height, 0);
        }
        
        // Horizontale Linien
        for (int y = 0; y < Screen.height; y += 50)
        {
            GL.Vertex3(0, y, 0);
            GL.Vertex3(Screen.width, y, 0);
        }
        
        GL.End();
    }
    
    void DrawColorfulLines()
    {
        GL.Begin(GL.LINES);
        
        // Regenbogen-Linien
        for (int i = 0; i < 7; i++)
        {
            float hue = i / 7f;
            GL.Color(Color.HSVToRGB(hue, 1f, 1f));
            
            float y = 100 + i * 20;
            GL.Vertex3(50, y, 0);
            GL.Vertex3(250, y, 0);
        }
        
        GL.End();
    }
    
    void DrawRotatingTriangle()
    {
        Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
        float radius = 100f;
        
        GL.Begin(GL.TRIANGLES);
        GL.Color(new Color(1f, 0.5f, 0f, 0.7f));
        
        // Drei Punkte des Dreiecks
        for (int i = 0; i < 3; i++)
        {
            float angle = rotationAngle + i * 120f;
            float rad = angle * Mathf.Deg2Rad;
            
            float x = center.x + Mathf.Cos(rad) * radius;
            float y = center.y + Mathf.Sin(rad) * radius;
            
            GL.Vertex3(x, y, 0);
        }
        
        GL.End();
        
        // Umriss des Dreiecks
        GL.Begin(GL.LINES);
        GL.Color(Color.white);
        
        for (int i = 0; i < 3; i++)
        {
            float angle1 = rotationAngle + i * 120f;
            float angle2 = rotationAngle + ((i + 1) % 3) * 120f;
            float rad1 = angle1 * Mathf.Deg2Rad;
            float rad2 = angle2 * Mathf.Deg2Rad;
            
            float x1 = center.x + Mathf.Cos(rad1) * radius;
            float y1 = center.y + Mathf.Sin(rad1) * radius;
            float x2 = center.x + Mathf.Cos(rad2) * radius;
            float y2 = center.y + Mathf.Sin(rad2) * radius;
            
            GL.Vertex3(x1, y1, 0);
            GL.Vertex3(x2, y2, 0);
        }
        
        GL.End();
    }
    
    void DrawMouseFollower()
    {
        // Kreis der der Maus folgt
        GL.Begin(GL.LINES);
        GL.Color(Color.cyan);
        
        int segments = 32;
        float radius = 30f;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * 2 * Mathf.PI;
            float angle2 = ((i + 1) / (float)segments) * 2 * Mathf.PI;
            
            float x1 = mousePosition.x + Mathf.Cos(angle1) * radius;
            float y1 = mousePosition.y + Mathf.Sin(angle1) * radius;
            float x2 = mousePosition.x + Mathf.Cos(angle2) * radius;
            float y2 = mousePosition.y + Mathf.Sin(angle2) * radius;
            
            GL.Vertex3(x1, y1, 0);
            GL.Vertex3(x2, y2, 0);
        }
        
        GL.End();
        
        // Fadenkreuz
        GL.Begin(GL.LINES);
        GL.Color(Color.yellow);
        
        GL.Vertex3(mousePosition.x - 20, mousePosition.y, 0);
        GL.Vertex3(mousePosition.x + 20, mousePosition.y, 0);
        GL.Vertex3(mousePosition.x, mousePosition.y - 20, 0);
        GL.Vertex3(mousePosition.x, mousePosition.y + 20, 0);
        
        GL.End();
    }
    
    void DrawCoordinateSystem()
    {
        Vector2 origin = new Vector2(100, Screen.height - 100);
        float axisLength = 80f;
        
        GL.Begin(GL.LINES);
        
        // X-Achse (Rot)
        GL.Color(Color.red);
        GL.Vertex3(origin.x, origin.y, 0);
        GL.Vertex3(origin.x + axisLength, origin.y, 0);
        
        // X-Pfeilspitze
        GL.Vertex3(origin.x + axisLength, origin.y, 0);
        GL.Vertex3(origin.x + axisLength - 10, origin.y + 5, 0);
        GL.Vertex3(origin.x + axisLength, origin.y, 0);
        GL.Vertex3(origin.x + axisLength - 10, origin.y - 5, 0);
        
        // Y-Achse (Grün)
        GL.Color(Color.green);
        GL.Vertex3(origin.x, origin.y, 0);
        GL.Vertex3(origin.x, origin.y - axisLength, 0);
        
        // Y-Pfeilspitze
        GL.Vertex3(origin.x, origin.y - axisLength, 0);
        GL.Vertex3(origin.x - 5, origin.y - axisLength + 10, 0);
        GL.Vertex3(origin.x, origin.y - axisLength, 0);
        GL.Vertex3(origin.x + 5, origin.y - axisLength + 10, 0);
        
        GL.End();
    }
    
    void OnDestroy()
    {
        // Aufräumen
        if (lineMaterial != null)
        {
            DestroyImmediate(lineMaterial);
        }
    }
}