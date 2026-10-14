using UnityEngine;
namespace TimeCourier
{
    [RequireComponent(typeof(BoxCollider))]
    public class Mailbox : MonoBehaviour
    {
        public DeliveryGame game;
        private void OnTriggerEnter(Collider other) { TryDeliver(other); }
        private void OnTriggerStay(Collider other) { TryDeliver(other); }
        private void TryDeliver(Collider other)
        {
            if (game != null && other.GetComponent<CourierPlayer>() != null) game.Deliver();
        }
    }
}
