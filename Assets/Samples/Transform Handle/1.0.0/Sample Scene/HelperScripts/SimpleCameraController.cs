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
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 30f;
        [SerializeField] private float maxSpeed = 50f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private float rotationSmoothTime = 0.05f;
        [SerializeField] private bool invertY = false;
        
        [Header("Zoom Settings")]
        [SerializeField] private float scrollSpeed = 10f;
        [SerializeField] private float scrollAcceleration = 50f;
        
        private Camera cam;
        private Vector3 currentVelocity;
        private Vector3 targetVelocity;
        private Vector2 rotationVelocity;
        private Vector2 currentRotation;
        private float scrollVelocity;
        
        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
                cam = Camera.main;
                
            // Initialize rotation from current transform
            currentRotation.x = transform.eulerAngles.x;
            currentRotation.y = transform.eulerAngles.y;
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
            bool isRightMousePressed = Mouse.current.rightButton.isPressed;
            
            Vector3 inputDirection = Vector3.zero;
            
            if (isRightMousePressed)
            {
                // Get input
                if (Keyboard.current.wKey.isPressed) inputDirection += Vector3.forward;
                if (Keyboard.current.sKey.isPressed) inputDirection -= Vector3.forward;
                if (Keyboard.current.aKey.isPressed) inputDirection -= Vector3.right;
                if (Keyboard.current.dKey.isPressed) inputDirection += Vector3.right;
                if (Keyboard.current.qKey.isPressed) inputDirection -= Vector3.up;
                if (Keyboard.current.eKey.isPressed) inputDirection += Vector3.up;
                
                // Normalize input
                if (inputDirection.magnitude > 1f)
                    inputDirection.Normalize();
            }
            
            // Speed modifier
            float targetSpeed = moveSpeed;
            if (Keyboard.current.shiftKey.isPressed)
                targetSpeed *= shiftSpeedMultiplier;
            
            // Calculate target velocity in world space
            if (inputDirection.magnitude > 0.01f)
            {
                targetVelocity = transform.TransformDirection(inputDirection) * targetSpeed;
                targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxSpeed);
            }
            else
            {
                targetVelocity = Vector3.zero;
            }
            
            // Smooth acceleration/deceleration
            float smoothingFactor = inputDirection.magnitude > 0.01f ? acceleration : deceleration;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, smoothingFactor * Time.deltaTime);
            
            // Apply movement
            if (currentVelocity.magnitude > 0.01f)
            {
                transform.position += currentVelocity * Time.deltaTime;
            }
        }
        
        private void HandleRotation()
        {
            if (Mouse.current == null || !Mouse.current.rightButton.isPressed) return;
            
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            if (mouseDelta.magnitude > 0)
            {
                // Calculate target rotation
                float yaw = mouseDelta.x * rotationSpeed;
                float pitch = (invertY ? mouseDelta.y : -mouseDelta.y) * rotationSpeed;
                
                // Smooth rotation
                rotationVelocity = Vector2.Lerp(rotationVelocity, new Vector2(pitch, yaw), 
                    Time.deltaTime / rotationSmoothTime);
                
                currentRotation.x += rotationVelocity.x;
                currentRotation.y += rotationVelocity.y;
                
                // Clamp pitch to prevent flipping
                currentRotation.x = Mathf.Clamp(currentRotation.x, -89f, 89f);
                
                // Apply rotation
                transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
            }
            else
            {
                // Dampen rotation velocity when not moving mouse
                rotationVelocity = Vector2.Lerp(rotationVelocity, Vector2.zero, 
                    Time.deltaTime / rotationSmoothTime);
            }
        }
        
        private void HandleScroll()
        {
            if (Mouse.current == null) return;
            
            float scrollInput = Mouse.current.scroll.y.ReadValue();
            
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                // Accelerate scroll velocity
                scrollVelocity += scrollInput * scrollSpeed * Time.deltaTime;
                scrollVelocity = Mathf.Clamp(scrollVelocity, -maxSpeed, maxSpeed);
            }
            else
            {
                // Decelerate when not scrolling
                scrollVelocity = Mathf.Lerp(scrollVelocity, 0f, scrollAcceleration * Time.deltaTime);
            }
            
            // Apply scroll movement
            if (Mathf.Abs(scrollVelocity) > 0.01f)
            {
                transform.position += transform.forward * scrollVelocity * Time.deltaTime;
            }
        }
        
        // Public method to reset velocity (useful when teleporting)
        public void ResetVelocity()
        {
            currentVelocity = Vector3.zero;
            targetVelocity = Vector3.zero;
            scrollVelocity = 0f;
            rotationVelocity = Vector2.zero;
        }
        
        // Public method to set position without interpolation
        public void SetPosition(Vector3 newPosition)
        {
            transform.position = newPosition;
            ResetVelocity();
        }
    }
}