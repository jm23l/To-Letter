using UnityEngine;
using UnityEngine.UI;

namespace LetterRun2D
{
    public enum RoundState { Menu, Playing, Paused, Result }

    public class DeliveryGame : MonoBehaviour
    {
        [Header("Rules")]
        [Min(1)] public float timeLimit = 60;
        [Min(1)] public int targetDeliveries = 10;
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
        public Button collectButton, startButton;
        public Button retryButton, menuButton, pauseButton, resumeButton;
        public GameObject menuPanel, resultPanel, pausePanel, controlsPanel;
        public MobileStick stick;
        public TimeHoldButton holdButton;

        public RoundState State { get; private set; } = RoundState.Menu;
        public float RemainingTime { get; private set; }
        public float Energy { get; private set; }
        public int Delivered { get; private set; }
        public bool IsSlowing { get; private set; }
        public TutorialGuide Tutorial { get; private set; }
        public DeliveryFeedback Feedback { get; private set; }
        public GameAudio Audio { get; private set; }
        public BicyclePass Bicycle { get; private set; }
        public RoundVariation Variation { get; private set; }
        public bool IsTraining => Tutorial != null && Tutorial.Active;
        public bool CanCollect => !IsTraining || Tutorial.CanCollect;
        private float finishDelay;
        public float ObstacleFactor => IsTraining ? 0 : (IsSlowing ? slowFactor : 1);
        public bool IsPlaying => State == RoundState.Playing && finishDelay <= 0;
        private bool exhausted;
        private float nextHitTime, hintUntil;
        private string hint = "";

        private void Start()
        {
            Audio = GetComponent<GameAudio>();
            if (Audio == null) Audio = gameObject.AddComponent<GameAudio>();
            Audio.Initialize();
            TownContentUpdate.Apply(this);
            Bicycle=BicyclePass.Attach(this);
            // Existing scenes also receive exactly one rush car: the road near the mailbox.
            MovingObstacle rushCar=null;
            foreach(var car in obstacles)
                if(car!=null && (rushCar==null || car.startPoint.y<rushCar.startPoint.y))rushCar=car;
            foreach(var car in obstacles)if(car!=null){car.rushEnabled=car==rushCar;car.stopAndGoEnabled=car!=rushCar;}
            MobileControlsLayout.ApplyLayout(this);
            PostalUI.Apply(this);
            Variation = GetComponent<RoundVariation>();
            if (Variation == null) Variation = gameObject.AddComponent<RoundVariation>();
            Variation.Initialize(this);
            AbilityOverhead.Attach(this);
            Feedback = GetComponent<DeliveryFeedback>();
            if (Feedback == null) Feedback = gameObject.AddComponent<DeliveryFeedback>();
            Feedback.Initialize(this);
            Tutorial = GetComponent<TutorialGuide>();
            if (Tutorial == null) Tutorial = gameObject.AddComponent<TutorialGuide>();
            Tutorial.Initialize(this);
            if (hintLabel != null) hintLabel.enabled = showGameplayHints;
            collectButton.onClick.AddListener(player.Collect);
            startButton.onClick.AddListener(BeginRound);
            retryButton.onClick.AddListener(BeginRound);
            menuButton.onClick.AddListener(ShowMenu);
            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            ShowMenu();
        }

        public void BeginRound()
        {
            finishDelay = 0;
            Bicycle?.ResetPass();
            Tutorial?.RestoreLetters();
            Variation?.Restore();
            Feedback?.Clear();
            ClearInput();
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
            Tutorial?.BeginIfNeeded();
            if (!IsTraining) { Variation?.Apply(); Audio?.Play(AudioCue.Start); }
            Notify($"능력 편지도 배달할 수 있어요. 총 {targetDeliveries}개를 배달하세요!", 4);
        }

