using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameManager game;
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody2D body;
    private InputAction move;
    private float slowUntil;
    private float slowMultiplier = 1f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        move = new InputAction("Move", InputActionType.Value);
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        move.AddBinding("<Gamepad>/leftStick");
    }
    private void OnEnable() => move.Enable();
    private void OnDisable()
    {
        move.Disable();
        body.linearVelocity = Vector2.zero;
    }
    private void OnDestroy() => move.Dispose();
    private void FixedUpdate()
    {
        if (game == null || !game.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 input = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);
        float multiplier = Time.time < slowUntil ? slowMultiplier : 1f;
        body.linearVelocity = input * moveSpeed * multiplier;
    }
    public void ApplySlow(float duration, float multiplier)
    {
        if (Time.time < slowUntil) return; // 감속 중에는 중첩·갱신하지 않음
        slowUntil = Time.time + duration;
        slowMultiplier = Mathf.Clamp01(multiplier);
    }
}
