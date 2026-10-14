using UnityEngine;
using UnityEngine.UI;
namespace TimeCourier
{
    // Render only the 3D world at a low resolution; UI stays sharp.
    [RequireComponent(typeof(Camera))]
    public class PixelCamera : MonoBehaviour
    {
        [Range(240, 1080)] public int verticalResolution = 480;
        public bool pixelEffect = true;
        private Camera view;
        private Camera screenCamera;
        private RenderTexture texture;
        private RawImage output;
        private GameObject display;
        private int previousWidth, previousHeight;
        private void Start()
        {
            view = GetComponent<Camera>();
            // Keep an enabled camera rendering to the display while the world
            // camera renders into a texture. This also suppresses the Editor's
            // "No cameras rendering" overlay.
            var screenObject = new GameObject("Pixel Screen Camera", typeof(Camera));
            screenObject.transform.SetParent(transform, false);
            screenCamera = screenObject.GetComponent<Camera>();
            screenCamera.cullingMask = 0;
            screenCamera.clearFlags = CameraClearFlags.SolidColor;
            screenCamera.backgroundColor = view.backgroundColor;
            screenCamera.depth = view.depth - 100;
            screenCamera.targetDisplay = view.targetDisplay;
            screenCamera.allowHDR = false;
            screenCamera.allowMSAA = false;
            display = new GameObject("Pixel World Display", typeof(RectTransform), typeof(Canvas));
            var canvas = display.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -20;
            var imageObject = new GameObject("World texture", typeof(RectTransform), typeof(RawImage));
            var rt = (RectTransform)imageObject.transform;
            rt.SetParent(display.transform, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            output = imageObject.GetComponent<RawImage>(); output.raycastTarget = false;
            UpdateTexture();
        }
        private void LateUpdate() { if (view != null) UpdateTexture(); }
        private void UpdateTexture()
        {
            if (!pixelEffect)
            { view.targetTexture = null; screenCamera.enabled = false; display.SetActive(false); return; }
            screenCamera.enabled = true;
            screenCamera.targetDisplay = view.targetDisplay;
            display.SetActive(true);
            int height = Mathf.Clamp(verticalResolution, 240, 1080);
            int width = Mathf.Max(1, Mathf.RoundToInt(height * (float)Screen.width / Mathf.Max(1, Screen.height)));
            if (texture == null || width != previousWidth || height != previousHeight)
            {
                ReleaseTexture();
                texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                texture.name = "Voxel world low resolution";
                texture.filterMode = FilterMode.Point;
                texture.antiAliasing = 1;
                texture.Create();
                previousWidth = width; previousHeight = height;
            }
            view.targetTexture = texture; output.texture = texture;
        }
        private void ReleaseTexture()
        {
            if (view != null) view.targetTexture = null;
            if (texture != null) { texture.Release(); Destroy(texture); texture = null; }
        }
        private void OnDisable()
        { if (view != null) view.targetTexture = null; if (screenCamera != null) screenCamera.enabled = false; if (display != null) display.SetActive(false); }
        private void OnDestroy() { ReleaseTexture(); if (display != null) Destroy(display); if (screenCamera != null) Destroy(screenCamera.gameObject); }
    }
}
