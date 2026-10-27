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
        private bool Occupied(int side)
        {
            var gate=blockers[side];
            foreach(var hit in Physics2D.OverlapBoxAll(gate.transform.position,gate.size+Vector2.one*.15f,0))
                if(hit.GetComponent<CourierPlayer>()!=null)return true;
            return false;
        }
        private void FixedUpdate()
        {
            if(game==null)return;
            if(game.IsTraining || game.State==RoundState.Menu || game.State==RoundState.Result)
            {foreach(var blocker in blockers)blocker.enabled=false;return;}
            if(!game.IsPlaying)return;
            blockers[closedSide].enabled=!Occupied(closedSide) || blockers[closedSide].enabled;
            blockers[1-closedSide].enabled=false;
            elapsed+=Time.fixedDeltaTime*game.ObstacleFactor;
            if(elapsed>=Mathf.Max(.1f,holdDuration)+Mathf.Max(.1f,warningDuration) && !Occupied(1-closedSide))
            {
                blockers[closedSide].enabled=false;closedSide=1-closedSide;
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
