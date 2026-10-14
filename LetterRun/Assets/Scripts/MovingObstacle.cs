using UnityEngine;
namespace TimeCourier
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovingObstacle : MonoBehaviour
    {
        public DeliveryGame game;
        public Vector3 startPoint, endPoint;
        public float moveSpeed = 1.5f;
        private Rigidbody body;
        private bool toEnd = true;
        private float districtMultiplier = 1;
        private void Awake() { body = GetComponent<Rigidbody>(); }
        public void ResetObstacle(float multiplier)
        {
            if (body == null) body = GetComponent<Rigidbody>();
            body.position = startPoint;
            transform.position = startPoint;
            toEnd = true;
            districtMultiplier = multiplier;
        }
        private void FixedUpdate()
        {
            if (game == null || !game.IsPlaying)
            { return; }
            Vector3 target = toEnd ? endPoint : startPoint;
            Vector3 next = Vector3.MoveTowards(body.position, target,
                moveSpeed * districtMultiplier * game.ObstacleFactor * Time.fixedDeltaTime);
            body.MovePosition(next);
            if ((next - target).sqrMagnitude < 0.0001f) toEnd = !toEnd;
        }
        private void OnTriggerEnter(Collider other) { Hit(other); }
        private void OnTriggerStay(Collider other) { Hit(other); }
        private void Hit(Collider other)
        { if (game != null && other.GetComponent<CourierPlayer>() != null) game.Hit(); }
        private void OnDrawGizmosSelected()
        { Gizmos.color = Color.red; Gizmos.DrawLine(startPoint, endPoint); }
    }
}
