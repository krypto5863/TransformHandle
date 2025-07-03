using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace MeshFreeHandles
{
    /// <summary>
    /// Manages object selection through mouse clicks and communicates with TransformHandleManager
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        private static SelectionManager instance;
       
        public static SelectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<SelectionManager>();
                   
                    if (instance == null)
                    {
                        GameObject go = new GameObject("Selection Manager");
                        instance = go.AddComponent<SelectionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        [Header("Selection Settings")]
        [SerializeField] private LayerMask selectableLayerMask = -1;
        [SerializeField] private float maxSelectionDistance = 1000f;
        private Camera mainCamera;
        private Transform currentSelection;
        public event Action<Transform> OnSelectionChanged;
        
        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
           
            instance = this;
            DontDestroyOnLoad(gameObject);
            mainCamera = Camera.main;
        }
        
        void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                // Skip if clicking on UI
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                
                // Skip if hovering over a handle
                if (TransformHandleManager.Instance.IsHovering)
                    return;
                
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, maxSelectionDistance, selectableLayerMask))
                {
                    // Hit an object - select it
                    currentSelection = hit.transform;
                    TransformHandleManager.Instance.SetTarget(currentSelection);
                    OnSelectionChanged?.Invoke(currentSelection);
                }
                else
                {
                    // Hit nothing - clear selection
                    currentSelection = null;
                    TransformHandleManager.Instance.ClearTarget();
                    OnSelectionChanged?.Invoke(null);
                }
            }
        }
        
        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}