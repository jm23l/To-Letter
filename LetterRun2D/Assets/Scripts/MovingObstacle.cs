using UnityEngine;
using UnityEngine.UI;
namespace LetterRun2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingObstacle : MonoBehaviour
    {
        public DeliveryGame game;
        public Vector2 startPoint, endPoint;
        public float moveSpeed = 1.5f;
        [Min(.1f)] public float normalSpeedMultiplier = 2f;
        [Header("Rush cycle (one car)")]
        public bool rushEnabled;
        [Min(.1f)] public float cruiseDuration = 3f;
        [Min(.5f)] public float warningDuration = 1f;
        [Min(.1f)] public float rushDuration = 3f;
        [Min(1)] public float rushMultiplier = 2f;
        [Header("Stop and depart (other car)")]
        public bool stopAndGoEnabled;
        [Min(.1f)] public float stopDuration = 1f;
        [Min(.1f)] public float departureWarningDuration = .7f;
        private int stopPhase;
        private float stopTime;
        private bool stoppedThisLeg;
        private RectTransform departureBadge, badgeCanvas;
        private Text departureText;
        public bool RushActive => rushEnabled && phase!=0;
        private int phase;
        private float phaseTime;
        private RectTransform warningPanel;
        private Text warningText;
        private Rigidbody2D body;
        private RectTransform CreateBadge(string name, out Text text)
        {
            badgeCanvas=(RectTransform)game.hintLabel.canvas.transform;
            var rect=DeliveryFeedback.CreateRect(name,badgeCanvas,Vector2.zero,Vector2.zero,new Vector2(44,32));
            text=DeliveryFeedback.CreateText("Symbol",rect,25,Color.white);
            text.fontStyle=FontStyle.Bold;
            var outline=text.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(0,0,0,.85f);outline.effectDistance=new Vector2(1,-1);
            rect.gameObject.SetActive(false);
            return rect;
        }
        private void FollowCar(RectTransform badge,float pulse,bool bob)
        {
            var position=DeliveryFeedback.Project(badgeCanvas,transform.position+Vector3.up*.65f);
            position.y+=bob?2.5f*pulse:0;
            badge.anchoredPosition=new Vector2(Mathf.Clamp(position.x,26,badgeCanvas.rect.width-26),Mathf.Clamp(position.y,20,badgeCanvas.rect.height-20));
            badge.localScale=Vector3.one*(1+.06f*pulse);
        }
        private void UpdateDepartureBadge()
        {
            bool visible=stopAndGoEnabled && game!=null && game.IsPlaying && !game.IsTraining && stopPhase!=0;
            if(visible && departureBadge==null)departureBadge=CreateBadge("Departure icon",out departureText);
            if(departureBadge==null)return;
            departureBadge.gameObject.SetActive(visible);
            if(!visible)return;
            float pulse=stopPhase==1?0:Mathf.Sin(stopTime*10);
            FollowCar(departureBadge,pulse,false);
            string arrow=(toEnd?endPoint.x:startPoint.x)>body.position.x?"→":"←";
            departureText.text=stopPhase==1?"Ⅱ":arrow;
            departureText.color=stopPhase==1?new Color32(150,215,231,255):Color.Lerp(new Color32(255,187,65,255),new Color32(255,240,174,255),.5f+.5f*pulse);
        }
        private void LateUpdate()
        {
            UpdateDepartureBadge();
            bool active=rushEnabled && game!=null && game.IsPlaying && !game.IsTraining && phase!=0;
            if(active && warningPanel==null)warningPanel=CreateBadge("Rush icon",out warningText);
            if(warningPanel==null)return;
            warningPanel.gameObject.SetActive(active);
            if(!active)return;
            float pulse=Mathf.Sin(phaseTime*10);
            FollowCar(warningPanel,pulse,phase==1);
            bool right=(toEnd?endPoint.x:startPoint.x)>body.position.x;
            warningText.text=phase==1?"!":right?"»":"«";
            warningText.color=phase==1?Color.Lerp(new Color32(255,190,70,255),new Color32(255,241,175,255),.5f+.5f*pulse):new Color32(255,116,90,255);
        }
        private void OnDestroy()
        {
            if(warningPanel!=null)Destroy(warningPanel.gameObject);
            if(departureBadge!=null)Destroy(departureBadge.gameObject);
        }
        private float TickRush()
        {
            if(!rushEnabled || game.IsTraining)return 1;
            if(phase==0 && game.Bicycle!=null && game.Bicycle.Busy)return 1;
            // Real gameplay seconds: slow affects movement, not the warning window.
            phaseTime+=Time.fixedDeltaTime;
            float duration=phase==0?cruiseDuration:phase==1?warningDuration:rushDuration;
            if(phaseTime>=Mathf.Max(.1f,duration))
            {
                phaseTime=0;phase=(phase+1)%3;
                if(phase==1)game.Audio?.Play(AudioCue.Warning);
            }
            return phase==2?Mathf.Max(1,rushMultiplier):1;
        }
        private bool toEnd = true;
        private float districtMultiplier = 1;
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            // Update the previous defaults retained by existing scene serialization.
            if(Mathf.Approximately(cruiseDuration,5f))cruiseDuration=3f;
            if(Mathf.Approximately(stopDuration,1.5f))stopDuration=1f;
        }
        public void ResetObstacle(float multiplier, float progress = 0, bool towardsEnd = true)
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            body.position = Vector2.Lerp(startPoint, endPoint, Mathf.Clamp01(progress));
            transform.position = body.position;
            toEnd = towardsEnd;
            districtMultiplier = multiplier;
            stopPhase=0;stopTime=0;stoppedThisLeg=false;
            if(departureBadge!=null)departureBadge.gameObject.SetActive(false);
            phase=0;phaseTime=0;if(warningPanel!=null)warningPanel.gameObject.SetActive(false);
        }
        private void FixedUpdate()
        {
            if (game == null || !game.IsPlaying)
            { return; }
            if(game.IsTraining && (!game.Tutorial.PracticeSlow || !rushEnabled))return;
            float factor=game.IsTraining?(game.IsSlowing?game.slowFactor:1):game.ObstacleFactor;
            if(stopAndGoEnabled && stopPhase!=0)
            {
                // The car remains stopped through the full warning; pause freezes this timer.
                stopTime+=Time.fixedDeltaTime;
                float wait=stopPhase==1?stopDuration:departureWarningDuration;
                if(stopTime>=Mathf.Max(.1f,wait))
                {
                    stopTime=0;
                    if(stopPhase==1){stopPhase=2;game.Audio?.Play(AudioCue.Warning);}
                    else stopPhase=0;
                }
                return;
            }
            float rush = TickRush();
            Vector2 target = toEnd ? endPoint : startPoint;
            Vector2 next = Vector2.MoveTowards(body.position, target,
                moveSpeed * normalSpeedMultiplier * districtMultiplier * rush * factor * Time.fixedDeltaTime);
            if(stopAndGoEnabled && !stoppedThisLeg)
            {
                Vector2 midpoint=(startPoint+endPoint)*.5f;
                // Stop only when this leg actually reaches/crosses the midpoint, even at high speed.
                if(Vector2.Dot(midpoint-body.position,midpoint-next)<=0)
                {
                    next=midpoint;stoppedThisLeg=true;stopPhase=1;stopTime=0;
                }
            }
            var art = GetComponent<SpriteRenderer>();
            if (art != null) art.flipX = target.x < body.position.x;
            body.MovePosition(next);
            if ((next - target).sqrMagnitude < 0.0001f) {toEnd = !toEnd;stoppedThisLeg=false;}
        }
        private void OnTriggerEnter2D(Collider2D other) { Hit(other); }
        private void OnTriggerStay2D(Collider2D other) { Hit(other); }
        private void Hit(Collider2D other)
        { if (stopAndGoEnabled && stopPhase!=0)return;
            if (game != null && other.GetComponent<CourierPlayer>() != null) game.Hit(); }
        private void OnDrawGizmosSelected()
        { Gizmos.color = Color.red; Gizmos.DrawLine(startPoint, endPoint); }
    }
}
