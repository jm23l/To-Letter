using UnityEngine;
namespace LetterRun2D
{
    // Kept under its previous script name so existing imports keep their identity.
    // The mechanic is now a physical barrier, with no movement-input filtering.
    public class CentralConveyor : MonoBehaviour
    {
        public float gateY=-.65f,holdDuration=5f,warningDuration=1f;
        private DeliveryGame game;
        private int closedSide;
        private float elapsed;
        private BoxCollider2D[] blockers;
        private SpriteRenderer[] arms,lamps,posts;
        private Texture2D texture;
        private Sprite stripe;
        public static CentralConveyor Attach(DeliveryGame owner)
        {
            var obj=new GameObject("Central alternating barriers");obj.transform.SetParent(owner.transform,false);
            var gates=obj.AddComponent<CentralConveyor>();gates.Initialize(owner);return gates;
        }
        private SpriteRenderer Draw(string name,Vector2 position,Vector2 scale,Sprite sprite,int order)
        {
            var obj=new GameObject(name);obj.transform.SetParent(transform,false);obj.transform.localPosition=position;obj.transform.localScale=scale;
            var art=obj.AddComponent<SpriteRenderer>();art.sprite=sprite;art.sortingOrder=order;return art;
        }
        private void Initialize(DeliveryGame owner)
        {
            game=owner;arms=new SpriteRenderer[2];lamps=new SpriteRenderer[2];posts=new SpriteRenderer[2];blockers=new BoxCollider2D[2];
            texture=new Texture2D(64,4,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;
            for(int y=0;y<4;y++)for(int x=0;x<64;x++)texture.SetPixel(x,y,(x+y)/5%2==0?new Color32(245,239,211,255):new Color32(215,93,71,255));
            texture.Apply();stripe=Sprite.Create(texture,new Rect(0,0,64,4),new Vector2(.5f,.5f),64);
            var square=Resources.Load<Sprite>("Sprites/square");
            for(int i=0;i<2;i++)
            {
                float sign=i==0?-1:1;
                arms[i]=Draw("Barrier arm",new Vector2(sign*1.875f,gateY),new Vector2(3.75f,2.88f),stripe,107);
                // Collider uses unscaled world units on a separate transform.
                var hit=new GameObject("Solid barrier");hit.transform.SetParent(transform,false);hit.transform.localPosition=new Vector3(sign*1.875f,gateY,0);
                blockers[i]=hit.AddComponent<BoxCollider2D>();blockers[i].size=new Vector2(3.75f,.18f);blockers[i].isTrigger=false;
                posts[i]=Draw("Barrier post",new Vector2(sign*3.62f,gateY),new Vector2(.25f,.42f),square,108);posts[i].color=new Color32(53,72,81,255);
                lamps[i]=Draw("Barrier lamp",new Vector2(sign*3.62f,gateY+.12f),new Vector2(.13f,.13f),square,109);
            }
            ResetCycle();
        }
        public void ResetCycle()
        {
            closedSide=0;elapsed=0;
            if(blockers!=null)foreach(var blocker in blockers)blocker.enabled=false;
        }
        private void ClearClosingArea(int side)
        {
            var player=game.player;
            var body=player.GetComponent<Rigidbody2D>();
            var circle=player.GetComponent<CircleCollider2D>();
            Vector2 center=circle.bounds.center;
            Vector2 extent=circle.bounds.extents;
            Vector2 gate=blockers[side].transform.position;
            Vector2 half=blockers[side].size*.5f;
            if(Mathf.Abs(center.x-gate.x)>half.x+extent.x || Mathf.Abs(center.y-gate.y)>half.y+extent.y+.03f)return;
            // Close on schedule; gently move to the nearer side instead of trapping the player.
            float sign=center.y>=gate.y?1f:-1f;
            float destination=gate.y+sign*(half.y+extent.y+.06f);
            Vector2 position=body.position+Vector2.up*(destination-center.y);
            player.StopMoving();body.position=position;player.transform.position=position;
            Physics2D.SyncTransforms();
        }
        public bool BlocksPickup(Vector2 from,Vector2 to)
        {
            if(game==null || game.IsTraining || blockers==null)return false;
            foreach(var blocker in blockers)
            {
                if(!blocker.enabled)continue;
                Vector2 center=blocker.transform.position,half=blocker.size*.5f;
                Vector2 delta=to-from;float enter=0,exit=1;
                if(ClipAxis(from.x,delta.x,center.x-half.x,center.x+half.x,ref enter,ref exit) &&
                   ClipAxis(from.y,delta.y,center.y-half.y,center.y+half.y,ref enter,ref exit))return true;
            }
            return false;
        }
        private static bool ClipAxis(float start,float delta,float min,float max,ref float enter,ref float exit)
        {
            if(Mathf.Abs(delta)<.00001f)return start>=min && start<=max;
            float a=(min-start)/delta,b=(max-start)/delta;
            if(a>b){float swap=a;a=b;b=swap;}
            enter=Mathf.Max(enter,a);exit=Mathf.Min(exit,b);return enter<=exit;
        }
        private void FixedUpdate()
        {
            if(game==null)return;
            if(game.IsTraining || game.State==RoundState.Menu || game.State==RoundState.Result)
            {foreach(var blocker in blockers)blocker.enabled=false;return;}
            if(!game.IsPlaying)return;
            if(!blockers[closedSide].enabled)ClearClosingArea(closedSide);
            blockers[closedSide].enabled=true;
            blockers[1-closedSide].enabled=false;
            elapsed+=Time.fixedDeltaTime*game.ObstacleFactor;
            if(elapsed>=Mathf.Max(.1f,holdDuration)+Mathf.Max(.1f,warningDuration))
            {
                blockers[closedSide].enabled=false;closedSide=1-closedSide;
                ClearClosingArea(closedSide);
                blockers[closedSide].enabled=true;elapsed=0;
            }
        }
        private void LateUpdate()
        {
            if(game==null)return;
            bool show=!game.IsTraining && (game.State==RoundState.Playing || game.State==RoundState.Paused);
            bool warning=elapsed>=Mathf.Max(.1f,holdDuration);
            for(int i=0;i<2;i++)
            {
                bool closed=i==closedSide;
                arms[i].enabled=show && (closed||warning);posts[i].enabled=lamps[i].enabled=show;
                arms[i].color=closed?Color.white:new Color(1,.85f,.4f,.2f+.25f*(.5f+.5f*Mathf.Sin(elapsed*12)));
                lamps[i].color=warning?Color.Lerp(new Color(1,.55f,.1f),Color.yellow,.5f+.5f*Mathf.Sin(elapsed*12)):
                    closed?new Color(1,.25f,.2f):new Color(.3f,.9f,.6f);
            }
        }
        private void OnDestroy(){if(stripe!=null)Destroy(stripe);if(texture!=null)Destroy(texture);}
    }
}
