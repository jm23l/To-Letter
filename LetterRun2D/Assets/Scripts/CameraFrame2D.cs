using UnityEngine;
namespace LetterRun2D
{
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public class CameraFrame2D : MonoBehaviour
    {
        [Min(1)] public float mapWidth = 9;
        [Min(1)] public float mapHeight = 11.6f;
        private Camera backgroundCamera;
        private void OnEnable() { Refresh(); }
        private void LateUpdate() { Refresh(); }
        private void ClearWholeScreen(Camera main)
        {
            if (!Application.isPlaying) return;
            if (backgroundCamera == null)
            {
                var obj = new GameObject("Full screen background");
                obj.transform.SetParent(transform, false);
                backgroundCamera = obj.AddComponent<Camera>();
                backgroundCamera.cullingMask = 0;
                backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
                backgroundCamera.orthographic = true;
                backgroundCamera.allowHDR = false;
                backgroundCamera.allowMSAA = false;
                backgroundCamera.useOcclusionCulling = false;
            }
            // A partial viewport cannot erase UI pixels left outside it by the previous frame.
            // Clear the full target first, then render the map and the screen-space UI normally.
            backgroundCamera.enabled = main.enabled;
            backgroundCamera.depth = main.depth - 1;
            backgroundCamera.rect = new Rect(0, 0, 1, 1);
            backgroundCamera.backgroundColor = main.backgroundColor;
            backgroundCamera.targetDisplay = main.targetDisplay;
            backgroundCamera.targetTexture = main.targetTexture;
        }
        private void OnDisable()
        { if (backgroundCamera != null) backgroundCamera.enabled = false; }
        private void OnDestroy()
        { if (backgroundCamera != null) Destroy(backgroundCamera.gameObject); }
        public void Refresh()
        {
            if (Screen.width < 1 || Screen.height < 1) return;
            var cam = GetComponent<Camera>();
            ClearWholeScreen(cam);
            var safe = Screen.safeArea;
            if (safe.width <= 0 || safe.height <= 0) safe = new Rect(0, 0, Screen.width, Screen.height);
            float scale = Mathf.Sqrt(Screen.width / 480f * Screen.height / 854f);
            float bottom = safe.yMin + 205 * scale;
            float top = safe.yMax - 112 * scale;
            if (top <= bottom + 40) { bottom = Screen.height * 0.23f; top = Screen.height * 0.85f; }
            // Match the clear world rectangle to the safe area between HUD and touch controls.
            cam.rect = new Rect(safe.xMin / Screen.width, bottom / Screen.height,
                safe.width / Screen.width, (top - bottom) / Screen.height);
            float aspect = safe.width / Mathf.Max(1, top - bottom);
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(mapHeight * 0.5f, mapWidth * 0.5f / aspect);
        }
    }
}
