using UnityEngine;

public class Puddle : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField, Range(0f, 1f)] private float multiplier = 0.5f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null) player.ApplySlow(duration, multiplier);
    }
}
