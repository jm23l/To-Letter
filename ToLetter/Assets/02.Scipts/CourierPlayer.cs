using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class CourierPlayer : MonoBehaviour
{
    [Header("이동")]
    [Min(0f)] public float moveSpeed = 4f;

    [Header("편지")]
    [Min(1)] public int maxCarry = 3;
    [Min(0.1f)] public float pickupRadius = 1.2f;
    public LayerMask letterLayer = ~0;

    [Header("장애물 충돌")]
    [Min(0f)] public float hitTimePenalty = 3f;
    [Min(0.1f)] public float hitCooldown = 1f;

    public int CarriedLetters { get; private set; }

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float nextHitTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        var game = DeliveryGameManager.Instance;

        if (game == null || !game.IsPlaying)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 keyboardInput = Vector2.zero;
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed ||
                keyboard.leftArrowKey.isPressed)
                keyboardInput.x -= 1f;

            if (keyboard.dKey.isPressed ||
                keyboard.rightArrowKey.isPressed)
                keyboardInput.x += 1f;

            if (keyboard.sKey.isPressed ||
                keyboard.downArrowKey.isPressed)
                keyboardInput.y -= 1f;

            if (keyboard.wKey.isPressed ||
                keyboard.upArrowKey.isPressed)
                keyboardInput.y += 1f;

            if (keyboard.eKey.wasPressedThisFrame)
                TryCollectLetter();
        }

        // 모바일 On-Screen Stick도 가상 Gamepad 값을 보낸다.
        // 연결된 패드 중 가장 크게 기울어진 스틱을 사용한다.
        Vector2 stickInput = Vector2.zero;

        foreach (var pad in Gamepad.all)
        {
            Vector2 value = pad.leftStick.ReadValue();

            if (value.sqrMagnitude > stickInput.sqrMagnitude)
                stickInput = value;
        }

        moveInput = keyboardInput.sqrMagnitude > 0f
            ? Vector2.ClampMagnitude(keyboardInput, 1f)
            : Vector2.ClampMagnitude(stickInput, 1f);
    }

    private void FixedUpdate()
    {
        var game = DeliveryGameManager.Instance;

        rb.linearVelocity =
            game != null && game.IsPlaying
            ? moveInput * moveSpeed
            : Vector2.zero;
    }

    // 수집 버튼의 On Click에 연결한다.
    public void TryCollectLetter()
    {
        var game = DeliveryGameManager.Instance;

        if (game == null || !game.IsPlaying)
            return;

        if (CarriedLetters >= maxCarry)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            pickupRadius,
            letterLayer
        );

        LetterItem closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            LetterItem letter =
                hit.GetComponentInParent<LetterItem>();

            if (letter == null || letter.IsCollected)
                continue;

            float distance = (
                letter.transform.position - transform.position
            ).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = letter;
            }
        }

        if (closest != null && closest.TryCollect())
            CarriedLetters++;
    }

    public void DeliverLetters()
    {
        var game = DeliveryGameManager.Instance;

        if (game == null || !game.IsPlaying ||
            CarriedLetters <= 0)
            return;

        int amount = CarriedLetters;
        CarriedLetters = 0;
        game.AddDelivery(amount);
    }

    public void HitObstacle()
    {
        var game = DeliveryGameManager.Instance;

        if (game == null || !game.IsPlaying)
            return;

        if (Time.unscaledTime < nextHitTime)
            return;

        nextHitTime = Time.unscaledTime + hitCooldown;
        game.ApplyTimePenalty(hitTimePenalty);
    }

    public void StopMoving()
    {
        moveInput = Vector2.zero;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void OnDisable()
    {
        StopMoving();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}