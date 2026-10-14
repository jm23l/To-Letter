using UnityEngine;
using UnityEngine.UI;

namespace TimeCourier
{
    public enum RoundState { Menu, Playing, Paused, Result }

    public class DeliveryGame : MonoBehaviour
    {
        [Header("Rules")]
        [Min(1)] public float timeLimit = 60;
        [Min(1)] public int targetDeliveries = 6;
        [Min(1)] public int carryCapacity = 3;
        [Min(0)] public float collisionPenalty = 3;
        [Min(0.1f)] public float hitCooldown = 1;
        [Min(0)] public float stunDuration = 0.8f;
        [Min(0)] public float recoveryDuration = 1.4f;
        [Header("Time ability")]
        [Min(0.1f)] public float maximumEnergy = 5;
        [Min(0)] public float energyRecovery = 0.8f;
        [Range(0.05f, 1)] public float slowFactor = 0.3f;
        [Header("World")]
        public CourierPlayer player;
        public LetterItem[] letters;
        public MovingObstacle[] obstacles;
        [Header("UI")]
        public Text timerLabel, bagLabel, deliveryLabel, hintLabel, resultLabel;
        [Tooltip("중앙 아래 안내 문구 표시")]
        public bool showGameplayHints = false;
        public Image energyFill;
        public Button collectButton, districtOneButton, districtTwoButton;
        public Button retryButton, menuButton, pauseButton, resumeButton;
        public GameObject menuPanel, resultPanel, pausePanel, controlsPanel;
        public MobileStick stick;
        public TimeHoldButton holdButton;

        public RoundState State { get; private set; } = RoundState.Menu;
        public float RemainingTime { get; private set; }
        public float Energy { get; private set; }
        public int Delivered { get; private set; }
        public bool IsSlowing { get; private set; }
        public float ObstacleFactor => IsSlowing ? slowFactor : 1;
        public bool IsPlaying => State == RoundState.Playing;
        private bool exhausted;
        private float nextHitTime, hintUntil;
        private string hint = "";

        private void Start()
        {
            GameplayExpansion.ConfigureStartMenu(this);
            GameplayExpansion.Install(this);
            if (hintLabel != null) hintLabel.enabled = showGameplayHints;
            collectButton.onClick.AddListener(player.Collect);
            districtOneButton.onClick.AddListener(BeginRound);
            retryButton.onClick.AddListener(BeginRound);
            menuButton.onClick.AddListener(ShowMenu);
            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            ShowMenu();
        }

        public void BeginRound()
        {
            RemainingTime = timeLimit;
            Delivered = 0;
            Energy = maximumEnergy;
            exhausted = false;
            IsSlowing = false;
            nextHitTime = 0;
            hintUntil = 0;
            player.ResetPlayer();
            foreach (var letter in letters) letter.ResetLetter();
            foreach (var obstacle in obstacles)
                obstacle.ResetObstacle(1);
            foreach (var hazard in GetComponentsInChildren<TimedHazard>(true))
                hazard.ResetHazard(1);
            State = RoundState.Playing;
            menuPanel.SetActive(false);
            resultPanel.SetActive(false);
            pausePanel.SetActive(false);
            controlsPanel.SetActive(true);
            Notify("능력 편지도 배달할 수 있어요. 총 6개를 배달하세요!", 4);
        }

        public void ShowMenu()
        {
            State = RoundState.Menu;
            ClearInput();
            RemainingTime = timeLimit;
            Energy = maximumEnergy;
            Delivered = 0;
            player.ResetPlayer();
            foreach (var letter in letters) letter.ResetLetter();
            foreach (var obstacle in obstacles) obstacle.ResetObstacle(1);
            foreach (var hazard in GetComponentsInChildren<TimedHazard>(true)) hazard.ResetHazard(1);
            menuPanel.SetActive(true);
            resultPanel.SetActive(false);
            pausePanel.SetActive(false);
            controlsPanel.SetActive(false);
        }

