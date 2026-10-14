using UnityEngine;
namespace TimeCourier
{
    [RequireComponent(typeof(Camera))]
    public class PortraitCamera : MonoBehaviour
    {
        private Camera targetCamera;
        private void Awake() { targetCamera = GetComponent<Camera>(); }
        private void LateUpdate()
        {
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            targetCamera.orthographicSize = Mathf.Max(5.8f, 3.3f / Mathf.Max(0.1f, aspect));
        }
    }
}
