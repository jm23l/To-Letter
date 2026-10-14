using UnityEngine;
namespace TimeCourier
{
    public class SafeArea : MonoBehaviour
    {
        private Rect previous;
        private Vector2 size;
        private void Update()
        {
            Rect safe = Screen.safeArea;
            Vector2 screen = new Vector2(Screen.width, Screen.height);
            if (screen.x <= 0 || screen.y <= 0 || (safe == previous && size == screen)) return;
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(safe.xMin / screen.x, safe.yMin / screen.y);
            rt.anchorMax = new Vector2(safe.xMax / screen.x, safe.yMax / screen.y);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            previous = safe; size = screen;
        }
    }
}
