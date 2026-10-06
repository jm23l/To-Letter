using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private int target = 6;
    [SerializeField] private LetterInventory inventory;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private TMP_Text deliveredText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    private float remaining;
    private int delivered;
    public bool IsPlaying { get; private set; }

    private void Start()
    {
        remaining = timeLimit;
        delivered = 0;
        IsPlaying = true;
        if (resultPanel != null) resultPanel.SetActive(false);
        RefreshUI();
    }
    private void LateUpdate()
    {
        if (!IsPlaying) return;
        // 물리 충돌로 들어온 배달 판정 이후 시간 종료를 처리한다.
        remaining = Mathf.Max(0f, remaining - Time.deltaTime);
        if (delivered >= target) Finish(true);
        else if (remaining <= 0f) Finish(false);
        RefreshUI();
    }
    public void Deliver(int amount)
    {
        if (!IsPlaying || amount <= 0) return;
        delivered += amount;
        if (delivered >= target) Finish(true);
        RefreshUI();
    }
    private void Finish(bool success)
    {
        IsPlaying = false;
        if (resultPanel != null) resultPanel.SetActive(true);
        string message = success ? "배달 성공!" : "시간 초과!";
        message += $"\n배달: {delivered} / {target}";
        if (!success) message += $"\n부족한 편지: {Mathf.Max(0, target - delivered)}개";
        if (resultText != null) resultText.text = message;
        Debug.Log(message);
    }
    public void RefreshUI()
    {
        if (timeText != null) timeText.text = $"시간: {Mathf.CeilToInt(remaining)}";
        if (inventoryText != null && inventory != null) inventoryText.text = $"보유: {inventory.Count} / 3";
        if (deliveredText != null) deliveredText.text = $"배달: {delivered} / {target}";
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
