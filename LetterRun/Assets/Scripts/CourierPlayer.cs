using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeCourier
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class CourierPlayer : MonoBehaviour
    {
        public DeliveryGame game;
        public MobileStick stick;
        public Transform visual;
        public float moveSpeed = 3.2f;
        public float pickupRadius = 0.85f;
        public Vector2 spawnPoint = new Vector2(0, -2.35f);
        public int Carried { get; private set; }
        public bool KeyboardSlowHeld => slowAction != null && slowAction.IsPressed();
        private Rigidbody body;
        private InputAction moveAction, collectAction, slowAction;
        private Vector2 movement;
        public float StunRemaining { get; private set; }
        public float SpeedRemaining { get; private set; }
        public float ShieldRemaining { get; private set; }
        public float GraceRemaining { get; private set; }
        public bool IsStunned => StunRemaining > 0;
        public bool IsProtected => ShieldRemaining > 0 || GraceRemaining > 0;
        public string StatusText => IsStunned ? $"기절 {StunRemaining:0.0}초" :
            (SpeedRemaining > 0 ? $"가속 {SpeedRemaining:0.0}초  " : "") +
            (ShieldRemaining > 0 ? $"보호 {ShieldRemaining:0.0}초" :
            (GraceRemaining > 0 ? $"피격 방지 {GraceRemaining:0.0}초" : ""));

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");
            collectAction = new InputAction("Collect", InputActionType.Button, "<Keyboard>/e");
            slowAction = new InputAction("Slow", InputActionType.Button, "<Keyboard>/space");
        }
        private void OnEnable()
        { moveAction?.Enable(); collectAction?.Enable(); slowAction?.Enable(); }
        private void OnDisable()
        { moveAction?.Disable(); collectAction?.Disable(); slowAction?.Disable(); StopMoving(); }
        private void OnDestroy()
        { moveAction?.Dispose(); collectAction?.Dispose(); slowAction?.Dispose(); }
        private void Update()
        {
            if (game == null || !game.IsPlaying) { StopMoving(); return; }
            float dt = Time.unscaledDeltaTime;
            StunRemaining = Mathf.Max(0, StunRemaining - dt);
            GraceRemaining = Mathf.Max(0, GraceRemaining - dt);
            SpeedRemaining = Mathf.Max(0, SpeedRemaining - dt);
            ShieldRemaining = Mathf.Max(0, ShieldRemaining - dt);
            if (IsStunned) { StopMoving(); return; }
            Vector2 keyboard = moveAction.ReadValue<Vector2>();
            Vector2 touch = stick != null ? stick.Value : Vector2.zero;
            movement = Vector2.ClampMagnitude(touch.sqrMagnitude > 0 ? touch : keyboard, 1);
            if (collectAction.WasPressedThisFrame()) Collect();
        }
        private void FixedUpdate()
        {
            if (body != null)
                {
                    Vector3 forward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
                    forward.y = 0; forward.Normalize();
                    Vector3 right = Vector3.Cross(Vector3.up, forward);
                    Vector3 velocity = (right * movement.x + forward * movement.y) * moveSpeed * (SpeedRemaining > 0 ? 1.5f : 1f);
                    body.linearVelocity = game != null && game.IsPlaying && !IsStunned ? velocity : Vector3.zero;
                    if (velocity.sqrMagnitude > 0.01f && game != null && game.IsPlaying && !IsStunned && visual != null)
                        visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(velocity), 12f * Time.fixedDeltaTime);
                }
        }
        public void Collect()
        {
            if (game == null || !game.IsPlaying || IsStunned) return;
            if (Carried >= game.carryCapacity) { game.Notify("가방이 가득 찼어요. 우체통에 배달하세요."); return; }
            LetterItem nearest = null;
            float best = pickupRadius * pickupRadius;
            foreach (var letter in game.letters)
            {
                if (letter == null || !letter.gameObject.activeInHierarchy || letter.Collected) continue;
                float distance = new Vector2(letter.transform.position.x - transform.position.x, letter.transform.position.z - transform.position.z).sqrMagnitude;
                if (distance <= best) { nearest = letter; best = distance; }
            }
            if (nearest == null) { game.Notify("편지에 조금 더 가까이 가세요."); return; }
            if (nearest.TryCollect())
            {
                Carried++;
                if (nearest.kind == LetterKind.Speed)
                { SpeedRemaining = 3; game.Notify("편지 획득! 3초 동안 이동 속도 1.5배!"); }
                else if (nearest.kind == LetterKind.Shield)
                { ShieldRemaining = 4; game.Notify("편지 획득! 4초 동안 충돌 피해를 막아요!"); }
                else
                {
                    game.Notify(Carried == game.carryCapacity ? "가방이 가득 찼어요. 우체통으로!" : "편지를 주웠어요!");
                }
            }
        }
        public int TakeAllLetters() { int count = Carried; Carried = 0; return count; }
        public void ApplyStun(float seconds, float recoverySeconds)
        {
            StunRemaining = Mathf.Max(0, seconds);
            GraceRemaining = StunRemaining + Mathf.Max(0, recoverySeconds);
            StopMoving();
        }
        public void ResetPlayer()
        {
            Carried = 0;
            StunRemaining = SpeedRemaining = ShieldRemaining = GraceRemaining = 0;
            ReturnToSpawn();
        }
        public void ReturnToSpawn()
        {
            StopMoving();
            if (body == null) body = GetComponent<Rigidbody>();
            body.position = new Vector3(spawnPoint.x, 0.04f, spawnPoint.y);
            transform.position = body.position;
        }
        public void StopMoving()
        { movement = Vector2.zero; if (body != null) body.linearVelocity = Vector3.zero; }
        private void OnDrawGizmosSelected()
        { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, pickupRadius); }
    }
}
