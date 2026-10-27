using UnityEngine;
using UnityEngine.UI;
namespace LetterRun2D
{
    // Session-local: the first start teaches three actions; retry immediately plays a normal round.
    public class TutorialGuide : MonoBehaviour
    {
        public bool Active { get; private set; }
        public bool PracticeSlow => Active && step == 3;
        private float slowPracticeTime;
        private MovingObstacle practiceCar;
        public bool CanCollect => step == 1;
        public bool CanDeliver => step == 2;
        private DeliveryGame game;
        private bool completed;
        private int step;
        private float completionTime;
        private Vector3 origin;
        private Vector3[] savedPositions;
        private bool positionsSaved;
        private LetterItem practiceLetter;
        private Mailbox mailbox;
        private RectTransform banner, marker, canvasRect;
        private Text instruction, target;
        private Button skip;
        public void Initialize(DeliveryGame owner)
        {
            game = owner;
            mailbox = game.GetComponentInChildren<Mailbox>(true);
            canvasRect = (RectTransform)game.hintLabel.canvas.transform;
            var safe = game.hintLabel.transform.parent;
            banner = DeliveryFeedback.CreateRect("Tutorial instructions", safe, new Vector2(.5f,1),new Vector2(0,-151),new Vector2(444,86));
            var bg=banner.gameObject.AddComponent<Image>();bg.color=new Color32(16,27,45,244);bg.raycastTarget=false;
            instruction=DeliveryFeedback.CreateText("Step",banner,18,new Color32(255,220,155,255));
            instruction.rectTransform.offsetMin=new Vector2(12,24);instruction.rectTransform.offsetMax=new Vector2(-12,-4);
            var button=DeliveryFeedback.CreateRect("Skip tutorial",banner,new Vector2(1,0),new Vector2(-63,13),new Vector2(120,24));
            var buttonImage=button.gameObject.AddComponent<Image>();buttonImage.color=new Color32(49,78,101,255);
            skip=button.gameObject.AddComponent<Button>();skip.targetGraphic=buttonImage;
            skip.navigation=new Navigation{mode=Navigation.Mode.None};
            DeliveryFeedback.CreateText("Skip label",button,13,Color.white).text="연습 건너뛰기";
            skip.onClick.AddListener(CompleteAndStart);
            marker=DeliveryFeedback.CreateRect("Tutorial target",canvasRect,Vector2.zero,Vector2.zero,new Vector2(190,38));
            target=DeliveryFeedback.CreateText("Target label",marker,17,new Color32(255,220,155,255));
            var outline=target.gameObject.AddComponent<Outline>();outline.effectColor=new Color(0,0,0,.9f);outline.effectDistance=new Vector2(1,-1);
            Hide();
        }
        public void BeginIfNeeded()
        {
            if(completed || game == null || mailbox == null) return;
            practiceLetter=null;
            foreach(var letter in game.letters) if(letter!=null && letter.kind==LetterKind.Normal){practiceLetter=letter;break;}
            if(practiceLetter==null) return;
            savedPositions=new Vector3[game.letters.Length];
            for(int i=0;i<game.letters.Length;i++)
            {
                if(game.letters[i]==null) continue;
                savedPositions[i]=game.letters[i].transform.position;
                game.letters[i].gameObject.SetActive(false);
            }
            positionsSaved=true;
            origin=game.player.transform.position;
            practiceLetter.transform.position=origin+Vector3.right*2f;
            practiceLetter.ResetLetter();
            step=0;completionTime=0;Active=true;
            RefreshText();
        }
        public void OnCollected()
        {
            if(!Active || step!=1) return;
            step=2;RefreshText();
        }
        public void OnDelivered()
        {
            if(!Active || step!=2) return;
            step=3;completionTime=0;slowPracticeTime=0;
            practiceCar=null;
            foreach(var car in game.obstacles)if(car!=null && car.rushEnabled){practiceCar=car;break;}
            if(practiceCar==null){step=4;RefreshText();return;}
            game.player.StopMoving();
            var position=new Vector2(0,practiceCar.startPoint.y-1.1f);
            game.player.GetComponent<Rigidbody2D>().position=position;
            game.player.transform.position=position;
            practiceCar.ResetObstacle(1);
            RefreshText();
        }
        private void RefreshText()
        {
            string[] lines={
                "1/4 이동\n왼쪽 스틱으로 편지 쪽으로 움직여 보세요.",
                "2/4 편지 줍기\n편지 가까이에서 ‘편지 줍기’를 탭하세요.",
                "3/4 배달\n빨간 우체통 가까이 가면 자동으로 배달돼요.",
                "4/4 감속하며 도로 건너기 · 충돌하면 기절\n감속을 꾹 누르며 스틱으로 위쪽까지 이동하세요.",
                $"연습 완료!\n이제 {game.timeLimit:0}초 동안 편지 {game.targetDeliveries}개를 배달해 보세요."};
            instruction.text=lines[step];
        }
        private void Update()
        {
            if(!Active || !game.IsPlaying) return;
            if(step==0 && Vector2.Distance(origin,game.player.transform.position)>.4f){step=1;RefreshText();}
            if(step==3)
            {
                if(game.IsSlowing)slowPracticeTime+=Time.unscaledDeltaTime;
                if(practiceCar!=null && slowPracticeTime>=.7f && game.player.transform.position.y>practiceCar.startPoint.y+.8f)
                {step=4;completionTime=0;RefreshText();}
            }
            if(step==4){completionTime+=Time.unscaledDeltaTime;if(completionTime>=2.2f)CompleteAndStart();}
        }
        private void LateUpdate()
        {
            if(game==null)return;
            bool show=Active && game.IsPlaying;
            banner.gameObject.SetActive(show);
            marker.gameObject.SetActive(show && step<4);
            if(!show || step>=4)return;
            if(step==3 && practiceCar!=null)
            {
                target.text="도로를 건너 여기로 ↓";
                marker.anchoredPosition=DeliveryFeedback.Project(canvasRect,new Vector3(0,practiceCar.startPoint.y+1.1f,0))+new Vector2(0,34);
                return;
            }
            Transform destination=step==2?mailbox.transform:practiceLetter.transform;
            target.text=step==2?"여기에 배달 ↓":"이 편지를 주워요 ↓";
            marker.anchoredPosition=DeliveryFeedback.Project(canvasRect,destination.position)+new Vector2(0,34);
        }
        public void RestoreLetters()
        {
            if(!positionsSaved)return;
            for(int i=0;i<game.letters.Length && i<savedPositions.Length;i++)
                if(game.letters[i]!=null){game.letters[i].transform.position=savedPositions[i];game.letters[i].ResetLetter();}
            positionsSaved=false;
        }
        public void Cancel(){Active=false;RestoreLetters();Hide();}
        private void Hide(){if(banner!=null)banner.gameObject.SetActive(false);if(marker!=null)marker.gameObject.SetActive(false);}
        private void CompleteAndStart()
        {
            if(!Active || !game.IsPlaying)return;
            completed=true;Active=false;RestoreLetters();Hide();game.BeginRound();
        }
    }
}
