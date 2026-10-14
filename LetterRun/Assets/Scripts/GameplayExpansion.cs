using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TimeCourier
{
    // Installed at Start so existing saved scenes receive the update without being reset.
    public static class GameplayExpansion
    {
        public static void Install(DeliveryGame game)
        {
            AbilityOverhead.Attach(game);
            if (game.transform.Find("Gameplay Expansion") != null) return;
            var root = new GameObject("Gameplay Expansion").transform;
            root.SetParent(game.transform, false);
            var letters = new List<LetterItem>(game.letters);
            letters.Add(PowerLetter(root, "Speed letter", new Vector3(-1.15f, 0.4f, -2.55f),
                LetterKind.Speed, new Color32(100, 240, 140, 255)));
            letters.Add(PowerLetter(root, "Shield letter", new Vector3(1.3f, 0.4f, 1.3f),
                LetterKind.Shield, new Color32(90, 170, 255, 255)));
            game.letters = letters.ToArray();
            Hazard(root, game, "Timed hazard A", new Vector3(-0.85f, 0, 1.1f), 0);
            Hazard(root, game, "Timed hazard B", new Vector3(0.95f, 0, -0.55f), 1.2f);
        }

        // Kept compatible with existing scene references. The second button is disabled.
        public static bool ConfigureStartMenu(DeliveryGame game)
        {
            bool changed = false;
            if (game.districtTwoButton != null && game.districtTwoButton.gameObject.activeSelf)
            { game.districtTwoButton.gameObject.SetActive(false); changed = true; }
            if (game.districtOneButton != null)
            {
                var label = game.districtOneButton.GetComponentInChildren<Text>(true);
                if (label != null && label.text != "게임 시작")
                { label.text = "게임 시작"; changed = true; }
                var rect = (RectTransform)game.districtOneButton.transform;
                if (rect.anchoredPosition != new Vector2(0, -95) || rect.sizeDelta != new Vector2(370, 72))
                {
                    rect.anchoredPosition = new Vector2(0, -95);
                    rect.sizeDelta = new Vector2(370, 72);
                    changed = true;
                }
            }
            if (game.menuButton != null)
            {
                var label = game.menuButton.GetComponentInChildren<Text>(true);
                if (label != null && label.text != "처음 화면")
                { label.text = "처음 화면"; changed = true; }
            }
            foreach (var label in game.GetComponentsInChildren<Text>(true))
            {
                if (label.name != "Instructions") continue;
                string mobileText = label.text
                    .Replace("이동: 가상 스틱 / 방향키", "이동: 왼쪽 가상 스틱")
                    .Replace("수집: 버튼 탭 / E · 감속: 홀드 / 스페이스바",
                        "수집: 편지 줍기 버튼 탭 · 감속: 시간 감속 버튼 홀드");
                if (label.text != mobileText) { label.text = mobileText; changed = true; }
            }
            return changed;
        }

        public static void LocalizeUI(DeliveryGame game)
        {
            var font = Resources.Load<Font>("Fonts/CourierKorean");
            if (font == null)
            {
                Debug.LogError("Korean font missing: copy Assets/Resources/Fonts/CourierKorean.ttf from the update.");
                return;
            }
            foreach (var text in game.GetComponentsInChildren<Text>(true))
            {
                text.font = font;
                switch (text.name)
                {
                    case "Pause text": text.text = "일시정지"; text.fontSize = 14; break;
                    case "Move label": text.text = "이동"; break;
                    case "Collect text": text.text = "편지 줍기"; break;
                    case "Slow label": text.text = "꾹 눌러\n시간 감속"; break;
                    case "Retry text": text.text = "다시 하기"; break;
                    case "Menu text": text.text = "처음 화면"; break;
                    case "Paused label": text.text = "일시정지"; break;
                    case "Resume text": text.text = "계속하기"; break;
                    case "Result text": text.text = "배달 결과"; break;
                    case "Bag": text.text = "가방 0/3"; break;
                    case "Delivered": text.text = "배달 0/6"; break;
                    case "Title": text.text = "시간 배달부"; text.fontSize = 46; break;
                    case "Description":
                        text.text = "편지를 모아 장애물을 피해\n시간 안에 우체통으로 배달하세요."; break;
                    case "Rules": text.text = "제한 시간 60초 · 총 8개 중 6개 배달\n능력 편지도 포함 · 가방은 최대 3개"; break;
                    case "District 1 text": text.text = "게임 시작"; break;
                    case "Instructions":
                        text.fontSize = 15;
                        text.text = "이동: 왼쪽 가상 스틱\n수집: 편지 줍기 버튼 탭 · 감속: 시간 감속 버튼 홀드\n초록 편지: 5초 가속 · 파란 편지: 6초 보호\n바닥: 회색 안전 → 노랑 예고 → 빨강 위험\n충돌하면 제자리 스턴 + 시간 3초 감소";
                        break;
                }
            }
        }

        private static LetterItem PowerLetter(Transform root, string name, Vector3 position,
            LetterKind kind, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(root, false);
            obj.transform.position = position;
            Block("Envelope", obj.transform, Vector3.zero, new Vector3(0.48f, 0.34f, 0.1f), color);
            Block("Mark", obj.transform, new Vector3(0, 0, -0.06f), new Vector3(0.08f, 0.22f, 0.02f), Color.white);
            if (kind == LetterKind.Shield)
                Block("Shield mark", obj.transform, new Vector3(0, 0.06f, -0.065f), new Vector3(0.23f, 0.08f, 0.02f), Color.white);
            else
            {
                Block("Speed mark left", obj.transform, new Vector3(-0.10f, -0.07f, -0.065f), new Vector3(0.16f, 0.06f, 0.02f), Color.white);
                Block("Speed mark right", obj.transform, new Vector3(0.10f, 0.07f, -0.065f), new Vector3(0.16f, 0.06f, 0.02f), Color.white);
            }
            var letter = obj.AddComponent<LetterItem>();
            letter.kind = kind;
            return letter;
        }

        private static void Hazard(Transform root, DeliveryGame game, string name,
            Vector3 position, float delay)
        {
            var obj = new GameObject(name, typeof(BoxCollider));
            obj.transform.SetParent(root, false); obj.transform.position = position;
            var tile = Block("Warning tile", obj.transform, new Vector3(0, 0.07f, 0),
                new Vector3(1.05f, 0.07f, 0.75f), new Color32(92, 110, 121, 255));
            var bars = new GameObject("Raised barrier");
            bars.transform.SetParent(obj.transform, false);
            for (int i = -1; i <= 1; i++)
                Block("Danger post", bars.transform, new Vector3(i * 0.32f, 0.38f, 0),
                    new Vector3(0.13f, 0.58f, 0.44f), new Color32(230, 70, 60, 255));
            bars.SetActive(false);
            var box = obj.GetComponent<BoxCollider>();
            box.isTrigger = true; box.center = new Vector3(0, 0.4f, 0);
            box.size = new Vector3(1.05f, 0.8f, 0.75f);
            var hazard = obj.AddComponent<TimedHazard>();
            hazard.game = game; hazard.warningTile = tile.GetComponent<VoxelTint>();
            hazard.raisedBarrier = bars; hazard.initialDelay = delay;
            hazard.ResetHazard(1);
        }

        private static GameObject Block(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale;
            var collider = obj.GetComponent<Collider>();
            collider.enabled = false; Object.Destroy(collider);
            obj.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Materials/Voxel");
            var tint = obj.AddComponent<VoxelTint>(); tint.color = color; tint.Apply();
            return obj;
        }
    }
}
