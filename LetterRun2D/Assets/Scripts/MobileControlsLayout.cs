using UnityEngine;
using UnityEngine.UI;

namespace LetterRun2D
{
    public class MobileControlsLayout : MonoBehaviour
    {
        private DeliveryGame game;
        private Text status;
        private Image holdImage;
        public static void ApplyLayout(DeliveryGame owner)
        {
            if(owner==null || owner.holdButton==null || owner.energyFill==null)return;
            var layout=owner.GetComponent<MobileControlsLayout>();
            if(layout==null)layout=owner.gameObject.AddComponent<MobileControlsLayout>();
            layout.game=owner;
            SetBox((RectTransform)owner.holdButton.transform,new Vector2(.5f,0),new Vector2(0,94),new Vector2(112,76));
            var label=owner.holdButton.GetComponentInChildren<Text>(true);
            if(label!=null)
            {
                SetBox(label.rectTransform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(108,72));
                label.text="꾹 눌러\n시간 감속";label.fontSize=16;label.raycastTarget=false;
            }
            var hud=owner.timerLabel.transform.parent;
            SetBox((RectTransform)owner.pauseButton.transform,new Vector2(.5f,.5f),new Vector2(165,17),new Vector2(90,40));
            var pauseLabel=owner.pauseButton.GetComponentInChildren<Text>(true);
            if(pauseLabel!=null)SetBox(pauseLabel.rectTransform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(86,36));
            var energy=(RectTransform)owner.energyFill.transform.parent;
            energy.SetParent(hud,false);
            SetBox(energy,new Vector2(.5f,.5f),new Vector2(155,-35),new Vector2(120,6));
            var existing=hud.Find("Slow status");
            if(existing!=null)layout.status=existing.GetComponent<Text>();
            if(layout.status==null)
            {
                var obj=new GameObject("Slow status",typeof(RectTransform),typeof(Text));obj.transform.SetParent(hud,false);
                layout.status=obj.GetComponent<Text>();
            }
            SetBox(layout.status.rectTransform,new Vector2(.5f,.5f),new Vector2(155,-19),new Vector2(130,20));
            layout.status.font=Resources.Load<Font>("Fonts/CourierKorean");layout.status.fontSize=12;
            layout.status.alignment=TextAnchor.MiddleCenter;layout.status.raycastTarget=false;
            layout.holdImage=owner.holdButton.GetComponent<Image>();
            layout.Refresh();
        }
        private static void SetBox(RectTransform rect,Vector2 anchor,Vector2 position,Vector2 size)
        {
            rect.anchorMin=rect.anchorMax=anchor;rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
        }
        private void LateUpdate(){Refresh();}
        private void Refresh()
        {
            if(game==null || status==null)return;
            int percent=Mathf.RoundToInt(100*game.Energy/Mathf.Max(.1f,game.maximumEnergy));
            status.text=game.IsTraining?(game.IsSlowing?"감속 연습 중":"연습 중"):game.IsSlowing?$"감속 중 {percent}%":$"감속 {percent}%";
            status.color=game.IsSlowing?new Color32(255,220,155,255):new Color32(115,217,231,255);
            if(holdImage!=null)holdImage.color=game.IsSlowing?new Color32(54,164,180,255):new Color32(72,116,151,255);
            if(game.energyFill!=null)game.energyFill.color=status.color;
        }
    }
}
