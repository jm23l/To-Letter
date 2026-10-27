using UnityEngine;
using UnityEngine.EventSystems;
namespace LetterRun2D
{
    public class TimeHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsHeld { get; private set; }
        private int? pointer;
        public void OnPointerDown(PointerEventData e)
        { if (pointer.HasValue) return; pointer = e.pointerId; IsHeld = true; }
        public void OnPointerUp(PointerEventData e) { if (pointer == e.pointerId) Release(); }
        public void OnPointerExit(PointerEventData e) { if (pointer == e.pointerId) Release(); }
        public void Release() { pointer = null; IsHeld = false; }
        private void OnDisable() { Release(); }
    }
}
