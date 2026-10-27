using UnityEngine;
using UnityEngine.UI;
namespace LetterRun2D
{
    public class AbilityOverhead : MonoBehaviour
    {
        private DeliveryGame game;
        private RectTransform canvasRect, panel;
        private Camera worldCamera;
        private sealed class Indicator
        {
            public RectTransform root, fill;
            public CanvasGroup group;
            public Texture2D texture;
            public Sprite sprite;
            public Image icon;
        }
        private Indicator speed, shield;
        public static void Attach(DeliveryGame target)
        {
            var canvas=target.hintLabel.canvas;
            if(canvas==null || canvas.transform.Find("Ability Overhead")!=null)return;
            var obj=new GameObject("Ability Overhead",typeof(RectTransform),typeof(AbilityOverhead));
            obj.transform.SetParent(canvas.transform,false);
            obj.GetComponent<AbilityOverhead>().Initialize(target,canvas);
        }
        private void Initialize(DeliveryGame target,Canvas canvas)
        {
            game=target;canvasRect=(RectTransform)canvas.transform;panel=(RectTransform)transform;
            panel.anchorMin=panel.anchorMax=Vector2.zero;panel.pivot=new Vector2(.5f,0);
            speed=Create("Speed",new Color32(120,237,159,255),true);
            shield=Create("Shield",new Color32(122,201,255,255),false);
            worldCamera=Camera.main;
        }
        private Indicator Create(string name,Color accent,bool bolt)
        {
            var item=new Indicator();
            item.root=DeliveryFeedback.CreateRect(name,panel,new Vector2(.5f,0),Vector2.zero,new Vector2(34,33));
            item.root.pivot=new Vector2(.5f,0);
            item.group=item.root.gameObject.AddComponent<CanvasGroup>();item.group.blocksRaycasts=false;item.group.interactable=false;
            item.texture=new Texture2D(16,16,TextureFormat.RGBA32,false);item.texture.filterMode=FilterMode.Point;
            Vector2[] polygon=bolt?new[]{new Vector2(8,15),new Vector2(3,7),new Vector2(7,7),new Vector2(5,0),new Vector2(13,10),new Vector2(9,10),new Vector2(12,15)}:
                new[]{new Vector2(2,14),new Vector2(8,15),new Vector2(14,14),new Vector2(13,6),new Vector2(8,1),new Vector2(3,6)};
            for(int y=0;y<16;y++)for(int x=0;x<16;x++)
            {
                bool inside=false;var point=new Vector2(x+.5f,y+.5f);
                for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)
                {
                    var a=polygon[i];var b=polygon[j];
                    if((a.y>point.y)!=(b.y>point.y) && point.x<(b.x-a.x)*(point.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
                }
                item.texture.SetPixel(x,y,inside?accent:Color.clear);
            }
            item.texture.Apply();item.sprite=Sprite.Create(item.texture,new Rect(0,0,16,16),new Vector2(.5f,.5f),16);
            var iconRect=DeliveryFeedback.CreateRect("Ability icon",item.root,new Vector2(.5f,0),new Vector2(0,21),new Vector2(23,23));
            var icon=iconRect.gameObject.AddComponent<Image>();item.icon=icon;icon.sprite=item.sprite;icon.raycastTarget=false;
            var outline=icon.gameObject.AddComponent<Outline>();outline.effectColor=new Color(0,0,0,.85f);outline.effectDistance=new Vector2(1,-1);
            var track=DeliveryFeedback.CreateRect("Time track",item.root,new Vector2(.5f,0),new Vector2(0,5),new Vector2(30,5));
            var background=track.gameObject.AddComponent<Image>();background.color=new Color32(20,36,42,220);background.raycastTarget=false;
            item.fill=DeliveryFeedback.CreateRect("Remaining time",track,Vector2.zero,new Vector2(1,1),new Vector2(28,3));item.fill.pivot=Vector2.zero;
            var fill=item.fill.gameObject.AddComponent<Image>();fill.color=accent;fill.raycastTarget=false;
            item.root.gameObject.SetActive(false);return item;
        }
        private void Set(Indicator item,bool visible,float seconds,float duration,float x)
        {
            item.root.gameObject.SetActive(visible);if(!visible)return;
            item.root.anchoredPosition=new Vector2(x,0);
            item.fill.sizeDelta=new Vector2(28*Mathf.Clamp01(seconds/Mathf.Max(.1f,duration)),3);
            item.group.alpha=Mathf.Clamp01(seconds/.35f);
            float flash=item==shield && game.Feedback!=null?game.Feedback.ShieldFlashRemaining:0;
            item.icon.color=flash>0?Color.Lerp(Color.white,new Color(.5f,.85f,1),.5f+.5f*Mathf.Sin(flash*35)):Color.white;
            item.icon.rectTransform.localScale=Vector3.one*(flash>0?1+.25f*Mathf.Sin(Mathf.PI*flash/.45f):1);
        }
        private Vector2 ToCanvas(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,screen,null,out Vector2 local);
            return local+Vector2.Scale(canvasRect.pivot,canvasRect.rect.size);
        }
        private void LateUpdate()
        {
            if(game==null || speed==null || shield==null)return;
            if(worldCamera==null)worldCamera=Camera.main;
            bool visible=game.IsPlaying && game.player!=null && worldCamera!=null;
            bool a=visible && game.player.SpeedRemaining>0,b=visible && game.player.ShieldRemaining>0;
            if(!visible){speed.root.gameObject.SetActive(false);shield.root.gameObject.SetActive(false);return;}
            Set(speed,a,game.player.SpeedRemaining,game.player.speedDuration,b?-19:0);
            Set(shield,b,game.player.ShieldRemaining,game.player.shieldDuration,a?19:0);
            if(!a&&!b)return;
            float width=a&&b?72:34;panel.sizeDelta=new Vector2(width,33);
            Vector2 point=ToCanvas(worldCamera.WorldToScreenPoint(game.player.transform.position+Vector3.up*.8f));
            Vector2 low=ToCanvas(worldCamera.pixelRect.min),high=ToCanvas(worldCamera.pixelRect.max);
            panel.anchoredPosition=new Vector2(Mathf.Clamp(point.x,low.x+width/2+3,Mathf.Max(low.x+width/2+3,high.x-width/2-3)),
                Mathf.Clamp(point.y+5,low.y+3,Mathf.Max(low.y+3,high.y-36)));
        }
        private void OnDestroy()
        {
            foreach(var item in new[]{speed,shield})
                if(item!=null){if(item.sprite!=null)Destroy(item.sprite);if(item.texture!=null)Destroy(item.texture);}
        }
    }
}
