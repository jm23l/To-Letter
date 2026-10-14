using UnityEngine;

namespace TimeCourier
{
    // A fixed floor obstacle: safe -> warning -> raised barrier -> safe.
    public class TimedHazard : MonoBehaviour
    {
        public DeliveryGame game;
        public VoxelTint warningTile;
        public GameObject raisedBarrier;
        [Min(0.1f)] public float safeSeconds = 2.5f;
        [Min(0.1f)] public float warningSeconds = 1.2f;
        [Min(0.1f)] public float dangerSeconds = 1.3f;
        [Min(0)] public float initialDelay;
        public bool IsDangerous { get; private set; }
        private float elapsed, speed = 1;
        private int lastPhase = -1;

        public void ResetHazard(float multiplier)
        {
            elapsed = -initialDelay;
            speed = multiplier;
            lastPhase = -1;
            SetPhase(0);
        }

        private void FixedUpdate()
        {
            if (game == null || !game.IsPlaying) return;
            elapsed += Time.fixedDeltaTime * speed * game.ObstacleFactor;
            float safe = Mathf.Max(0.1f, safeSeconds);
            float warning = Mathf.Max(0.1f, warningSeconds);
            float danger = Mathf.Max(0.1f, dangerSeconds);
            float cycle = safe + warning + danger;
            if (elapsed >= cycle) elapsed = Mathf.Repeat(elapsed, cycle);
            SetPhase(elapsed < safe ? 0 : elapsed < safe + warning ? 1 : 2);
        }

        private void SetPhase(int phase)
        {
            IsDangerous = phase == 2;
            if (phase == lastPhase) return;
            lastPhase = phase;
            if (raisedBarrier != null) raisedBarrier.SetActive(IsDangerous);
            if (warningTile == null) return;
            warningTile.color = phase == 0 ? new Color32(92, 110, 121, 255) :
                phase == 1 ? new Color32(255, 203, 70, 255) : new Color32(235, 65, 60, 255);
            warningTile.Apply();
        }

        private void OnTriggerEnter(Collider other) { CheckHit(other); }
        private void OnTriggerStay(Collider other) { CheckHit(other); }
        private void CheckHit(Collider other)
        {
            if (IsDangerous && game != null && game.IsPlaying &&
                other.GetComponent<CourierPlayer>() != null) game.Hit();
        }
    }
}
