using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DeliveryGameManager : MonoBehaviour
{
    public static DeliveryGameManager Instance { get; private set; }

    [Header("게임 규칙")]
    [Min(1f)] public float timeLimit = 60f;
    [Min(1)] public int targetDeliveryCount = 6;

    [Header("시간 능력")]
    [Min(0.1f)] public float maxEnergy = 5f;
    [Min(0f)] public float energyRecoveryPerSecond = 0.8f;
    [Range(0.05f, 1f)] public float slowMultiplier = 0.3f;

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text inventoryText;
    public TMP_Text deliveredText;
    public Slider energySlider;
    public GameObject resultPanel;
    public TMP_Text resultText;

    [Header("플레이어 연결")]
    public CourierPlayer player;

    public bool IsPlaying { get; private set; }
    public bool IsSlowing { get; private set; }
    public float RemainingTime { get; private set; }
    public float Energy { get; private set; }
    public int DeliveredCount { get; private set; }

    public float ObstacleSpeedMultiplier =>
        IsSlowing ? slowMultiplier : 1f;

    private bool touchHold;
    private bool exhausted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RemainingTime = timeLimit;
        Energy = maxEnergy;
        IsPlaying = true;

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsPlaying)
            return;

        // 시간 능력과 관계없이 실제 시간 기준으로 감소한다.
        float dt = Time.unscaledDeltaTime;
        RemainingTime = Mathf.Max(0f, RemainingTime - dt);

        if (RemainingTime <= 0f)
        {
            EndGame(false);
            return;
        }

        bool keyboardHold =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.isPressed;

        bool wantsSlow = touchHold || keyboardHold;

        // 게이지가 바닥나면 버튼을 놓아야 다시 사용할 수 있다.
        if (!wantsSlow)
            exhausted = false;

        IsSlowing = wantsSlow && !exhausted && Energy > 0f;

        if (IsSlowing)
        {
            Energy = Mathf.Max(0f, Energy - dt);

            if (Energy <= 0f)
            {
                exhausted = true;
                IsSlowing = false;
            }
        }
        else
        {
            Energy = Mathf.Min(
                maxEnergy,
                Energy + energyRecoveryPerSecond * dt
            );
        }
    }

    private void LateUpdate()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(RemainingTime);
            timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
        }

        if (inventoryText != null && player != null)
        {
            inventoryText.text =
                $"편지 {player.CarriedLetters}/{player.maxCarry}";
        }

        if (deliveredText != null)
        {
            deliveredText.text =
                $"배달 {DeliveredCount}/{targetDeliveryCount}";
        }

        if (energySlider != null)
        {
            energySlider.minValue = 0f;
            energySlider.maxValue = maxEnergy;
            energySlider.value = Energy;
        }
    }

    public void SetTimeButtonHeld(bool held)
    {
        touchHold = held;
    }

    public void AddDelivery(int amount)
    {
        if (!IsPlaying || amount <= 0)
            return;

        DeliveredCount += amount;

        if (DeliveredCount >= targetDeliveryCount)
            EndGame(true);
    }

    public void ApplyTimePenalty(float seconds)
    {
        if (!IsPlaying)
            return;

        RemainingTime = Mathf.Max(
            0f,
            RemainingTime - Mathf.Max(0f, seconds)
        );

        if (RemainingTime <= 0f)
            EndGame(false);
    }

    private void EndGame(bool success)
    {
        IsPlaying = false;
        IsSlowing = false;
        touchHold = false;

        if (player != null)
            player.StopMoving();

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text = success
                ? $"배달 성공!\n총 {DeliveredCount}개 배달"
                : $"시간 종료!\n총 {DeliveredCount}개 배달";
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            touchHold = false;
            IsSlowing = false;
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            touchHold = false;
            IsSlowing = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}