using UnityEngine;
using UnityEngine.InputSystem;

namespace MeshFreeHandles
{
    /// <summary>
    /// Unity Editor-style camera navigation with WASD+QE movement and right mouse button rotation
    /// </summary>
    public class SimpleCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float shiftSpeedMultiplier = 3f;
        [SerializeField] private float smoothTime = 0.1f;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private bool invertY = false;

        [Header("Zoom Settings")]
        [SerializeField] private float scrollSpeed = 10f;

        private Camera cam;
        private Vector3 velocity;
        private Vector3 targetPosition;
        private float currentSpeed;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
                cam = Camera.main;

            targetPosition = transform.position;
        }

        void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleScroll();
        }

        private void HandleMovement()
        {
            if (Mouse.current == null || Keyboard.current == null) return;
            
            // Only move when right mouse button is held
            if (!Mouse.current.rightButton.isPressed) return;

            Vector3 moveDirection = Vector3.zero;

            // Get input
            if (Keyboard.current.wKey.isPressed) moveDirection += transform.forward;
            if (Keyboard.current.sKey.isPressed) moveDirection -= transform.forward;
            if (Keyboard.current.aKey.isPressed) moveDirection -= transform.right;
            if (Keyboard.current.dKey.isPressed) moveDirection += transform.right;
            if (Keyboard.current.qKey.isPressed) moveDirection -= transform.up;
            if (Keyboard.current.eKey.isPressed) moveDirection += transform.up;

            // Speed modifier
            currentSpeed = moveSpeed;
            if (Keyboard.current.shiftKey.isPressed)
                currentSpeed *= shiftSpeedMultiplier;

            // Apply movement
            if (moveDirection.magnitude > 0)
            {
                targetPosition += moveDirection.normalized * currentSpeed * Time.deltaTime;
            }

            // Smooth movement
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }

        private void HandleRotation()
        {
            if (Mouse.current == null || !Mouse.current.rightButton.isPressed) return;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            if (mouseDelta.magnitude > 0)
            {
                // Horizontal rotation (Y axis)
                transform.Rotate(Vector3.up, mouseDelta.x * rotationSpeed, Space.World);

                // Vertical rotation (X axis)
                float verticalRotation = invertY ? mouseDelta.y : -mouseDelta.y;
                transform.Rotate(transform.right, verticalRotation * rotationSpeed, Space.World);

                // Update target position to current position when rotating
                targetPosition = transform.position;
            }
        }

        private void HandleScroll()
        {
            if (Mouse.current == null) return;

            float scrollDelta = Mouse.current.scroll.y.ReadValue();
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                // Move forward/backward based on scroll
                Vector3 scrollMovement = transform.forward * (scrollDelta * scrollSpeed * 0.01f);
                targetPosition += scrollMovement;
            }
        }
    }
}