        public void ShowMenu()
        {
            finishDelay = 0;
            Bicycle?.ResetPass();
            Tutorial?.Cancel();
            Variation?.Restore();
            Feedback?.Clear();
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
            if (State == RoundState.Playing && finishDelay > 0)
            {
                finishDelay = Mathf.Max(0, finishDelay - Time.unscaledDeltaTime);
                if (finishDelay <= 0) Finish(true);
                return;
            }
            if (!IsPlaying) return;
            if (IsTraining) { IsSlowing = Tutorial.PracticeSlow && !player.IsStunned && (holdButton.IsHeld || player.KeyboardSlowHeld); return; }
            float dt = Time.unscaledDeltaTime;
            RemainingTime = Mathf.Max(0, RemainingTime - dt);
            if (RemainingTime <= 0) { Finish(false); return; }
            bool wasSlowing = IsSlowing;
            bool held = !player.IsStunned && (holdButton.IsHeld || player.KeyboardSlowHeld);
            if (!held) exhausted = false;
            IsSlowing = held && !exhausted && Energy > 0;
            if (IsSlowing)
            {
                Energy = Mathf.Max(0, Energy - dt);
                if (Energy <= 0) { exhausted = true; IsSlowing = false; }
            }
            else Energy = Mathf.Min(maximumEnergy, Energy + energyRecovery * dt);
            if (IsSlowing != wasSlowing) Audio?.Play(IsSlowing ? AudioCue.SlowStart : AudioCue.SlowEnd);
        }

        private void LateUpdate()
        {
            if (hintLabel != null) hintLabel.enabled = showGameplayHints && IsPlaying;
            int seconds = Mathf.CeilToInt(RemainingTime);
            timerLabel.text = IsTraining ? "연습" : $"{seconds / 60:00}:{seconds % 60:00}";
            timerLabel.color = RemainingTime < 10 ? new Color(1, 0.45f, 0.35f) : Color.white;
            bagLabel.text = $"가방 {player.Carried}/{carryCapacity}";
            deliveryLabel.text = IsTraining ? "튜토리얼 · 시간 제한 없음" : $"배달  {Delivered}/{targetDeliveries}";
            energyFill.fillAmount = Energy / Mathf.Max(0.1f, maximumEnergy);
            collectButton.interactable = IsPlaying && CanCollect && !player.IsStunned && player.Carried < carryCapacity;
            pauseButton.interactable = IsPlaying;
            hintLabel.text = IsPlaying ? (Time.unscaledTime < hintUntil ? hint :
                (IsSlowing ? "장애물 감속 중 · 제한 시간은 계속 흘러요" : "이동: 가상 스틱 · 수집: 탭 · 감속: 홀드")) : "";
            if (IsPlaying && player.IsStunned)
                hintLabel.text = $"기절 {player.StunRemaining:0.0}초\n" + hintLabel.text;
            else if (IsPlaying && player.GraceRemaining > 0)
                hintLabel.text = $"피격 방지 {player.GraceRemaining:0.0}초\n" + hintLabel.text;
        }

        public void Deliver(Vector3 mailboxPosition)
        {
            if (!IsPlaying || player.IsStunned || player.Carried == 0) return;
            if (IsTraining && !Tutorial.CanDeliver) return;
            int amount = player.TakeAllLetters();
            Feedback?.Delivered(amount, player.transform.position, mailboxPosition);
            if (IsTraining) { Tutorial.OnDelivered(); return; }
            Delivered += amount;
            Notify($"편지 {amount}개를 배달했어요!");
            if (Delivered >= targetDeliveries)
            {
                finishDelay = 1.2f;
                ClearInput();
            }
        }

        public void Hit()
        {
            if (!IsPlaying || (IsTraining && !Tutorial.PracticeSlow) || player.IsProtected || Time.unscaledTime < nextHitTime) return;
            Audio?.Play(AudioCue.Hit);
            float penalty = IsTraining ? 0 : collisionPenalty;
            Feedback?.Hit(penalty,player.transform.position);
            nextHitTime = Time.unscaledTime + hitCooldown;
            RemainingTime = Mathf.Max(0, RemainingTime - penalty);
            player.ApplyStun(stunDuration, Mathf.Max(recoveryDuration, hitCooldown - stunDuration));
            IsSlowing = false;
            Notify($"충돌! 기절 · 남은 시간 {collisionPenalty:0.#}초 감소", stunDuration + recoveryDuration);
            if (RemainingTime <= 0) Finish(false);
        }

        public void Notify(string message, float duration = 2)
        { hint = message; hintUntil = Time.unscaledTime + duration; }

        private void Finish(bool won)
        {
            Audio?.Stop();
            Audio?.Play(won ? AudioCue.Win : AudioCue.Lose);
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
            Audio?.Stop();
            Audio?.Play(AudioCue.Pause);
            State = RoundState.Paused;
            ClearInput();
            pausePanel.SetActive(true);
        }
        public void Resume()
        {
            if (State != RoundState.Paused) return;
            Audio?.Play(AudioCue.Resume);
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
