using UnityEngine;

public class HandleType
{
    
}
namespace TransformHandle
{
    /// <summary>
    /// Defines the type of transform handle to display
    /// </summary>
    public enum HandleType
    {
        Translation,  // Move/Position handles (arrows)
        Rotation,     // Rotate handles (circles)
        Scale,        // Scale handles (boxes) - for future implementation
        None
    }
}