        private void Update()
        {
            if (!IsPlaying) return;
            float dt = Time.unscaledDeltaTime;
            RemainingTime = Mathf.Max(0, RemainingTime - dt);
            if (RemainingTime <= 0) { Finish(false); return; }
            bool held = !player.IsStunned && (holdButton.IsHeld || player.KeyboardSlowHeld);
            if (!held) exhausted = false;
            IsSlowing = held && !exhausted && Energy > 0;
            if (IsSlowing)
            {
                Energy = Mathf.Max(0, Energy - dt);
                if (Energy <= 0) { exhausted = true; IsSlowing = false; }
            }
            else Energy = Mathf.Min(maximumEnergy, Energy + energyRecovery * dt);
        }

        private void LateUpdate()
        {
            if (hintLabel != null) hintLabel.enabled = showGameplayHints && IsPlaying;
            int seconds = Mathf.CeilToInt(RemainingTime);
            timerLabel.text = $"{seconds / 60:00}:{seconds % 60:00}";
            timerLabel.color = RemainingTime < 10 ? new Color(1, 0.45f, 0.35f) : Color.white;
            bagLabel.text = $"가방 {player.Carried}/{carryCapacity}";
            deliveryLabel.text = $"배달  {Delivered}/{targetDeliveries}";
            energyFill.fillAmount = Energy / Mathf.Max(0.1f, maximumEnergy);
            collectButton.interactable = IsPlaying && !player.IsStunned && player.Carried < carryCapacity;
            pauseButton.interactable = IsPlaying;
            hintLabel.text = IsPlaying ? (Time.unscaledTime < hintUntil ? hint :
                (IsSlowing ? "장애물 감속 중 · 제한 시간은 계속 흘러요" : "이동: 가상 스틱 · 수집: 탭 · 감속: 홀드")) : "";
            if (IsPlaying && player.IsStunned)
                hintLabel.text = $"기절 {player.StunRemaining:0.0}초\n" + hintLabel.text;
            else if (IsPlaying && player.GraceRemaining > 0)
                hintLabel.text = $"피격 방지 {player.GraceRemaining:0.0}초\n" + hintLabel.text;
        }

        public void Deliver()
        {
            if (!IsPlaying || player.IsStunned || player.Carried == 0) return;
            int amount = player.TakeAllLetters();
            Delivered += amount;
            Notify($"편지 {amount}개를 배달했어요!");
            if (Delivered >= targetDeliveries) Finish(true);
        }

        public void Hit()
        {
            if (!IsPlaying || player.IsProtected || Time.unscaledTime < nextHitTime) return;
            nextHitTime = Time.unscaledTime + hitCooldown;
            RemainingTime = Mathf.Max(0, RemainingTime - collisionPenalty);
            player.ApplyStun(stunDuration, Mathf.Max(recoveryDuration, hitCooldown - stunDuration));
            IsSlowing = false;
            Notify($"충돌! 기절 · 남은 시간 {collisionPenalty:0.#}초 감소", stunDuration + recoveryDuration);
            if (RemainingTime <= 0) Finish(false);
        }

        public void Notify(string message, float duration = 2)
        { hint = message; hintUntil = Time.unscaledTime + duration; }

        private void Finish(bool won)
        {
            State = RoundState.Result;
            ClearInput();
            controlsPanel.SetActive(false);
            resultPanel.SetActive(true);
            resultLabel.text = (won ? "배달 성공!" : "시간 종료!") +
                $"\n\n배달한 편지  {Delivered}/{targetDeliveries}\n남은 시간  {Mathf.CeilToInt(RemainingTime)}초";
        }

        public void Pause()
        {
            if (!IsPlaying) return;
            State = RoundState.Paused;
            ClearInput();
            pausePanel.SetActive(true);
        }
        public void Resume()
        {
            if (State != RoundState.Paused) return;
            ClearInput();
            State = RoundState.Playing;
            pausePanel.SetActive(false);
        }
        private void ClearInput()
        {
            IsSlowing = false;
            stick.Release();
            holdButton.Release();
            player.StopMoving();
        }
        private void OnApplicationFocus(bool focused) { if (!Application.isEditor && !focused) Pause(); }
        private void OnApplicationPause(bool paused) { if (!Application.isEditor && paused) Pause(); }
    }
}
