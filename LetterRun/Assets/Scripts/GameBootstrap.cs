using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace TimeCourier
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake() { BuildScene(); }
        private static readonly Color Navy = new Color32(16, 27, 45, 255);
        private static readonly Color Cyan = new Color32(115, 217, 231, 255);
        private static readonly Color Orange = new Color32(255, 189, 115, 255);
        private static Font font;

        // Also called by the Editor installer to create an editable scene.
        public static GameObject BuildScene()
        {
            font = Resources.Load<Font>("Fonts/CourierKorean");
            var root = new GameObject("TimeCourier");
            root.SetActive(false);
            var game = root.AddComponent<DeliveryGame>();
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PortraitCamera), typeof(PixelCamera));
            cameraObject.transform.SetParent(root.transform);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(2.5f, 11, -10);
            cameraObject.transform.LookAt(new Vector3(0, 0, 0.4f));
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 5.8f;
            camera.backgroundColor = new Color32(151, 186, 199, 255);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.nearClipPlane = 0.1f; camera.farClipPlane = 100;
            camera.allowMSAA = false;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.65f, 0.72f, 0.80f);
            var sunObject = new GameObject("Sun", typeof(Light));
            sunObject.transform.SetParent(root.transform);
            sunObject.transform.rotation = Quaternion.Euler(48, -35, 0);
            var sun = sunObject.GetComponent<Light>();
            sun.type = LightType.Directional; sun.color = new Color(1, 0.93f, 0.8f);
            sun.intensity = 1.1f; sun.shadows = LightShadows.Hard;
            QualitySettings.shadowDistance = 45;
            QualitySettings.shadows = ShadowQuality.All;

            var world = new GameObject("Voxel City").transform;
            world.SetParent(root.transform);
            Cube("City foundation", world, new Vector3(0, -0.24f, 0.45f), new Vector3(10, 0.45f, 10), new Color32(68, 100, 87, 255));
            Cube("Walkable plaza", world, new Vector3(0, -0.03f, 0.45f), new Vector3(5.6f, 0.08f, 7.4f), new Color32(196, 187, 160, 255));
            for (int row = -3; row <= 4; row++)
                for (int col = -2; col <= 2; col++)
                    Cube("Paving tile", world, new Vector3(col, 0.012f, row), new Vector3(0.94f, 0.012f, 0.94f),
                        (row + col) % 2 == 0 ? new Color32(204, 196, 171, 255) : new Color32(192, 184, 162, 255));
            for (int i = 0; i < 2; i++)
            {
                float z = i == 0 ? 2.35f : 0.2f;
                Cube("Road " + i, world, new Vector3(0, 0.035f, z), new Vector3(5.5f, 0.035f, 0.85f), new Color32(61, 72, 85, 255));
                for (int j = -2; j <= 2; j++)
                    Cube("Road marking", world, new Vector3(j, 0.058f, z), new Vector3(0.42f, 0.01f, 0.045f), new Color32(241, 212, 137, 255));
                for (int j = -2; j <= 2; j++)
                    Cube("Crosswalk", world, new Vector3(-1.55f, 0.06f, z + j * 0.15f), new Vector3(0.55f, 0.01f, 0.075f), Color.white);
            }
            Boundary(world, new Vector3(-2.82f, 0.3f, 0.45f), new Vector3(0.16f, 0.6f, 7.55f));
            Boundary(world, new Vector3(2.82f, 0.3f, 0.45f), new Vector3(0.16f, 0.6f, 7.55f));
            Boundary(world, new Vector3(0, 0.3f, 4.18f), new Vector3(5.8f, 0.6f, 0.16f));
            Boundary(world, new Vector3(0, 0.3f, -3.27f), new Vector3(5.8f, 0.6f, 0.16f));
            Building(world, new Vector3(-3.7f, 0, 3), new Color32(225, 161, 112, 255), 2.4f);
            Building(world, new Vector3(3.7f, 0, 3.2f), new Color32(129, 177, 175, 255), 2.1f);
            Building(world, new Vector3(-3.7f, 0, -0.2f), new Color32(163, 175, 201, 255), 1.65f);
            Building(world, new Vector3(3.7f, 0, -0.5f), new Color32(219, 189, 130, 255), 1.5f);
            for (int i = -2; i <= 2; i++) Tree(world, new Vector3(i * 1.35f, 0, 4.9f));
            Tree(world, new Vector3(-3.6f, 0, -2.5f)); Tree(world, new Vector3(3.6f, 0, -2.5f));

            var playerObject = new GameObject("Player", typeof(Rigidbody), typeof(CapsuleCollider));
            playerObject.transform.SetParent(world); playerObject.transform.position = new Vector3(0, 0.04f, -2.35f);
            var playerBody = playerObject.GetComponent<Rigidbody>();
            playerBody.useGravity = false;
            playerBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            playerBody.interpolation = RigidbodyInterpolation.Interpolate;
            playerBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var playerCollider = playerObject.GetComponent<CapsuleCollider>();
            playerCollider.radius = 0.22f; playerCollider.height = 1.1f; playerCollider.center = new Vector3(0, 0.55f, 0);
            var model = new GameObject("Voxel courier").transform; model.SetParent(playerObject.transform, false);
            Cube("Uniform", model, new Vector3(0, 0.52f, 0), new Vector3(0.43f, 0.4f, 0.29f), Cyan);
            Cube("Head", model, new Vector3(0, 0.87f, 0), new Vector3(0.36f, 0.32f, 0.32f), new Color32(247, 192, 143, 255));
            Cube("Cap", model, new Vector3(0, 1.06f, 0), new Vector3(0.41f, 0.10f, 0.37f), new Color32(31, 83, 108, 255));
            Cube("Cap brim", model, new Vector3(0, 1.02f, 0.22f), new Vector3(0.4f, 0.05f, 0.16f), new Color32(31, 83, 108, 255));
            Cube("Eye L", model, new Vector3(-0.09f, 0.9f, 0.166f), new Vector3(0.045f, 0.06f, 0.018f), Navy);
            Cube("Eye R", model, new Vector3(0.09f, 0.9f, 0.166f), new Vector3(0.045f, 0.06f, 0.018f), Navy);
            Cube("Arm L", model, new Vector3(-0.28f, 0.48f, 0), new Vector3(0.12f, 0.32f, 0.14f), Cyan);
            Cube("Arm R", model, new Vector3(0.28f, 0.48f, 0), new Vector3(0.12f, 0.32f, 0.14f), Cyan);
            var legL = Cube("Leg L", model, new Vector3(-0.12f, 0.18f, 0), new Vector3(0.15f, 0.32f, 0.17f), Navy);
            var legR = Cube("Leg R", model, new Vector3(0.12f, 0.18f, 0), new Vector3(0.15f, 0.32f, 0.17f), Navy);
            Cube("Delivery bag", model, new Vector3(0.28f, 0.37f, -0.09f), new Vector3(0.24f, 0.27f, 0.28f), Orange);
            Cube("Bag strap", model, new Vector3(0.13f, 0.55f, -0.155f), new Vector3(0.065f, 0.39f, 0.025f), new Color32(142, 87, 42, 255));
            model.localRotation = Quaternion.Euler(0, 180, 0);
            var walk = model.gameObject.AddComponent<VoxelWalk>(); walk.body = playerBody; walk.leftLeg = legL.transform; walk.rightLeg = legR.transform;
            var player = playerObject.AddComponent<CourierPlayer>();
            player.visual = model; game.player = player; player.game = game;

            Vector2[] letterPositions = {
                new Vector2(-1.9f, 3.4f), new Vector2(1.8f, 3.4f),
                new Vector2(0, 1.35f), new Vector2(-1.9f, 1.25f),
                new Vector2(1.85f, -0.8f), new Vector2(-1.85f, -0.9f)
            };
            game.letters = new LetterItem[letterPositions.Length];
            for (int i = 0; i < letterPositions.Length; i++)
            {
                var item = new GameObject("Letter " + (i + 1)); item.transform.SetParent(world);
                item.transform.position = new Vector3(letterPositions[i].x, 0.4f, letterPositions[i].y);
                Cube("Envelope", item.transform, Vector3.zero, new Vector3(0.43f, 0.31f, 0.09f), Orange);
                Cube("Stamp", item.transform, new Vector3(-0.12f, 0.075f, -0.055f), new Vector3(0.09f, 0.09f, 0.025f), Cyan);
                Cube("Fold", item.transform, new Vector3(0, -0.015f, -0.055f), new Vector3(0.3f, 0.035f, 0.02f), Color.white);
                game.letters[i] = item.AddComponent<LetterItem>();
            }
            var post = new GameObject("Mailbox", typeof(BoxCollider), typeof(Mailbox));
            post.transform.SetParent(world); post.transform.position = new Vector3(0, 0, -1.55f);
            Cube("Post base", post.transform, new Vector3(0, 0.1f, 0), new Vector3(1, 0.2f, 0.5f), new Color32(76, 92, 104, 255));
            Cube("Post box", post.transform, new Vector3(0, 0.55f, 0), new Vector3(0.62f, 0.8f, 0.42f), new Color32(191, 56, 51, 255));
            Cube("Post slot", post.transform, new Vector3(0, 0.7f, -0.22f), new Vector3(0.4f, 0.065f, 0.025f), Navy);
            Cube("Post sign", post.transform, new Vector3(0, 0.42f, -0.225f), new Vector3(0.3f, 0.18f, 0.025f), Color.white);
            var postCollider = post.GetComponent<BoxCollider>(); postCollider.isTrigger = true;
            postCollider.center = new Vector3(0, 0.5f, 0); postCollider.size = new Vector3(1.15f, 1, 0.65f);
            post.GetComponent<Mailbox>().game = game;

            game.obstacles = new MovingObstacle[2];
            for (int i = 0; i < 2; i++)
            {
                float z = i == 0 ? 2.35f : 0.2f;
                var car = new GameObject("Moving car " + (i + 1), typeof(Rigidbody), typeof(BoxCollider));
                car.transform.SetParent(world);
                car.transform.position = new Vector3(i == 0 ? -2.15f : 2.15f, 0, z);
                var color = i == 0 ? new Color32(211, 77, 52, 255) : new Color32(221, 164, 49, 255);
                Cube("Car body", car.transform, new Vector3(0, 0.27f, 0), new Vector3(0.95f, 0.3f, 0.48f), color);
                Cube("Car cabin", car.transform, new Vector3(0, 0.5f, 0), new Vector3(0.43f, 0.22f, 0.44f), color);
                Cube("Windshield", car.transform, new Vector3(0.225f, 0.5f, 0), new Vector3(0.025f, 0.16f, 0.35f), Cyan);
                Cube("Side window", car.transform, new Vector3(0, 0.5f, -0.23f), new Vector3(0.33f, 0.15f, 0.02f), Cyan);
                for (int a = -1; a <= 1; a += 2)
                    for (int b = -1; b <= 1; b += 2)
                        Cube("Wheel", car.transform, new Vector3(a * 0.30f, 0.14f, b * 0.25f), new Vector3(0.22f, 0.22f, 0.09f), Navy);
                var body = car.GetComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                var hitbox = car.GetComponent<BoxCollider>(); hitbox.center = new Vector3(0, 0.3f, 0);
                hitbox.size = new Vector3(0.96f, 0.6f, 0.56f); hitbox.isTrigger = true;
                var obstacle = car.AddComponent<MovingObstacle>(); obstacle.game = game;
                obstacle.startPoint = car.transform.position;
                obstacle.endPoint = new Vector3(-obstacle.startPoint.x, 0, z);
                obstacle.moveSpeed = i == 0 ? 1.4f : 1.7f; game.obstacles[i] = obstacle;
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
            game.deliveryLabel = Label("Delivered", hud.transform, "배달 0/6", new Vector2(-69, -24), new Vector2(290, 25), 15);
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

            var menu = Panel("District menu", safe, new Color32(16, 27, 45, 252));
            game.menuPanel = menu.gameObject;
            Label("Title", menu.transform, "시간 배달부", new Vector2(0, 245), new Vector2(430, 140), 46, Cyan);
            Label("Description", menu.transform, "편지를 모아 장애물을 피해\n시간 안에 우체통으로 배달하세요.", new Vector2(0, 120), new Vector2(420, 75), 17);
            Label("Rules", menu.transform, "제한 시간 60초 · 총 8개 중 6개 배달\n능력 편지도 포함 · 가방은 최대 3개", new Vector2(0, 33), new Vector2(400, 60), 20, Orange);
            game.districtOneButton = Button("District 1", menu.transform, "게임 시작", new Vector2(0, -95), new Vector2(370, 72), Cyan, 20);
            Label("Instructions", menu.transform, "이동: 왼쪽 가상 스틱\n수집: 편지 줍기 버튼 탭 · 감속: 시간 감속 버튼 홀드\n초록 편지: 5초 가속 · 파란 편지: 6초 보호\n바닥: 회색 안전 → 노랑 예고 → 빨강 위험\n충돌하면 제자리 스턴 + 시간 3초 감소", new Vector2(0, -280), new Vector2(420, 170), 15);
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
            root.SetActive(true);
            return root;
        }
        private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name;
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
            var collider = obj.GetComponent<Collider>(); collider.enabled = false;
            if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            obj.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Voxel");
            var tint = obj.AddComponent<VoxelTint>(); tint.color = color; tint.Apply();
            return obj;
        }
        private static void Boundary(Transform parent, Vector3 pos, Vector3 size)
        {
            var wall = Cube("City border", parent, pos, size, new Color32(119, 131, 125, 255));
            wall.AddComponent<BoxCollider>();
        }
        private static void Building(Transform parent, Vector3 pos, Color color, float height)
        {
            var building = new GameObject("Voxel building").transform; building.SetParent(parent, false); building.localPosition = pos;
            Cube("Facade", building, new Vector3(0, height / 2, 0), new Vector3(1.45f, height, 1.8f), color);
            Cube("Roof", building, new Vector3(0, height + 0.12f, 0), new Vector3(1.6f, 0.24f, 1.95f), new Color32(57, 79, 97, 255));
            Cube("Roof vent", building, new Vector3(0.35f, height + 0.36f, 0.3f), new Vector3(0.35f, 0.28f, 0.35f), new Color32(82, 95, 99, 255));
            for (int floor = 0; floor < 2; floor++)
                for (int column = -1; column <= 1; column += 2)
                {
                    float y = 0.5f + floor * 0.7f;
                    if (y + 0.2f < height)
                    {
                        Cube("Window", building, new Vector3(column * 0.38f, y, -0.91f), new Vector3(0.36f, 0.38f, 0.035f), Cyan);
                        Cube("Side window", building, new Vector3(pos.x > 0 ? -0.73f : 0.73f, y, column * 0.43f), new Vector3(0.035f, 0.38f, 0.38f), Cyan);
                    }
                }
        }
        private static void Tree(Transform parent, Vector3 pos)
        {
            Cube("Tree trunk", parent, pos + new Vector3(0, 0.4f, 0), new Vector3(0.18f, 0.8f, 0.18f), new Color32(125, 83, 52, 255));
            Cube("Tree crown", parent, pos + new Vector3(0, 1.05f, 0), new Vector3(0.72f, 0.72f, 0.72f), new Color32(83, 139, 93, 255));
            Cube("Tree top", parent, pos + new Vector3(0, 1.48f, 0), new Vector3(0.45f, 0.22f, 0.45f), new Color32(114, 167, 104, 255));
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
