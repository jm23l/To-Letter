using UnityEngine;
using UnityEngine.UI;

namespace TimeCourier
{
    // Sharp screen-space UI follows the voxel courier's head.
    public class AbilityOverhead : MonoBehaviour
    {
        private DeliveryGame game;
        private Canvas canvas;
        private RectTransform canvasRect, panel;
        private GameObject speedRow, shieldRow;
        private Text speedLabel, shieldLabel;
        private RectTransform speedFill, shieldFill;
        private Camera worldCamera;
        private const float Width = 142;
        private const float RowHeight = 33;
        private const float BarWidth = 126;

        public static void Attach(DeliveryGame target)
        {
            var canvas = target.hintLabel.canvas;
            if (canvas == null || canvas.transform.Find("Ability Overhead") != null) return;
            var obj = new GameObject("Ability Overhead", typeof(RectTransform), typeof(AbilityOverhead));
            obj.transform.SetParent(canvas.transform, false);
            obj.GetComponent<AbilityOverhead>().Initialize(target, canvas);
        }

        private void Initialize(DeliveryGame target, Canvas owner)
        {
            game = target; canvas = owner;
            canvasRect = (RectTransform)canvas.transform;
            panel = (RectTransform)transform;
            panel.anchorMin = panel.anchorMax = Vector2.zero;
            panel.pivot = new Vector2(0.5f, 0);
            panel.sizeDelta = new Vector2(Width, RowHeight);
            var font = Resources.Load<Font>("Fonts/CourierKorean");
            CreateRow("Speed", new Color32(120, 237, 159, 255), font,
                out speedRow, out speedLabel, out speedFill);
            CreateRow("Shield", new Color32(122, 201, 255, 255), font,
                out shieldRow, out shieldLabel, out shieldFill);
            speedRow.SetActive(false); shieldRow.SetActive(false);
            worldCamera = Camera.main;
        }

        private void CreateRow(string name, Color accent, Font font,
            out GameObject row, out Text label, out RectTransform fill)
        {
            row = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)row.transform;
            rect.SetParent(panel, false); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0); rect.sizeDelta = new Vector2(Width, RowHeight - 2);
            var background = row.GetComponent<Image>();
            background.color = new Color32(16, 27, 45, 235); background.raycastTarget = false;
            var textObject = new GameObject("Remaining time", typeof(RectTransform), typeof(Text));
            var textRect = (RectTransform)textObject.transform;
            textRect.SetParent(rect, false); textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 1);
            textRect.pivot = new Vector2(0.5f, 1); textRect.anchoredPosition = new Vector2(0, -1);
            textRect.sizeDelta = new Vector2(Width - 8, 22);
            label = textObject.GetComponent<Text>(); label.font = font;
            label.fontSize = 14; label.alignment = TextAnchor.MiddleCenter;
            label.color = accent; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            var bar = new GameObject("Duration bar", typeof(RectTransform), typeof(Image));
            fill = (RectTransform)bar.transform; fill.SetParent(rect, false);
            fill.anchorMin = fill.anchorMax = Vector2.zero; fill.pivot = Vector2.zero;
            fill.anchoredPosition = new Vector2(8, 5); fill.sizeDelta = new Vector2(BarWidth, 4);
            var image = bar.GetComponent<Image>(); image.color = accent; image.raycastTarget = false;
        }

        private void LateUpdate()
        {
            if (game == null || panel == null) return;
            if (worldCamera == null) worldCamera = Camera.main;
            bool visible = game.IsPlaying && game.player != null && worldCamera != null;
            Vector3 viewport = visible ? worldCamera.WorldToViewportPoint(
                game.player.transform.position + Vector3.up * 1.38f) : Vector3.zero;
            visible = visible && viewport.z > 0 && viewport.x >= 0 && viewport.x <= 1 &&
                viewport.y >= 0 && viewport.y <= 1;
            bool speed = visible && game.player.SpeedRemaining > 0;
            bool shield = visible && game.player.ShieldRemaining > 0;
            speedRow.SetActive(speed); shieldRow.SetActive(shield);
            if (!speed && !shield) return;
            int rows = (speed ? 1 : 0) + (shield ? 1 : 0);
            float height = rows * RowHeight;
            panel.sizeDelta = new Vector2(Width, height);
            if (speed)
            {
                ((RectTransform)speedRow.transform).anchoredPosition = new Vector2(0, shield ? RowHeight : 0);
                speedLabel.text = $"가속  {game.player.SpeedRemaining:0.0}초";
                speedFill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(game.player.SpeedRemaining / 3f), 4);
            }
            if (shield)
            {
                ((RectTransform)shieldRow.transform).anchoredPosition = Vector2.zero;
                shieldLabel.text = $"보호  {game.player.ShieldRemaining:0.0}초";
                shieldFill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(game.player.ShieldRemaining / 4f), 4);
            }
            float w = canvasRect.rect.width, h = canvasRect.rect.height;
            panel.anchoredPosition = new Vector2(
                Mathf.Clamp(viewport.x * w, Width / 2 + 4, Mathf.Max(Width / 2 + 4, w - Width / 2 - 4)),
                Mathf.Clamp(viewport.y * h + 7, 4, Mathf.Max(4, h - height - 4)));
        }
    }
}
