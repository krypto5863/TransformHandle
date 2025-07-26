using MeshFreeHandles;
using UnityEngine;

namespace TransformHandle.Samples
{
   public class SwitchCamera : MonoBehaviour
   {
       public Camera camera1;
       public Camera camera2;
       
       [ContextMenu("Execute SetCamera1")]
       public void SwitchToCamera1()
       {
           camera1.enabled = true;
           camera2.enabled = false;
           TransformHandleManager.Instance.HandleCamera = camera1;
       }
       
       [ContextMenu("Execute SetCamera2")]
       public void SwitchToCamera2()
       {
           camera1.enabled = false;
           camera2.enabled = true;
           TransformHandleManager.Instance.HandleCamera = camera2;
       }
       
       void OnGUI()
       {
           // Position: Bottom right
           float width = 120f;
           float height = 70f;
           float x = Screen.width - width - 10f;
           float y = Screen.height - height - 10f;
           
           GUILayout.BeginArea(new Rect(x, y, width, height));
           
           if (GUILayout.Button("Camera 1", GUILayout.Height(30)))
           {
               SwitchToCamera1();
           }
           
           if (GUILayout.Button("Camera 2", GUILayout.Height(30)))
           {
               SwitchToCamera2();
           }
           
           GUILayout.EndArea();
       }
   }
}