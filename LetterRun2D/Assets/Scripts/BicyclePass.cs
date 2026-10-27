using UnityEngine;
using UnityEngine.UI;
namespace LetterRun2D
{
    // One cyclist, one crossing. All timers advance only during the normal round.
    public class BicyclePass : MonoBehaviour
    {
        [Tooltip("Upper, middle, lower crossing heights; car roads are excluded.")]
        public float[] laneHeights = {3.3f, -.2f, -2.7f};
        public float speed=8f;
        private float laneY=-.2f;
        public float minimumInterval=2f, maximumInterval=4f;
        public float warningDuration=1f;
        public bool Busy => stage!=0;
        private DeliveryGame game;
        private int stage, direction;
        private float remaining, exitX;
        private Rigidbody2D body;
        private SpriteRenderer art;
        private BoxCollider2D hitbox;
        private RectTransform badge, canvas;
        private Text label;
        private RectTransform iconRect;
        private CanvasGroup badgeGroup;
        private Texture2D texture;
        private Sprite sprite;
        public static BicyclePass Attach(DeliveryGame owner)
        {
            var obj=new GameObject("Passing bicycle");obj.transform.SetParent(owner.transform,false);
            var component=obj.AddComponent<BicyclePass>();component.Initialize(owner);return component;
        }
        private void Initialize(DeliveryGame owner)
        {
            game=owner;
            body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;
            body.interpolation=RigidbodyInterpolation2D.Interpolate;
            hitbox=gameObject.AddComponent<BoxCollider2D>();hitbox.isTrigger=true;hitbox.size=new Vector2(.9f,.45f);
            art=gameObject.AddComponent<SpriteRenderer>();art.sprite=MakeSprite();
            canvas=(RectTransform)game.hintLabel.canvas.transform;
            badge=DeliveryFeedback.CreateRect("Bicycle edge warning",canvas,Vector2.zero,Vector2.zero,new Vector2(62,36));
            badgeGroup=badge.gameObject.AddComponent<CanvasGroup>();badgeGroup.blocksRaycasts=false;badgeGroup.interactable=false;
            iconRect=DeliveryFeedback.CreateRect("Bicycle icon",badge,new Vector2(.5f,.5f),new Vector2(-10,0),new Vector2(38,30));
            var icon=iconRect.gameObject.AddComponent<Image>();icon.sprite=art.sprite;icon.preserveAspect=true;icon.raycastTarget=false;
            var shadow=icon.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.8f);shadow.effectDistance=new Vector2(1,-1);
            label=DeliveryFeedback.CreateText("Direction",badge,23,new Color32(255,228,130,255));
            label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);
            label.rectTransform.sizeDelta=new Vector2(24,32);
            var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=new Color(0,0,0,.9f);outline.effectDistance=new Vector2(1,-1);
            ResetPass();
        }
        public void ResetPass()
        {
            stage=0;remaining=Random.Range(Mathf.Max(1,minimumInterval),Mathf.Max(minimumInterval,maximumInterval));
            if(art!=null)art.enabled=false;if(hitbox!=null)hitbox.enabled=false;
            if(badge!=null)badge.gameObject.SetActive(false);
        }
        private bool RushBusy()
        {
            foreach(var car in game.obstacles)if(car!=null && car.RushActive)return true;
            return false;
        }
        private void FixedUpdate()
        {
            if(game==null || !game.IsPlaying || game.IsTraining)return;
            float dt=Time.fixedDeltaTime;
            if(stage==0)
            {
                remaining-=dt;
                if(remaining<=0 && !RushBusy() && Camera.main!=null)
                {
                    direction=Random.value<.5f?1:-1;
                    laneY=laneHeights!=null && laneHeights.Length>0?laneHeights[Random.Range(0,laneHeights.Length)]:-.2f;
                    var cam=Camera.main;float half=cam.orthographicSize*cam.aspect;
                    body.position=new Vector2(cam.transform.position.x-direction*(half+1),laneY);
                    transform.position=body.position;
                    exitX=cam.transform.position.x+direction*(half+1);
                    stage=1;remaining=Mathf.Max(.1f,warningDuration);
                    art.flipX=direction<0;game.Audio?.Play(AudioCue.BicycleBell);
                }
                return;
            }
            if(stage==1)
            {
                remaining-=dt;
                if(remaining<=0){stage=2;art.enabled=true;hitbox.enabled=true;}
                return;
            }
            var next=body.position+Vector2.right*direction*Mathf.Max(.1f,speed)*game.ObstacleFactor*dt;
            // Sweep the hitbox as well as trigger contacts so fast crossings cannot skip the courier.
            foreach(var hit in Physics2D.BoxCastAll(body.position,hitbox.size,0,Vector2.right*direction,Vector2.Distance(body.position,next)))
                if(hit.collider!=null && hit.collider.GetComponent<CourierPlayer>()!=null)game.Hit();
            body.MovePosition(next);
            if(direction*(next.x-exitX)>=0)ResetPass();
        }
        private void LateUpdate()
        {
            if(game==null)return;
            bool playing=game.IsPlaying && !game.IsTraining;
            art.enabled=playing && stage==2;hitbox.enabled=playing && stage==2;
            art.sortingOrder=100-Mathf.RoundToInt(laneY*10);
            badge.gameObject.SetActive(playing && stage==1);
            if(stage!=1 || Camera.main==null)return;
            Vector2 point=DeliveryFeedback.Project(canvas,new Vector3(Camera.main.transform.position.x,laneY,0));
            var safe=Screen.safeArea;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas,new Vector2(direction>0?safe.xMin:safe.xMax,0),null,out Vector2 edge);
            float x=edge.x+canvas.pivot.x*canvas.rect.width+(direction>0?37:-37);
            badge.anchoredPosition=new Vector2(Mathf.Clamp(x,37,canvas.rect.width-37),Mathf.Clamp(point.y,24,canvas.rect.height-24));
            label.text=direction>0?"→":"←";
            label.rectTransform.anchoredPosition=new Vector2(direction*20,0);
            iconRect.anchoredPosition=new Vector2(-direction*10,0);
            iconRect.localScale=new Vector3(direction,1,1);
            float elapsed=Mathf.Max(0,warningDuration-remaining);
            badgeGroup.alpha=Mathf.Lerp(.65f,1,.5f+.5f*Mathf.Sin(elapsed*12));
            badge.localScale=Vector3.one*Mathf.Lerp(.95f,1.05f,.5f+.5f*Mathf.Sin(elapsed*12));
        }
        private void OnTriggerEnter2D(Collider2D other){Contact(other);}
        private void OnTriggerStay2D(Collider2D other){Contact(other);}
        private void Contact(Collider2D other)
        {if(stage==2 && other.GetComponent<CourierPlayer>()!=null)game.Hit();}
        private Sprite MakeSprite()
        {
            // Code-drawn pixel bicycle and rider; no external art dependency.
            texture=new Texture2D(40,32,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;
            var pixels=new Color[40*32];texture.SetPixels(pixels);
            Color tire=new Color32(28,36,49,255),frame=new Color32(71,218,221,255),skin=new Color32(255,204,151,255);
            for(int y=0;y<16;y++)for(int x=0;x<40;x++)
            {
                float a=Vector2.Distance(new Vector2(x,y),new Vector2(9,8));
                float b=Vector2.Distance(new Vector2(x,y),new Vector2(31,8));
                if((a>=5 && a<=7)||(b>=5 && b<=7))texture.SetPixel(x,y,tire);
            }
            Line(9,8,17,18,frame);Line(17,18,23,8,frame);Line(23,8,9,8,frame);
            Line(17,18,28,18,frame);Line(28,18,23,8,frame);Line(28,20,31,8,frame);Line(28,20,33,21,tire);
            Line(16,19,22,19,tire);Line(19,20,22,26,new Color32(255,144,57,255));
            Line(22,25,28,21,skin);Line(19,20,16,15,tire);Line(16,15,23,10,tire);
            for(int y=26;y<31;y++)for(int x=22;x<27;x++)texture.SetPixel(x,y,y>=29?frame:skin);
            texture.Apply();sprite=Sprite.Create(texture,new Rect(0,0,40,32),new Vector2(.5f,.25f),32);return sprite;
        }
        private void Line(int x0,int y0,int x1,int y1,Color color)
        {
            int steps=Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0));
            for(int i=0;i<=steps;i++)
            {int x=Mathf.RoundToInt(Mathf.Lerp(x0,x1,i/(float)steps));int y=Mathf.RoundToInt(Mathf.Lerp(y0,y1,i/(float)steps));texture.SetPixel(x,y,color);if(y+1<32)texture.SetPixel(x,y+1,color);}
        }
        private void OnDestroy()
        {if(badge!=null)Destroy(badge.gameObject);if(sprite!=null)Destroy(sprite);if(texture!=null)Destroy(texture);}
    }
}
