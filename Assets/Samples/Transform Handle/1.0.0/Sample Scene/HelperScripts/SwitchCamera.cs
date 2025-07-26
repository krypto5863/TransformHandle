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

            // Explizit setzen!
            TransformHandleManager.Instance.HandleCamera = camera1;
        }
        [ContextMenu("Execute SetCamera2")]

        public void SwitchToCamera2()
        {
            camera1.enabled = false;
            camera2.enabled = true;

            TransformHandleManager.Instance.HandleCamera = camera2;
        }
    }
}
