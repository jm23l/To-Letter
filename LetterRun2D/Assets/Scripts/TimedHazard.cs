using UnityEngine;
namespace LetterRun2D
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class TimedHazard : MonoBehaviour
    {
        public DeliveryGame game;
        [Min(0.1f)] public float safeDuration = 2.5f;
        [Min(0.1f)] public float warningDuration = 1.2f;
        [Min(0.1f)] public float dangerDuration = 1.3f;
        [Min(0)] public float initialDelay;
        private float elapsed;
        private SpriteRenderer art;
        private Sprite safe, warning, danger;
        public bool IsDangerous => elapsed >= safeDuration + warningDuration;
        private void Awake()
        {
            art = GetComponent<SpriteRenderer>();
            safe = Resources.Load<Sprite>("Sprites/hazard_safe");
            warning = Resources.Load<Sprite>("Sprites/hazard_warning");
            danger = Resources.Load<Sprite>("Sprites/hazard_danger");
        }
        public void ResetHazard(float multiplier) { elapsed = -initialDelay; ApplyArt(); }
        private void FixedUpdate()
        {
            if (game == null || !game.IsPlaying) return;
            elapsed += Time.fixedDeltaTime * game.ObstacleFactor;
            float cycle = safeDuration + warningDuration + dangerDuration;
            if (elapsed >= cycle) elapsed %= cycle;
            ApplyArt();
        }
        private void ApplyArt()
        {
            if (art == null) return;
            art.sprite = IsDangerous ? danger : elapsed >= safeDuration ? warning : safe;
        }
        private void OnTriggerEnter2D(Collider2D other) { Hit(other); }
        private void OnTriggerStay2D(Collider2D other) { Hit(other); }
        private void Hit(Collider2D other)
        {
            if (IsDangerous && game != null && other.GetComponent<CourierPlayer>() != null) game.Hit();
        }
    }
}
