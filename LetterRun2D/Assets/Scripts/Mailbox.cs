using UnityEngine;
namespace LetterRun2D
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Mailbox : MonoBehaviour
    {
        public DeliveryGame game;
        private void OnTriggerEnter2D(Collider2D other) { TryDeliver(other); }
        private void OnTriggerStay2D(Collider2D other) { TryDeliver(other); }
        private void TryDeliver(Collider2D other)
        {
            if (game != null && other.GetComponent<CourierPlayer>() != null) game.Deliver(transform.position);
        }
    }
}
