using UnityEngine;
using UnityEngine.EventSystems;

public class TimeHoldButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    private int? activePointerId;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId.HasValue)
            return;

        activePointerId = eventData.pointerId;

        if (DeliveryGameManager.Instance != null)
        {
            DeliveryGameManager.Instance
                .SetTimeButtonHeld(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activePointerId == eventData.pointerId)
            Release();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (activePointerId == eventData.pointerId)
            Release();
    }

    private void OnDisable()
    {
        Release();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            Release();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            Release();
    }

    private void Release()
    {
        activePointerId = null;

        if (DeliveryGameManager.Instance != null)
        {
            DeliveryGameManager.Instance
                .SetTimeButtonHeld(false);
        }
    }
}