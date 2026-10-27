using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace LetterRun2D
{
    // All labels and animation images ignore raycasts, so stick + tap + hold still work together.
    public class DeliveryFeedback : MonoBehaviour
    {
        private sealed class Effect
        {
            public RectTransform rect;
            public Graphic graphic;
            public Vector3 from, to;
            public Transform follow;
            public float age, duration, delay;
            public bool flying;
            public Color tint;
        }
        private readonly List<Effect> effects=new List<Effect>();
        private DeliveryGame game;
        private RectTransform canvasRect, root;
        public void Initialize(DeliveryGame owner)
        {
            game=owner;canvasRect=(RectTransform)game.hintLabel.canvas.transform;
            root=CreateRect("Pickup and delivery effects",canvasRect,Vector2.zero,Vector2.zero,Vector2.zero);
            root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            // Menus/results remain on top. During play these effects overlay the world.
            root.SetAsFirstSibling();
        }
        public void Collected(LetterKind kind,Vector3 origin,Transform player)
        {
            Color color=kind==LetterKind.Speed?new Color32(120,237,159,255):kind==LetterKind.Shield?new Color32(122,201,255,255):new Color32(255,220,155,255);
            string title=kind==LetterKind.Speed?"편지 +1 · 가속!":kind==LetterKind.Shield?"편지 +1 · 보호!":"편지 +1";
            Popup(title,player.position+Vector3.up*.55f,color);
            Fly(origin,player.position,player,kind,0);
            game.Audio?.Play(kind==LetterKind.Speed?AudioCue.Speed:kind==LetterKind.Shield?AudioCue.Shield:AudioCue.Pickup);
        }
        public void Hit(float penalty, Vector3 position)
        {
            Popup(penalty>0?$"충돌! −{penalty:0.#}초":"충돌! 잠깐 기절",position+Vector3.up*.7f,new Color32(255,100,100,255));
        }
        public void Delivered(int amount,Vector3 player,Vector3 mailbox)
        {
            Popup($"{amount}개 배달 완료!",mailbox+Vector3.up*.65f,new Color32(120,237,159,255));
            for(int i=0;i<Mathf.Min(amount,8);i++)Fly(player,mailbox,null,LetterKind.Normal,i*.09f);
            game.Audio?.Play(AudioCue.Delivery);
        }
        private void Popup(string message,Vector3 world,Color color)
        {
            var rect=CreateRect("Feedback popup",root,Vector2.zero,Vector2.zero,new Vector2(230,42));
            var label=CreateText("Text",rect,22,color);label.text=message;
            var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=new Color(0,0,0,.95f);outline.effectDistance=new Vector2(1,-1);
            effects.Add(new Effect{rect=rect,graphic=label,from=world,to=world,duration=1.15f,tint=color});
        }
        private void Fly(Vector3 from,Vector3 to,Transform follow,LetterKind kind,float delay)
        {
            var rect=CreateRect("Flying letter",root,Vector2.zero,Vector2.zero,new Vector2(34,34));
            var img=rect.gameObject.AddComponent<Image>();img.raycastTarget=false;img.preserveAspect=true;
            img.sprite=Resources.Load<Sprite>("Sprites/"+(kind==LetterKind.Speed?"letter_speed":kind==LetterKind.Shield?"letter_shield":"letter"));
            effects.Add(new Effect{rect=rect,graphic=img,from=from,to=to,follow=follow,duration=.5f,delay=delay,flying=true,tint=Color.white});
        }
        private void LateUpdate()
        {
            if(game==null)return;
            bool show=game.State==RoundState.Playing;
            root.gameObject.SetActive(show);
            if(!show)return;
            for(int i=effects.Count-1;i>=0;i--)
            {
                var e=effects[i];e.age+=Time.unscaledDeltaTime;
                float elapsed=e.age-e.delay;
                e.graphic.enabled=elapsed>=0;
                if(elapsed<0)continue;
                float t=Mathf.Clamp01(elapsed/e.duration);
                Vector2 point;
                if(e.flying)
                {
                    Vector3 destination=e.follow!=null?e.follow.position:e.to;
                    point=Vector2.Lerp(Project(canvasRect,e.from),Project(canvasRect,destination),t);
                    point.y+=Mathf.Sin(t*Mathf.PI)*20;
                    e.rect.localScale=Vector3.one*Mathf.Lerp(1,.55f,t);
                }
                else point=Project(canvasRect,e.from)+new Vector2(0,28*t);
                e.rect.anchoredPosition=new Vector2(Mathf.Clamp(point.x,75,Mathf.Max(75,canvasRect.rect.width-75)),point.y);
                Color color=e.tint;color.a=1-Mathf.InverseLerp(.65f,1,t);e.graphic.color=color;
                if(t>=1){Destroy(e.rect.gameObject);effects.RemoveAt(i);}
            }
        }
        public void Clear()
        {
            foreach(var e in effects)if(e.rect!=null)Destroy(e.rect.gameObject);
            effects.Clear();game.Audio?.Stop();
        }
        public static Vector2 Project(RectTransform canvas,Vector3 position)
        {
            var camera=Camera.main;if(camera==null)return Vector2.zero;
            Vector3 screen=camera.WorldToScreenPoint(position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas,screen,null,out Vector2 local);
            return local+Vector2.Scale(canvas.pivot,canvas.rect.size);
        }
        public static RectTransform CreateRect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var obj=new GameObject(name,typeof(RectTransform));var rt=(RectTransform)obj.transform;
            rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=anchor;rt.anchoredPosition=position;rt.sizeDelta=size;return rt;
        }
        public static Text CreateText(string name,RectTransform parent,int size,Color color)
        {
            var rt=CreateRect(name,parent,Vector2.zero,Vector2.zero,Vector2.zero);rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
            var text=rt.gameObject.AddComponent<Text>();text.font=Resources.Load<Font>("Fonts/CourierKorean");text.fontSize=size;text.color=color;
            text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;return text;
        }
    }
}
