namespace TransformHandle
{
    /// <summary>
    /// Legt fest, ob die Handles in World- oder Local-Space angezeigt werden.
    /// </summary>
    public enum HandleSpace
    {
        Local,   // Achsen am Transform orientiert
        Global   // Welt-Achsen (Vector3.right/up/forward)
    }
}