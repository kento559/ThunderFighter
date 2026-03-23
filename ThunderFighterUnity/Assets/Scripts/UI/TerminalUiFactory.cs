using ThunderFighter.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderFighter.UI
{
    public static class TerminalUiFactory
    {
        private static Font cachedUiFont;
        private static readonly string[] PreferredFonts =
        {
            "Microsoft YaHei UI",
            "Microsoft YaHei",
            "DengXian",
            "SimHei",
            "SimSun",
            "Noto Sans SC",
            "Arial Unicode MS"
        };

        public static Font GetUiFont()
        {
            if (cachedUiFont != null)
            {
                return cachedUiFont;
            }

            cachedUiFont = Font.CreateDynamicFontFromOSFont(PreferredFonts, 28);
            if (cachedUiFont == null)
            {
                cachedUiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return cachedUiFont;
        }

        public static void EnsureCanvasRuntimeSettings(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = Screen.width >= Screen.height ? 0.64f : 0.84f;

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        public static Image CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            AddCorners(image.transform, new Color(Mathf.Clamp01(color.r + 0.16f), Mathf.Clamp01(color.g + 0.16f), Mathf.Clamp01(color.b + 0.2f), 0.78f));
            return image;
        }

        public static Image CreateStretchPanel(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Vector2 anchor, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            Text text = go.AddComponent<Text>();
            text.font = GetUiFont();
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.text = content;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.03f, 0.08f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -4f);
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Color fillColor, Color textColor)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.color = fillColor;
            Button button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = fillColor;
            colors.highlightedColor = fillColor * 1.08f;
            colors.pressedColor = fillColor * 0.9f;
            colors.selectedColor = fillColor * 1.04f;
            colors.disabledColor = new Color(fillColor.r * 0.4f, fillColor.g * 0.4f, fillColor.b * 0.4f, 0.55f);
            button.colors = colors;
            AddCorners(go.transform, new Color(0.85f, 0.96f, 1f, 0.88f));
            CreateText(go.transform, "Label", label, 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), size, textColor);
            return button;
        }

        public static Image CreateGlow(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Image CreateSprite(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static void AddCorners(Transform parent, Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                string cornerName = "Corner_" + i;
                if (parent.Find(cornerName) != null)
                {
                    continue;
                }

                GameObject go = new GameObject(cornerName);
                RectTransform rect = go.AddComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.anchorMin = new Vector2(i == 0 || i == 2 ? 0f : 1f, i < 2 ? 1f : 0f);
                rect.anchorMax = rect.anchorMin;
                rect.pivot = new Vector2(i == 0 || i == 2 ? 0f : 1f, i < 2 ? 1f : 0f);
                rect.sizeDelta = new Vector2(24f, 24f);
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 0f : i == 1 ? 90f : i == 2 ? -90f : 180f);
                Image image = go.AddComponent<Image>();
                image.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Wing);
                image.color = color;
                image.raycastTarget = false;
            }
        }

        public static void AddHorizontalDivider(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            image.color = color;
            image.raycastTarget = false;
        }
    }
}
