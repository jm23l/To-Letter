using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace LetterRun2D
{
    // Reuses existing buttons/listeners, including saved scenes. Decorative elements never intercept touch.
    public class PostalUI : MonoBehaviour
    {
        private static readonly Color Ink=new Color32(35,64,70,255);
        private static readonly Color Teal=new Color32(43,111,113,255);
        private static readonly Color Paper=new Color32(250,242,219,255);
        private static readonly Color Coral=new Color32(207,105,77,255);
        private readonly List<RectTransform> cards=new List<RectTransform>();
        private readonly List<Vector2> sizes=new List<Vector2>();
        public static void Apply(DeliveryGame game)
        {
            if(game.GetComponent<PostalUI>()!=null)return;
            game.gameObject.AddComponent<PostalUI>().Build(game);
        }
        private static void Box(RectTransform r,Vector2 position,Vector2 size)
        {
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;
        }
        private static Image Image(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
        {
            var r=DeliveryFeedback.CreateRect(name,parent,new Vector2(.5f,.5f),pos,size);
            var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;
        }
        private static void Edge(Graphic graphic)
        {
            var shadow=graphic.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.24f);shadow.effectDistance=new Vector2(0,-4);
            var outline=graphic.gameObject.AddComponent<Outline>();outline.effectColor=new Color32(27,63,65,255);outline.effectDistance=new Vector2(2,-2);
        }
        private RectTransform Card(GameObject menu,Vector2 size)
        {
            menu.GetComponent<Image>().color=new Color32(21,43,51,247);
            var card=Image("Postal card",menu.transform,Vector2.zero,size,Paper);Edge(card);
            card.transform.SetAsFirstSibling();cards.Add(card.rectTransform);sizes.Add(size);
            for(int row=0;row<2;row++)for(int i=0;i<12;i++)
                Image("Postal border",card.transform,new Vector2(-size.x/2+18+i*(size.x-36)/11,(row==0?1:-1)*(size.y/2-13)),new Vector2(20,5),i%2==0?Teal:Coral);
            return card.rectTransform;
        }
        private static void Place(Transform obj,Transform parent,Vector2 pos,Vector2 size)
        {obj.SetParent(parent,false);Box((RectTransform)obj,pos,size);}
        private static void TextStyle(Text text,int size,Color color)
        {
            text.fontSize=size;text.color=color;text.alignment=TextAnchor.MiddleCenter;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;text.raycastTarget=false;
        }
        private static Text Existing(GameObject panel,string name,Transform card,Vector2 pos,Vector2 size,int font)
        {
            var obj=panel.transform.Find(name);if(obj==null)return null;
            Place(obj,card,pos,size);var text=obj.GetComponent<Text>();TextStyle(text,font,Ink);return text;
        }
        private static Text Label(string message,RectTransform card,Vector2 pos,Vector2 size,int font,Color color)
        {
            var holder=DeliveryFeedback.CreateRect("Caption",card,new Vector2(.5f,.5f),pos,size);
            var text=DeliveryFeedback.CreateText("Text",holder,font,color);text.text=message;return text;
        }
        private static void ButtonStyle(Button button,Color color,Color textColor)
        {
            var image=button.GetComponent<Image>();image.color=color;Edge(image);
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;
            colors.pressedColor=new Color(.78f,.83f,.83f);colors.disabledColor=new Color(.6f,.6f,.6f,.65f);colors.fadeDuration=.08f;button.colors=colors;
            var text=button.GetComponentInChildren<Text>(true);if(text!=null)text.color=textColor;
        }
        private static void MenuButton(Button button,RectTransform card,float y,Color color)
        {
            Place(button.transform,card,new Vector2(0,y),new Vector2(324,60));ButtonStyle(button,color,Paper);
            var text=button.GetComponentInChildren<Text>(true);if(text!=null){Box(text.rectTransform,Vector2.zero,new Vector2(308,52));TextStyle(text,21,Paper);}
        }
        private static void SpriteIcon(string asset,RectTransform card,Vector2 pos,Vector2 size)
        {
            var image=Image(asset,card,pos,size,Color.white);image.sprite=Resources.Load<Sprite>("Sprites/"+asset);image.preserveAspect=true;
        }
        private void Build(DeliveryGame game)
        {
            var start=Card(game.menuPanel,new Vector2(420,670));
            Label("오늘의 배달",start,new Vector2(0,281),new Vector2(340,27),15,Teal);
            var title=Existing(game.menuPanel,"Title",start,new Vector2(0,225),new Vector2(360,76),52);if(title!=null){title.color=Teal;title.fontStyle=FontStyle.Bold;}
            SpriteIcon("courier_down",start,new Vector2(-72,133),new Vector2(66,86));
            SpriteIcon("letter",start,new Vector2(0,146),new Vector2(46,38));
            SpriteIcon("mailbox",start,new Vector2(76,133),new Vector2(75,90));
            Existing(game.menuPanel,"Description",start,new Vector2(0,55),new Vector2(366,53),17);
            Image("Rule divider",start,new Vector2(0,14),new Vector2(344,2),new Color32(204,190,160,255));
            Existing(game.menuPanel,"Rules",start,new Vector2(0,-28),new Vector2(376,64),18);
            MenuButton(game.startButton,start,-117,Teal);
            var instructions=Existing(game.menuPanel,"Instructions",start,new Vector2(0,-225),new Vector2(370,100),16);
            if(instructions!=null)instructions.text="스틱으로 이동 · 탭으로 편지 줍기\n꾹 눌러 시간 감속 · 우체통에서 자동 배달\n처음 시작하면 조작을 연습할 수 있어요";
            var result=Card(game.resultPanel,new Vector2(420,570));
            Label("배달 기록",result,new Vector2(0,225),new Vector2(340,30),18,Teal);
            SpriteIcon("letter",result,new Vector2(0,158),new Vector2(80,56));
            Place(game.resultLabel.transform,result,new Vector2(0,35),new Vector2(370,168));TextStyle(game.resultLabel,25,Ink);
            MenuButton(game.retryButton,result,-110,Teal);MenuButton(game.menuButton,result,-190,Coral);
            var pause=Card(game.pausePanel,new Vector2(380,330));
            Existing(game.pausePanel,"Paused label",pause,new Vector2(0,63),new Vector2(340,62),32);
            MenuButton(game.resumeButton,pause,-47,Teal);
            Label("잠깐 쉬어 가도 괜찮아요",pause,new Vector2(0,-112),new Vector2(340,25),14,Ink);
            ButtonStyle(game.collectButton,Coral,Paper);ButtonStyle(game.pauseButton,Teal,Paper);
            Edge(game.holdButton.GetComponent<Image>());
            var holdText=game.holdButton.GetComponentInChildren<Text>(true);if(holdText!=null)holdText.color=Paper;
            game.stick.GetComponent<Image>().color=new Color32(49,84,88,255);
            game.stick.handle.GetComponent<Image>().color=new Color32(236,212,164,255);
            var hud=game.timerLabel.transform.parent.GetComponent<Image>();if(hud!=null)hud.color=new Color32(28,60,66,255);
            var band=game.controlsPanel.transform.Find("Bottom band");if(band!=null)band.GetComponent<Image>().color=new Color32(21,43,51,250);
            Refresh();
        }
        private void Refresh()
        {
            for(int i=0;i<cards.Count;i++)
            {
                if(cards[i]==null)continue;
                var available=((RectTransform)cards[i].parent).rect.size;
                if(available.x<=0 || available.y<=0)continue;
                float scale=Mathf.Clamp(Mathf.Min((available.x-24)/sizes[i].x,(available.y-24)/sizes[i].y),.1f,1);
                cards[i].localScale=Vector3.one*scale;
            }
        }
        private void LateUpdate(){Refresh();}
    }
}
