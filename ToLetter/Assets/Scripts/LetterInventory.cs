using UnityEngine;

public class LetterInventory : MonoBehaviour
{
    [SerializeField] private GameManager game;
    [SerializeField] private int capacity = 3;
    public int Count { get; private set; }
    public bool TryCollect()
    {
        if (game == null || !game.IsPlaying || Count >= capacity) return false;
        Count++;
        game.RefreshUI();
        return true;
    }
    public void DeliverAll()
    {
        if (game == null || !game.IsPlaying || Count == 0) return;
        int delivered = Count;
        Count = 0;
        game.Deliver(delivered);
    }
}
