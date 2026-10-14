using UnityEngine;
namespace TimeCourier
{
    public enum LetterKind { Normal, Speed, Shield }
    public class LetterItem : MonoBehaviour
    {
        public LetterKind kind = LetterKind.Normal;
        public bool Collected { get; private set; }
        public bool TryCollect()
        {
            if (Collected) return false;
            Collected = true;
            gameObject.SetActive(false);
            return true;
        }
        public void ResetLetter() { Collected = false; gameObject.SetActive(true); }
    }
}
