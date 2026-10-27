using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace LetterRun2D
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake() { BuildScene(); }
        private static readonly Color Navy = new Color32(16,27,45,255);
        private static readonly Color Cyan = new Color32(115,217,231,255);
        private static readonly Color Orange = new Color32(255,189,115,255);
        private static Font font;
        public static GameObject BuildScene()
        {
            font = Resources.Load<Font>("Fonts/CourierKorean");
            var root = new GameObject("LetterRun2D");
            root.SetActive(false);
            var game = root.AddComponent<DeliveryGame>();
            root.AddComponent<RoundVariation>();
            root.AddComponent<GameAudio>();
            var cameraObject = new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(CameraFrame2D));
            cameraObject.transform.SetParent(root.transform);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0,0,-10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Navy;
            camera.nearClipPlane=0.1f;camera.farClipPlane=30;
            camera.allowMSAA=false;
            var world = new GameObject("2D Town").transform;world.SetParent(root.transform,false);
            BuildMap(world);
            var playerObject=new GameObject("Player",typeof(Rigidbody2D),typeof(CircleCollider2D));
            playerObject.transform.SetParent(world,false);
            playerObject.transform.position=new Vector3(0,-4.2f,0);
            var body=playerObject.GetComponent<Rigidbody2D>();
            body.gravityScale=0;body.constraints=RigidbodyConstraints2D.FreezeRotation;
            body.interpolation=RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            playerObject.GetComponent<CircleCollider2D>().radius=0.2f;
            var model=Sprite("Courier",playerObject.transform,"courier_down",new Vector2(0,0.2f),Vector2.one,Color.white,10);
            var player=playerObject.AddComponent<CourierPlayer>();player.visual=model.transform;player.game=game;game.player=player;
            var animation=playerObject.AddComponent<CourierVisual2D>();animation.player=player;animation.art=model;
            Vector2[] points={new Vector2(-2.7f,4.1f),new Vector2(2.7f,4.1f),new Vector2(-2.8f,0.1f),new Vector2(2.8f,0.1f),new Vector2(-2.8f,-3.6f),new Vector2(2.8f,-3.6f),new Vector2(-0.9f,3.1f),new Vector2(0.9f,3.1f)};
            game.letters=new LetterItem[points.Length];
            for(int i=0;i<points.Length;i++)
            {
                var kind=i==6?LetterKind.Speed:i==7?LetterKind.Shield:LetterKind.Normal;
                var letter=Sprite("Letter "+(i+1)+" "+kind,world,kind==LetterKind.Speed?"letter_speed":kind==LetterKind.Shield?"letter_shield":"letter",points[i],Vector2.one,Color.white,5);
                game.letters[i]=letter.gameObject.AddComponent<LetterItem>();game.letters[i].kind=kind;
            }
            var post=Sprite("Mailbox",world,"mailbox",new Vector2(0,-3.4f),new Vector2(1.25f,1.25f),Color.white,134);
            var box=post.gameObject.AddComponent<BoxCollider2D>();box.isTrigger=true;box.size=new Vector2(0.95f,0.95f);
            post.gameObject.AddComponent<Mailbox>().game=game;
            game.obstacles=new MovingObstacle[2];
            for(int i=0;i<2;i++)
            {
                float y=i==0?2.0f:-1.5f;
                var car=Sprite("Moving car "+(i+1),world,i==0?"car_red":"car_yellow",new Vector2(i==0?-3.2f:3.2f,y),Vector2.one,Color.white,100-Mathf.RoundToInt(y*10));
                var rb=car.gameObject.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;rb.interpolation=RigidbodyInterpolation2D.Interpolate;
                var collider=car.gameObject.AddComponent<BoxCollider2D>();collider.isTrigger=true;collider.size=new Vector2(1.1f,0.56f);
                var mover=car.gameObject.AddComponent<MovingObstacle>();mover.game=game;mover.startPoint=car.transform.position;mover.endPoint=new Vector2(-mover.startPoint.x,y);mover.moveSpeed=i==0?1.5f:1.8f;game.obstacles[i]=mover;
            }
            for(int i=0;i<2;i++)
            {
                var tile=Sprite("Timed hazard "+(i+1),world,"hazard_safe",new Vector2(i==0?-1.5f:1.5f,0.8f),Vector2.one,Color.white,2);
                var hitbox=tile.gameObject.AddComponent<BoxCollider2D>();hitbox.isTrigger=true;hitbox.size=new Vector2(0.9f,0.9f);
                var hazard=tile.gameObject.AddComponent<TimedHazard>();hazard.game=game;hazard.initialDelay=i*1.2f;
            }
            var canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(root.transform);
            canvasObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(480, 854);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            var safe = Rect("Safe Area", canvasObj.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            safe.gameObject.AddComponent<SafeArea>();
            var hud = Panel("HUD", safe, new Color32(26, 48, 67, 255));
            AnchorBox(hud.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -58), new Vector2(448, 88));
            game.timerLabel = Label("Timer", hud.transform, "01:00", new Vector2(-153, 12), new Vector2(115, 42), 29);
            game.bagLabel = Label("Bag", hud.transform, "가방 0/3", new Vector2(0, 12), new Vector2(150, 42), 22);
            game.deliveryLabel = Label("Delivered", hud.transform, "배달 0/10", new Vector2(-69, -24), new Vector2(290, 25), 15);
            game.pauseButton = Button("Pause", hud.transform, "일시정지", new Vector2(165, 2), new Vector2(90, 47), new Color32(66, 91, 113, 255), 14);

            var controls = Rect("Controls", safe, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            game.controlsPanel = controls.gameObject;
            var bottom = Panel("Bottom band", controls, new Color32(16, 27, 45, 245));
            AnchorBox(bottom.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(480, 180));
            bottom.raycastTarget = false;
            var baseImage = Panel("Virtual stick", controls, new Color32(54, 88, 107, 255));
            baseImage.sprite = Resources.Load<Sprite>("Sprites/circle");
            AnchorBox(baseImage.rectTransform, Vector2.zero, new Vector2(90, 88), new Vector2(126, 126));
            var handle = Panel("Handle", baseImage.transform, Cyan);
            handle.sprite = baseImage.sprite; handle.raycastTarget = false;
            AnchorBox(handle.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(51, 51));
            game.stick = baseImage.gameObject.AddComponent<MobileStick>();
            game.stick.handle = handle.rectTransform;
            player.stick = game.stick;
            var moveLabel = Label("Move label", controls, "이동", new Vector2(0, 0), new Vector2(120, 24), 14);
            AnchorBox(moveLabel.rectTransform, Vector2.zero, new Vector2(90, 16), new Vector2(120, 24));
            game.collectButton = Button("Collect", controls, "편지 줍기", Vector2.zero, new Vector2(106, 76), Orange, 18);
            AnchorBox((RectTransform)game.collectButton.transform, new Vector2(1, 0), new Vector2(-76, 64), new Vector2(106, 76));
            var slowImage = Panel("Time hold", controls, new Color32(90, 143, 178, 255));
            AnchorBox(slowImage.rectTransform, new Vector2(1, 0), new Vector2(-76, 159), new Vector2(106, 60));
            Label("Slow label", slowImage.transform, "꾹 눌러\n시간 감속", Vector2.zero, new Vector2(102, 56), 16);
            game.holdButton = slowImage.gameObject.AddComponent<TimeHoldButton>();
            var energyBack = Panel("Energy background", controls, new Color32(50, 75, 94, 255));
            AnchorBox(energyBack.rectTransform, new Vector2(1, 0), new Vector2(-76, 119), new Vector2(106, 7));
            energyBack.raycastTarget = false;
            game.energyFill = Panel("Energy fill", energyBack.transform, Cyan);
            game.energyFill.sprite = Resources.Load<Sprite>("Sprites/square");
            game.energyFill.type = Image.Type.Filled;
            game.energyFill.fillMethod = Image.FillMethod.Horizontal;
            game.energyFill.fillOrigin = 0;
            game.energyFill.raycastTarget = false;
            game.hintLabel = Label("Hint", safe, "", Vector2.zero, new Vector2(450, 48), 14);
            AnchorBox(game.hintLabel.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 225), new Vector2(450, 48));

            var menu = Panel("Start menu", safe, new Color32(16, 27, 45, 252));
            game.menuPanel = menu.gameObject;
            Label("Title", menu.transform, "레터런", new Vector2(0, 245), new Vector2(430, 140), 46, Cyan);
            Label("Description", menu.transform, "편지를 모아 장애물을 피해\n시간 안에 우체통으로 배달하세요.", new Vector2(0, 120), new Vector2(420, 75), 17);
            Label("Rules", menu.transform, "제한 시간 60초 · 총 12개 중 10개 배달\n능력 편지도 포함 · 가방은 최대 3개", new Vector2(0, 33), new Vector2(400, 60), 20, Orange);
            game.startButton = Button("Start", menu.transform, "게임 시작", new Vector2(0, -95), new Vector2(370, 72), Cyan, 20);
            Label("Instructions", menu.transform, "이동: 왼쪽 가상 스틱\n수집: 편지 줍기 버튼 탭 · 감속: 시간 감속 버튼 홀드\n초록 편지: 가속 · 파란 편지: 보호\n바닥: 회색 안전 → 노랑 예고 → 빨강 위험\n충돌하면 제자리 스턴 + 시간 3초 감소", new Vector2(0, -280), new Vector2(420, 170), 15);
            var result = Panel("Result", safe, new Color32(16, 27, 45, 250));
            game.resultPanel = result.gameObject;
            game.resultLabel = Label("Result text", result.transform, "배달 결과", new Vector2(0, 115), new Vector2(430, 210), 26, Cyan);
            game.retryButton = Button("Retry", result.transform, "다시 하기", new Vector2(0, -60), new Vector2(320, 65), Cyan, 22);
            game.menuButton = Button("Menu", result.transform, "처음 화면", new Vector2(0, -150), new Vector2(320, 65), Orange, 22);
            result.gameObject.SetActive(false);
            var pause = Panel("Pause panel", safe, new Color32(16, 27, 45, 248));
            game.pausePanel = pause.gameObject;
            Label("Paused label", pause.transform, "일시정지", new Vector2(0, 80), new Vector2(420, 70), 38, Cyan);
            game.resumeButton = Button("Resume", pause.transform, "계속하기", new Vector2(0, -35), new Vector2(320, 65), Cyan, 23);
            pause.gameObject.SetActive(false);
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(root.transform);
            MobileControlsLayout.ApplyLayout(game);
            camera.GetComponent<CameraFrame2D>().Refresh();
            root.SetActive(true);
            return root;
        }
        private static SpriteRenderer Sprite(string name,Transform parent,string resource,Vector2 position,Vector2 scale,Color color,int order)
        {
            var obj=new GameObject(name,typeof(SpriteRenderer));obj.transform.SetParent(parent,false);
            obj.transform.localPosition=position;obj.transform.localScale=new Vector3(scale.x,scale.y,1);
            var sr=obj.GetComponent<SpriteRenderer>();sr.sprite=Resources.Load<Sprite>("Sprites/"+resource);sr.color=color;sr.sortingOrder=order;return sr;
        }
        private static void BuildMap(Transform world)
        {
            Sprite("Grass",world,"square",Vector2.zero,new Vector2(9,12),new Color32(80,130,104,255),-20);
            Sprite("Paving",world,"square",Vector2.zero,new Vector2(7.6f,10.4f),new Color32(216,209,176,255),-15);
            for(int x=-3;x<=3;x++)for(int y=-5;y<=5;y++)
                Sprite("Paving tile",world,"square",new Vector2(x,y),new Vector2(0.96f,0.96f),(x+y)%2==0?new Color32(223,216,188,255):new Color32(210,203,176,255),-14);
            foreach(float y in new[]{2f,-1.5f})
            {
                Sprite("Road",world,"square",new Vector2(0,y),new Vector2(7.5f,1.05f),new Color32(56,72,88,255),-10);
                for(int x=-3;x<=3;x++)Sprite("Road dash",world,"square",new Vector2(x,y),new Vector2(0.4f,0.05f),Orange,-9);
                for(int k=-2;k<=2;k++)Sprite("Crosswalk",world,"square",new Vector2(-2f,y+k*0.18f),new Vector2(0.65f,0.08f),Color.white,-8);
            }
            foreach(float x in new[]{-3.9f,3.9f})
            {
                var wall=Sprite("Side fence",world,"square",new Vector2(x,0),new Vector2(0.18f,10.6f),new Color32(130,104,82,255),0);
                wall.gameObject.AddComponent<BoxCollider2D>();
                for(int y=-4;y<=4;y+=2)Sprite("Tree",world,"tree",new Vector2(x+(x<0?-0.4f:0.4f),y),Vector2.one,Color.white,0);
            }
            foreach(float y in new[]{-5.3f,5.3f})
            {
                var wall=Sprite("End fence",world,"square",new Vector2(0,y),new Vector2(8,0.18f),new Color32(130,104,82,255),0);wall.gameObject.AddComponent<BoxCollider2D>();
            }
            Sprite("Post office",world,"building",new Vector2(0,4.7f),Vector2.one,Color.white,0);
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)obj.transform;
            rt.SetParent(parent, false); rt.anchorMin = min; rt.anchorMax = max;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }
        private static void AnchorBox(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        { rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size; }
        private static Image Panel(string name, Transform parent, Color color)
        {
            var rt = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rt.gameObject.AddComponent<Image>(); image.color = color; return image;
        }
        private static Text Label(string name, Transform parent, string value, Vector2 pos, Vector2 size, int fontSize, Color? color = null)
        {
            var rt = Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            var label = rt.gameObject.AddComponent<Text>();
            label.font = font; label.text = value; label.fontSize = fontSize;
            label.color = color ?? Color.white; label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false; label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }
        private static Button Button(string name, Transform parent, string value, Vector2 pos, Vector2 size, Color color, int fontSize)
        {
            var image = Panel(name, parent, color);
            AnchorBox(image.rectTransform, new Vector2(0.5f, 0.5f), pos, size);
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Label(name + " text", image.transform, value, Vector2.zero, size - new Vector2(8, 4), fontSize, Navy);
            return button;
        }
    }
}
