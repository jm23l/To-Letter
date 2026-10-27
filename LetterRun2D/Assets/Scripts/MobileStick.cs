using UnityEngine;
using UnityEngine.EventSystems;
namespace LetterRun2D
{
    public class MobileStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform handle;
        public float radius = 42;
        public Vector2 Value { get; private set; }
        private int? pointer;
        public void OnPointerDown(PointerEventData e)
        { if (pointer.HasValue) return; pointer = e.pointerId; OnDrag(e); }
        public void OnDrag(PointerEventData e)
        {
            if (pointer != e.pointerId) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, e.position, e.pressEventCamera, out Vector2 local))
            {
                Value = Vector2.ClampMagnitude(local / Mathf.Max(1, radius), 1);
                handle.anchoredPosition = Value * radius;
            }
        }
        public void OnPointerUp(PointerEventData e) { if (pointer == e.pointerId) Release(); }
        public void Release()
        { pointer = null; Value = Vector2.zero; if (handle != null) handle.anchoredPosition = Vector2.zero; }
        private void OnDisable() { Release(); }
    }
}
