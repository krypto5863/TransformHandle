# Mesh-Free Transform Handles for Unity
Lightweight immediate mode rendered handles without GameObjects or Meshes

A high-performance runtime transform handle system for Unity, inspired by Unity's editor transform tools. Manipulate GameObjects at runtime with visual handles for translation, rotation, and scale - all without creating a single mesh or GameObject.

## ✨ Basic Features

- 🎯 **Translation Handles** - Move objects along X, Y, Z axes with arrow handles
- 🔄 **Rotation Handles** - Rotate objects with circular handles
- 📏 **Scale Handles** - Scale objects with box handles and uniform scaling
- 🌍 **Local/Global Space** - Toggle between local and world space (X key)
- ⌨️ **Editor-Style Controls** - W for translation, E for rotation, R for scale
- 🚀 **Performant** - GL-based rendering, no GameObjects needed

## 📦 Installation

### Option 1: Unity Package (Recommended)
1. Download the latest `MeshFreeHandles_v0.2.0-beta.unitypackage` from [Releases](https://github.com/BjoernGit/TransformHandle/releases)
2. Import into Unity: Assets → Import Package → Custom Package
3. Done! No dependencies required

### Option 2: Manual Installation
1. Clone or download this repository
2. Copy the `Assets/Scripts/MeshFreeHandles` folder into your Unity project
3. That's it - the system initializes automatically

## 🚀 Quick Start

```csharp
using MeshFreeHandles;

// Show handles on any transform
myTransform.ShowHandles();

// Or select with mouse
void Update()
{
   if (Input.GetMouseButtonDown(0))
   {
       if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
       {
           hit.transform.ShowHandles();
       }
   }
   
   // Controls
   if (Input.GetKeyDown(KeyCode.W)) TransformHandleManager.Instance.SetTranslationMode();
   if (Input.GetKeyDown(KeyCode.E)) TransformHandleManager.Instance.SetRotationMode();
   if (Input.GetKeyDown(KeyCode.R)) TransformHandleManager.Instance.SetScaleMode();
   if (Input.GetKeyDown(KeyCode.X)) TransformHandleManager.Instance.ToggleHandleSpace();
}

```

Or use the already implemented "Transform Handle Key Manager.
For flexibility this is optional.

![grafik](https://github.com/user-attachments/assets/61fd9cbf-f47b-4331-85fa-3034165a1140)

## 🗺️ Roadmap
- [ ] Per-axis space configuration (mix local/global per axis)
- [ ] Axis constraints and locking
- [ ] Multiple simultanious handles

---
⭐ If you find this useful, please consider giving it a star!
