using UnityEngine;

public class LetterPickup : MonoBehaviour
{
    private bool collected;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (collected) return;
        LetterInventory inventory = other.GetComponentInParent<LetterInventory>();
        if (inventory == null || !inventory.TryCollect()) return;
        collected = true; 
        gameObject.SetActive(false);
    }
}
