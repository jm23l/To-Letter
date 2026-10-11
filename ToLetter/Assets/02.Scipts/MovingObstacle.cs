using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class MovingObstacle : MonoBehaviour
{
    [Header("왕복 이동")]
    public Vector2 moveOffset = new Vector2(3f, 0f);
    [Min(0f)] public float moveSpeed = 2f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool movingToEnd = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        GetComponent<BoxCollider2D>().isTrigger = true;

        startPosition = rb.position;
        endPosition = startPosition + moveOffset;
    }

    private void FixedUpdate()
    {
        var game = DeliveryGameManager.Instance;

        if (game == null || !game.IsPlaying)
            return;

        Vector2 target = movingToEnd
            ? endPosition
            : startPosition;

        float speed =
            moveSpeed * game.ObstacleSpeedMultiplier;

        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            target,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        if ((nextPosition - target).sqrMagnitude < 0.0001f)
            movingToEnd = !movingToEnd;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHitPlayer(other);
    }

    private void TryHitPlayer(Collider2D other)
    {
        CourierPlayer player =
            other.GetComponentInParent<CourierPlayer>();

        if (player != null)
            player.HitObstacle();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 from = Application.isPlaying
            ? new Vector3(
                startPosition.x,
                startPosition.y,
                transform.position.z
            )
            : transform.position;

        Vector3 to = from + (Vector3)moveOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(to, 0.2f);
    }
}