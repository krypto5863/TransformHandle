using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MeshFreeHandles
{
    /// <summary>
    /// Dispatches key events for setting translation/rotation mode and toggling Local/Global space,
    /// using Editor-style shortcuts (W, E, X).
    /// </summary>
    public class TransformHandleKeyManager : MonoBehaviour
    {
        [Header("Mode Keys")]
        [Tooltip("Key to set Translation mode (move tool).")]
        public Key translationKey = Key.W;
        [Tooltip("Invoked when translationKey is pressed.")]
        public UnityEvent onSetTranslation;

        [Tooltip("Key to set Rotation mode (rotate tool).")]
        public Key rotationKey = Key.E;
        [Tooltip("Invoked when rotationKey is pressed.")]
        public UnityEvent onSetRotation;

        [Tooltip("Key to set Scale mode (scale tool).")]
        public Key scaleKey = Key.R;
        [Tooltip("Invoked when scaleKey is pressed.")]
        public UnityEvent onSetScale;

        [Header("Space Key")]
        [Tooltip("Key to toggle between Local and Global axes.")]
        public Key handleSpaceToggleKey = Key.X;
        [Tooltip("Invoked when handleSpaceToggleKey is pressed.")]
        public UnityEvent onToggleHandleSpace;

        void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current[translationKey].wasPressedThisFrame)
                onSetTranslation?.Invoke();

            if (Keyboard.current[rotationKey].wasPressedThisFrame)
                onSetRotation?.Invoke();

            if (Keyboard.current[scaleKey].wasPressedThisFrame)
                onSetScale?.Invoke();

            if (Keyboard.current[handleSpaceToggleKey].wasPressedThisFrame)
                onToggleHandleSpace?.Invoke();
        }
    }
}