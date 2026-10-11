using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LetterItem : MonoBehaviour
{
    public bool IsCollected { get; private set; }

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    public bool TryCollect()
    {
        if (IsCollected)
            return false;

        IsCollected = true;
        gameObject.SetActive(false);
        return true;
    }
}