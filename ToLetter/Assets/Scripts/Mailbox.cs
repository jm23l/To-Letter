using UnityEngine;

public class Mailbox : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        LetterInventory inventory = other.GetComponentInParent<LetterInventory>();
        if (inventory != null) inventory.DeliverAll();
    }
}
