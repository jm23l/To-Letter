using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Mailbox : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Deliver(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Deliver(other);
    }
    
    private void Deliver(Collider2D other)
    {
        CourierPlayer player =
            other.GetComponentInParent<CourierPlayer>();

        if (player != null)
            player.DeliverLetters();
    }
}