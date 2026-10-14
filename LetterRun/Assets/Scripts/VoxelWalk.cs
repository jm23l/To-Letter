using UnityEngine;
namespace TimeCourier
{
    public class VoxelWalk : MonoBehaviour
    {
        public Rigidbody body;
        public Transform leftLeg, rightLeg;
        private float phase;
        private void Update()
        {
            if (body == null || leftLeg == null || rightLeg == null) return;
            float speed = new Vector2(body.linearVelocity.x, body.linearVelocity.z).magnitude;
            phase += speed * Time.deltaTime * 7;
            float angle = speed > 0.05f ? Mathf.Sin(phase) * 25 : 0;
            leftLeg.localRotation = Quaternion.Euler(angle, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(-angle, 0, 0);
        }
    }
}
