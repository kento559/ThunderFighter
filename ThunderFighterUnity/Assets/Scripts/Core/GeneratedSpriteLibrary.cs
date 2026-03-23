using System.Collections.Generic;
using UnityEngine;

namespace ThunderFighter.Core
{
    public enum GeneratedSpriteKind
    {
        Hull = 0,
        Nose = 1,
        Cockpit = 2,
        Wing = 3,
        Engine = 4,
        Thruster = 5,
        Bullet = 6,
        Flash = 7,
        Ring = 8
    }

    public static class GeneratedSpriteLibrary
    {
        private const int TextureSize = 256;
        private static readonly Dictionary<GeneratedSpriteKind, Sprite> Cache = new Dictionary<GeneratedSpriteKind, Sprite>();
        private static readonly Dictionary<string, Sprite> ShipPresentationCache = new Dictionary<string, Sprite>();

        public static Sprite Get(GeneratedSpriteKind kind)
        {
            if (Cache.TryGetValue(kind, out Sprite sprite) && sprite != null)
            {
                return sprite;
            }

            sprite = CreateSprite(kind);
            Cache[kind] = sprite;
            return sprite;
        }

        public static Sprite GetShipPresentationSprite(ShipId shipId, bool portrait)
        {
            string key = shipId + (portrait ? "_portrait" : "_battle");
            if (ShipPresentationCache.TryGetValue(key, out Sprite sprite) && sprite != null)
            {
                return sprite;
            }

            sprite = CreateShipPresentationSprite(shipId, portrait);
            ShipPresentationCache[key] = sprite;
            return sprite;
        }

        private static Sprite CreateSprite(GeneratedSpriteKind kind)
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = x / (TextureSize - 1f) * 2f - 1f;
                    float v = y / (TextureSize - 1f) * 2f - 1f;
                    float alpha = Evaluate(kind, u, v);
                    float brightness = EvaluateBrightness(kind, u, v, alpha);
                    pixels[(y * TextureSize) + x] = new Color(brightness, brightness, brightness, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 128f);
        }

        private static Sprite CreateShipPresentationSprite(ShipId shipId, bool portrait)
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[TextureSize * TextureSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            if (portrait)
            {
                PaintPortraitBackdrop(pixels, shipId);
            }

            PaintShipPresentation(pixels, shipId, portrait);
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), portrait ? 164f : 156f);
        }

        private static float Evaluate(GeneratedSpriteKind kind, float u, float v)
        {
            switch (kind)
            {
                case GeneratedSpriteKind.Nose:
                    return SmoothMask(1f - (Mathf.Abs(u) * 1.32f + Mathf.Abs(v + 0.12f) * 0.82f), 0.018f);
                case GeneratedSpriteKind.Cockpit:
                    return SmoothMask(1f - ((u * u) / 0.24f + (v * v) / 0.56f), 0.018f);
                case GeneratedSpriteKind.Wing:
                    return WingMask(u, v);
                case GeneratedSpriteKind.Engine:
                    return RoundedBoxMask(u, v, 0.46f, 0.74f, 0.3f);
                case GeneratedSpriteKind.Thruster:
                    return ThrusterMask(u, v);
                case GeneratedSpriteKind.Bullet:
                    return SmoothMask(1f - ((u * u) / 0.26f + (v * v) / 1.25f), 0.015f);
                case GeneratedSpriteKind.Flash:
                    return FlashMask(u, v);
                case GeneratedSpriteKind.Ring:
                    return RingMask(u, v);
                case GeneratedSpriteKind.Hull:
                default:
                    return RoundedBoxMask(u, v, 0.56f, 0.82f, 0.24f);
            }
        }

        private static float EvaluateBrightness(GeneratedSpriteKind kind, float u, float v, float alpha)
        {
            if (alpha <= 0.001f)
            {
                return 0f;
            }

            float radial = 1f - Mathf.Clamp01(Mathf.Sqrt((u * u) + (v * v)));
            float highlight = Mathf.Clamp01(((-u * 0.38f) + (v * 0.92f) + 0.42f));
            float centerBand = Mathf.Clamp01(1f - Mathf.Abs(u) * 1.4f);
            float panelShade = 0.82f + radial * 0.12f + highlight * 0.16f + centerBand * 0.08f;

            switch (kind)
            {
                case GeneratedSpriteKind.Cockpit:
                    return Mathf.Clamp01(0.72f + radial * 0.2f + highlight * 0.18f);
                case GeneratedSpriteKind.Nose:
                    return Mathf.Clamp01(panelShade + 0.06f);
                case GeneratedSpriteKind.Wing:
                    return Mathf.Clamp01(0.74f + radial * 0.08f + highlight * 0.18f);
                case GeneratedSpriteKind.Engine:
                    return Mathf.Clamp01(0.66f + radial * 0.1f + highlight * 0.14f);
                case GeneratedSpriteKind.Thruster:
                    return Mathf.Clamp01(0.82f + radial * 0.1f + highlight * 0.08f);
                case GeneratedSpriteKind.Bullet:
                    return Mathf.Clamp01(0.92f + radial * 0.08f);
                case GeneratedSpriteKind.Flash:
                    return Mathf.Clamp01(0.96f + radial * 0.04f);
                case GeneratedSpriteKind.Ring:
                    return Mathf.Clamp01(0.9f + highlight * 0.08f + radial * 0.06f);
                case GeneratedSpriteKind.Hull:
                default:
                    return Mathf.Clamp01(panelShade);
            }
        }

        private static float RoundedBoxMask(float u, float v, float halfWidth, float halfHeight, float radius)
        {
            float qx = Mathf.Abs(u) - halfWidth + radius;
            float qy = Mathf.Abs(v) - halfHeight + radius;
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
            return SmoothMask(radius - (outside + inside), 0.03f);
        }

        private static float WingMask(float u, float v)
        {
            Vector2 point = new Vector2(u, v);
            float mainWing = TriangleMask(point, new Vector2(-0.98f, -0.16f), new Vector2(0.94f, 0.06f), new Vector2(-0.38f, 0.46f), 0.03f);
            float trailingCut = TriangleMask(point, new Vector2(-0.72f, -0.08f), new Vector2(-0.1f, 0.02f), new Vector2(-0.42f, 0.34f), 0.02f);
            return Mathf.Clamp01(Mathf.Max(0f, mainWing - trailingCut * 0.45f));
        }

        private static float ThrusterMask(float u, float v)
        {
            float ellipse = SmoothMask(1f - ((u * u) / 0.22f + ((v + 0.2f) * (v + 0.2f)) / 0.32f), 0.025f);
            float tail = TriangleMask(new Vector2(u, v), new Vector2(-0.26f, -0.1f), new Vector2(0.26f, -0.1f), new Vector2(0f, 0.95f), 0.02f);
            return Mathf.Clamp01(Mathf.Max(ellipse, tail));
        }

        private static float FlashMask(float u, float v)
        {
            float core = SmoothMask(1f - ((u * u) / 0.14f + (v * v) / 0.14f), 0.02f);
            float vertical = SmoothMask(1f - (Mathf.Abs(u) * 3.8f + Mathf.Abs(v) * 0.9f), 0.02f);
            float horizontal = SmoothMask(1f - (Mathf.Abs(v) * 3.8f + Mathf.Abs(u) * 0.9f), 0.02f);
            return Mathf.Clamp01(Mathf.Max(core, Mathf.Max(vertical, horizontal)));
        }

        private static float RingMask(float u, float v)
        {
            float distance = Mathf.Sqrt((u * u) + (v * v));
            float outer = SmoothMask(0.82f - distance, 0.03f);
            float inner = SmoothMask(distance - 0.54f, 0.03f);
            return Mathf.Clamp01(outer * inner);
        }

        private static float TriangleMask(Vector2 p, Vector2 a, Vector2 b, Vector2 c, float feather)
        {
            float area = Cross(b - a, c - a);
            float w0 = Cross(b - p, c - p) / area;
            float w1 = Cross(c - p, a - p) / area;
            float w2 = Cross(a - p, b - p) / area;
            float edge = Mathf.Min(w0, Mathf.Min(w1, w2));
            return SmoothMask(edge, feather);
        }

        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private static float SmoothMask(float value, float feather)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-feather, feather, value));
        }

        private static void PaintPortraitBackdrop(Color[] pixels, ShipId shipId)
        {
            Color glow = GetShipAccentColor(shipId);
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = x / (TextureSize - 1f) * 2f - 1f;
                    float v = y / (TextureSize - 1f) * 2f - 1f;
                    float radial = Mathf.Clamp01(1f - Mathf.Sqrt((u * u) + (v * v)));
                    float ring = Mathf.Clamp01(Mathf.InverseLerp(0.18f, 0.03f, Mathf.Abs(Mathf.Sqrt((u * u) + (v * v)) - 0.68f)));
                    BlendPixel(pixels, x, y, new Color(glow.r * (0.08f + radial * 0.16f), glow.g * (0.08f + radial * 0.16f), glow.b * (0.1f + radial * 0.18f), Mathf.Clamp01(0.05f + radial * 0.14f + ring * 0.09f)));
                }
            }
        }

        private static void PaintShipPresentation(Color[] pixels, ShipId shipId, bool portrait)
        {
            Color hull = GetShipHullColor(shipId);
            Color accent = GetShipAccentColor(shipId);
            Color canopyColor = GetShipCanopyColor(shipId);
            float scale = portrait ? 0.82f : 0.76f;

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = (x / (TextureSize - 1f) * 2f - 1f) / scale;
                    float v = (y / (TextureSize - 1f) * 2f - 1f) / scale;
                    float alpha = shipId switch
                    {
                        ShipId.Rapid => RapidShipMask(u, v),
                        ShipId.Heavy => HeavyShipMask(u, v),
                        _ => BalancedShipMask(u, v)
                    };

                    if (alpha <= 0.001f)
                    {
                        continue;
                    }

                    float highlight = Mathf.Clamp01((-u * 0.18f) + (v * 0.7f) + 0.44f);
                    float panel = Mathf.Clamp01(1f - Mathf.Abs(u) * 1.35f);
                    float accentMask = EvaluateShipAccentMask(shipId, u, v);
                    float panelMask = EvaluateShipPanelMask(shipId, u, v);
                    float trimMask = EvaluateShipTrimMask(shipId, u, v);
                    float engineMask = EvaluateShipEngineMask(shipId, u, v);
                    float underbellyMask = EvaluateShipUnderbellyMask(shipId, u, v);
                    float intakeMask = EvaluateShipIntakeMask(shipId, u, v);
                    float hardpointMask = EvaluateShipHardpointMask(shipId, u, v);
                    float plateMask = EvaluateShipPlateMask(shipId, u, v);
                    float seamMask = EvaluateShipSeamMask(shipId, u, v);
                    float specularMask = EvaluateShipSpecularMask(shipId, u, v);
                    float glowMask = EvaluateShipGlowMask(shipId, u, v);
                    float ribMask = EvaluateShipRibMask(shipId, u, v);
                    float warmthMask = Mathf.Clamp01((u * -0.14f) + (v * 0.42f) + 0.16f);
                    float coolMask = Mathf.Clamp01((-u * 0.3f) + (1f - Mathf.Abs(v + 0.06f)) * 0.24f);

                    Color baseMetal = Color.Lerp(hull * 0.42f, hull, 0.46f + highlight * 0.22f);
                    baseMetal = Color.Lerp(baseMetal, new Color(0.08f, 0.1f, 0.14f, 1f), underbellyMask * 0.34f);
                    baseMetal = Color.Lerp(baseMetal, hull * 1.08f, plateMask * 0.26f);
                    baseMetal = Color.Lerp(baseMetal, new Color(0.88f, 0.92f, 1f, 1f), specularMask * 0.22f);
                    baseMetal = Color.Lerp(baseMetal, accent * 0.18f + baseMetal * 0.82f, coolMask * 0.12f);
                    baseMetal = Color.Lerp(baseMetal, new Color(0.98f, 0.84f, 0.68f, 1f), warmthMask * 0.06f);

                    Color color = baseMetal;
                    color = Color.Lerp(color, hull * 0.56f, panelMask * 0.3f);
                    color = Color.Lerp(color, hull * 0.42f, underbellyMask * 0.48f);
                    color = Color.Lerp(color, Color.Lerp(hull * 1.04f, Color.white, 0.14f), ribMask * 0.26f);
                    color = Color.Lerp(color, accent, accentMask * 0.82f);
                    color = Color.Lerp(color, accent * 0.86f, intakeMask * 0.52f);
                    color = Color.Lerp(color, Color.Lerp(accent, Color.white, 0.22f), hardpointMask * 0.4f);
                    color = Color.Lerp(color, Color.white, trimMask * 0.44f + specularMask * 0.14f);
                    color = Color.Lerp(color, accent * 1.18f, engineMask * 0.42f);
                    color = Color.Lerp(color, canopyColor * 0.9f, glowMask * 0.36f);
                    color = Color.Lerp(color, new Color(0.05f, 0.06f, 0.08f, 1f), seamMask * 0.36f);
                    color = Color.Lerp(color, Color.white, Mathf.Clamp01(panel * 0.12f + highlight * 0.2f + specularMask * 0.2f));
                    color.a = alpha;
                    BlendPixel(pixels, x, y, color);
                }
            }

            PaintCanopy(pixels, shipId, scale, canopyColor);
        }

        private static void PaintCanopy(Color[] pixels, ShipId shipId, float scale, Color canopy)
        {
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = (x / (TextureSize - 1f) * 2f - 1f) / scale;
                    float v = (y / (TextureSize - 1f) * 2f - 1f) / scale;
                    float alpha = Mathf.Clamp01(1f - ((u * u) / 0.05f + ((v - 0.08f) * (v - 0.08f)) / 0.16f));
                    if (alpha <= 0.01f)
                    {
                        continue;
                    }

                    float topHighlight = Mathf.Clamp01((-u * 0.18f) + (v * 0.86f) + 0.56f);
                    float energyBand = Mathf.Clamp01(1f - Mathf.Abs(u) * 4.8f) * Mathf.Clamp01(Mathf.InverseLerp(-0.02f, 0.28f, v));
                    Color glass = Color.Lerp(canopy * 0.72f, canopy, topHighlight * 0.46f);
                    glass = Color.Lerp(glass, Color.white, topHighlight * 0.18f + energyBand * 0.16f);
                    BlendPixel(pixels, x, y, new Color(glass.r, glass.g, glass.b, alpha * 0.94f));
                }
            }
        }

        private static float BalancedShipMask(float u, float v)
        {
            Vector2 point = new Vector2(u, v);
            float spine = RoundedBoxMask(u, v, 0.16f, 0.78f, 0.08f);
            float nose = TriangleMask(point, new Vector2(-0.14f, 0.34f), new Vector2(0.14f, 0.34f), new Vector2(0f, 1.14f), 0.028f);
            float shoulderPods = Mathf.Max(
                RoundedBoxMask(u + 0.24f, v - 0.02f, 0.12f, 0.26f, 0.05f),
                RoundedBoxMask(u - 0.24f, v - 0.02f, 0.12f, 0.26f, 0.05f));
            float mainWings = Mathf.Max(
                TriangleMask(point, new Vector2(-0.96f, -0.12f), new Vector2(-0.2f, 0.08f), new Vector2(-0.38f, 0.6f), 0.028f),
                TriangleMask(point, new Vector2(0.96f, -0.12f), new Vector2(0.2f, 0.08f), new Vector2(0.38f, 0.6f), 0.028f));
            float canards = Mathf.Max(
                TriangleMask(point, new Vector2(-0.46f, 0.44f), new Vector2(-0.08f, 0.28f), new Vector2(-0.2f, 0.76f), 0.022f),
                TriangleMask(point, new Vector2(0.46f, 0.44f), new Vector2(0.08f, 0.28f), new Vector2(0.2f, 0.76f), 0.022f));
            float ventralFins = Mathf.Max(
                TriangleMask(point, new Vector2(-0.34f, -0.98f), new Vector2(-0.06f, -0.42f), new Vector2(-0.16f, 0.02f), 0.024f),
                TriangleMask(point, new Vector2(0.34f, -0.98f), new Vector2(0.06f, -0.42f), new Vector2(0.16f, 0.02f), 0.024f));
            float engineBlock = RoundedBoxMask(u, v + 0.84f, 0.26f, 0.16f, 0.06f);
            return Mathf.Clamp01(Mathf.Max(spine, Mathf.Max(nose, Mathf.Max(shoulderPods, Mathf.Max(mainWings, Mathf.Max(canards, Mathf.Max(ventralFins, engineBlock)))))));
        }

        private static float RapidShipMask(float u, float v)
        {
            Vector2 point = new Vector2(u, v);
            float spine = RoundedBoxMask(u, v, 0.11f, 0.88f, 0.06f);
            float nose = TriangleMask(point, new Vector2(-0.08f, 0.18f), new Vector2(0.08f, 0.18f), new Vector2(0f, 1.2f), 0.022f);
            float needle = BoxMask(u, v - 0.08f, 0.05f, 0.96f);
            float sweptWings = Mathf.Max(
                TriangleMask(point, new Vector2(-1.08f, -0.04f), new Vector2(-0.12f, 0.12f), new Vector2(-0.32f, 0.74f), 0.022f),
                TriangleMask(point, new Vector2(1.08f, -0.04f), new Vector2(0.12f, 0.12f), new Vector2(0.32f, 0.74f), 0.022f));
            float dorsalBlades = Mathf.Max(
                TriangleMask(point, new Vector2(-0.22f, -0.12f), new Vector2(-0.02f, 0.36f), new Vector2(-0.08f, 0.96f), 0.02f),
                TriangleMask(point, new Vector2(0.22f, -0.12f), new Vector2(0.02f, 0.36f), new Vector2(0.08f, 0.96f), 0.02f));
            float aftFins = Mathf.Max(
                TriangleMask(point, new Vector2(-0.54f, -1.02f), new Vector2(-0.08f, -0.36f), new Vector2(-0.18f, 0.1f), 0.022f),
                TriangleMask(point, new Vector2(0.54f, -1.02f), new Vector2(0.08f, -0.36f), new Vector2(0.18f, 0.1f), 0.022f));
            float nacelles = Mathf.Max(
                RoundedBoxMask(u + 0.28f, v + 0.12f, 0.1f, 0.22f, 0.05f),
                RoundedBoxMask(u - 0.28f, v + 0.12f, 0.1f, 0.22f, 0.05f));
            return Mathf.Clamp01(Mathf.Max(spine, Mathf.Max(needle, Mathf.Max(nose, Mathf.Max(sweptWings, Mathf.Max(dorsalBlades, Mathf.Max(aftFins, nacelles)))))));
        }

        private static float HeavyShipMask(float u, float v)
        {
            Vector2 point = new Vector2(u, v);
            float hull = RoundedBoxMask(u, v, 0.36f, 0.86f, 0.18f);
            float nose = TriangleMask(point, new Vector2(-0.18f, 0.4f), new Vector2(0.18f, 0.4f), new Vector2(0f, 1.06f), 0.024f);
            float bridge = RoundedBoxMask(u, v + 0.18f, 0.18f, 0.24f, 0.08f);
            float shoulders = Mathf.Max(
                RoundedBoxMask(u + 0.48f, v + 0.02f, 0.26f, 0.34f, 0.08f),
                RoundedBoxMask(u - 0.48f, v + 0.02f, 0.26f, 0.34f, 0.08f));
            float batteringWings = Mathf.Max(
                TriangleMask(point, new Vector2(-1.04f, -0.22f), new Vector2(-0.28f, -0.02f), new Vector2(-0.52f, 0.52f), 0.024f),
                TriangleMask(point, new Vector2(1.04f, -0.22f), new Vector2(0.28f, -0.02f), new Vector2(0.52f, 0.52f), 0.024f));
            float ventralPods = Mathf.Max(
                RoundedBoxMask(u + 0.26f, v - 0.58f, 0.12f, 0.24f, 0.06f),
                RoundedBoxMask(u - 0.26f, v - 0.58f, 0.12f, 0.24f, 0.06f));
            float engineBlock = RoundedBoxMask(u, v + 0.94f, 0.28f, 0.18f, 0.06f);
            return Mathf.Clamp01(Mathf.Max(hull, Mathf.Max(nose, Mathf.Max(bridge, Mathf.Max(shoulders, Mathf.Max(batteringWings, Mathf.Max(ventralPods, engineBlock)))))));
        }

        private static float EvaluateShipUnderbellyMask(ShipId shipId, float u, float v)
        {
            float belly = Mathf.Clamp01(Mathf.InverseLerp(-0.82f, 0.12f, -v)) * Mathf.Clamp01(1f - Mathf.Abs(u) * 1.7f);
            float wingShadow = Mathf.Clamp01(Mathf.InverseLerp(0.2f, 0.6f, Mathf.Abs(u))) * Mathf.Clamp01(Mathf.InverseLerp(-0.24f, 0.38f, -v));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(belly * 0.45f + wingShadow * 0.32f),
                ShipId.Heavy => Mathf.Clamp01(belly * 0.62f + wingShadow * 0.48f),
                _ => Mathf.Clamp01(belly * 0.52f + wingShadow * 0.36f)
            };
        }

        private static float EvaluateShipIntakeMask(ShipId shipId, float u, float v)
        {
            float left = Mathf.Clamp01(1f - (((u + 0.26f) * (u + 0.26f)) / 0.014f + ((v + 0.08f) * (v + 0.08f)) / 0.042f));
            float right = Mathf.Clamp01(1f - (((u - 0.26f) * (u - 0.26f)) / 0.014f + ((v + 0.08f) * (v + 0.08f)) / 0.042f));
            float center = Mathf.Clamp01(1f - ((u * u) / 0.02f + ((v - 0.18f) * (v - 0.18f)) / 0.05f));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(left + right),
                ShipId.Heavy => Mathf.Clamp01(center * 0.35f + (left + right) * 0.5f),
                _ => Mathf.Clamp01(center * 0.42f + (left + right) * 0.52f)
            };
        }

        private static float EvaluateShipHardpointMask(ShipId shipId, float u, float v)
        {
            float sideMounts = Mathf.Clamp01(Mathf.InverseLerp(0.18f, 0.34f, Mathf.Abs(u))) * Mathf.Clamp01(Mathf.InverseLerp(-0.22f, 0.18f, v));
            float wingRoots = Mathf.Clamp01(Mathf.InverseLerp(0.12f, 0.24f, Mathf.Abs(u))) * Mathf.Clamp01(Mathf.InverseLerp(0.02f, 0.56f, v));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(sideMounts * 0.45f + wingRoots * 0.75f),
                ShipId.Heavy => Mathf.Clamp01(sideMounts * 0.82f + wingRoots * 0.3f),
                _ => Mathf.Clamp01(sideMounts * 0.58f + wingRoots * 0.52f)
            };
        }

        private static float EvaluateShipPlateMask(ShipId shipId, float u, float v)
        {
            float spinePlate = Mathf.Clamp01(1f - Mathf.Abs(u) * 5.4f) * Mathf.Clamp01(Mathf.InverseLerp(-0.72f, 0.78f, v));
            float banding = Mathf.Clamp01(Mathf.InverseLerp(0.28f, 0.06f, Mathf.Abs(Mathf.Sin((v + 0.82f) * 8.2f))));
            float sidePlate = Mathf.Clamp01(Mathf.InverseLerp(0.18f, 0.42f, Mathf.Abs(u))) * Mathf.Clamp01(Mathf.InverseLerp(-0.1f, 0.5f, v));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(spinePlate * 0.46f + sidePlate * 0.92f * banding),
                ShipId.Heavy => Mathf.Clamp01(spinePlate * 0.72f * banding + sidePlate * 0.86f),
                _ => Mathf.Clamp01(spinePlate * 0.62f + sidePlate * 0.58f * banding)
            };
        }

        private static float EvaluateShipSeamMask(ShipId shipId, float u, float v)
        {
            float centerSeam = Mathf.Clamp01(1f - Mathf.Abs(u) * 18f) * Mathf.Clamp01(Mathf.InverseLerp(-0.76f, 0.84f, v));
            float transverse = Mathf.Clamp01(1f - Mathf.Abs(v * 7.2f - Mathf.Round(v * 7.2f))) * 0.24f;
            float sideCuts = Mathf.Clamp01(Mathf.InverseLerp(0.22f, 0.34f, Mathf.Abs(u))) * Mathf.Clamp01(Mathf.InverseLerp(-0.2f, 0.4f, v));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(centerSeam * 0.18f + sideCuts * 0.3f + transverse * 0.1f),
                ShipId.Heavy => Mathf.Clamp01(centerSeam * 0.26f + sideCuts * 0.42f + transverse * 0.14f),
                _ => Mathf.Clamp01(centerSeam * 0.22f + sideCuts * 0.34f + transverse * 0.12f)
            };
        }

        private static float EvaluateShipSpecularMask(ShipId shipId, float u, float v)
        {
            float diagonal = Mathf.Clamp01(1f - Mathf.Abs((u * 0.92f) + (v * 0.56f) - 0.12f) * 5.8f);
            float noseGlint = Mathf.Clamp01(1f - Mathf.Abs(u) * 8.2f) * Mathf.Clamp01(Mathf.InverseLerp(0.32f, 1.04f, v));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(diagonal * 0.32f + noseGlint * 0.18f),
                ShipId.Heavy => Mathf.Clamp01(diagonal * 0.18f + noseGlint * 0.12f),
                _ => Mathf.Clamp01(diagonal * 0.26f + noseGlint * 0.16f)
            };
        }

        private static float EvaluateShipGlowMask(ShipId shipId, float u, float v)
        {
            float cockpit = Mathf.Clamp01(1f - ((u * u) / 0.028f + ((v - 0.14f) * (v - 0.14f)) / 0.06f));
            float ventLeft = Mathf.Clamp01(1f - (((u + 0.22f) * (u + 0.22f)) / 0.02f + ((v + 0.04f) * (v + 0.04f)) / 0.05f));
            float ventRight = Mathf.Clamp01(1f - (((u - 0.22f) * (u - 0.22f)) / 0.02f + ((v + 0.04f) * (v + 0.04f)) / 0.05f));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(cockpit * 0.38f + (ventLeft + ventRight) * 0.28f),
                ShipId.Heavy => Mathf.Clamp01(cockpit * 0.24f + (ventLeft + ventRight) * 0.22f),
                _ => Mathf.Clamp01(cockpit * 0.34f + (ventLeft + ventRight) * 0.24f)
            };
        }

        private static float EvaluateShipRibMask(ShipId shipId, float u, float v)
        {
            float spineRib = Mathf.Clamp01(1f - Mathf.Abs(u) * 6.8f) * Mathf.Clamp01(Mathf.InverseLerp(-0.66f, 0.48f, v));
            float segment = Mathf.Clamp01(Mathf.InverseLerp(0.18f, 0.04f, Mathf.Abs(Mathf.Sin((v + 0.42f) * 13.5f))));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(spineRib * segment * 0.18f),
                ShipId.Heavy => Mathf.Clamp01(spineRib * segment * 0.28f),
                _ => Mathf.Clamp01(spineRib * segment * 0.22f)
            };
        }

        private static float EvaluateShipAccentMask(ShipId shipId, float u, float v)
        {
            float stripe = Mathf.Clamp01(1f - Mathf.Abs(u) * 4.4f) * Mathf.Clamp01(Mathf.InverseLerp(-0.55f, 0.65f, v));
            float wingEdge = Mathf.Clamp01(Mathf.InverseLerp(0.1f, 0.34f, Mathf.Abs(u))) * Mathf.Clamp01(Mathf.InverseLerp(-0.2f, 0.58f, v));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(stripe * 0.7f + wingEdge * 1.1f),
                ShipId.Heavy => Mathf.Clamp01(stripe * 0.38f + Mathf.Clamp01(1f - Mathf.Abs(v + 0.34f) * 3.2f) * Mathf.Clamp01(1f - Mathf.Abs(u) * 1.5f)),
                _ => Mathf.Clamp01(stripe * 0.9f + wingEdge * 0.75f)
            };
        }

        private static float EvaluateShipPanelMask(ShipId shipId, float u, float v)
        {
            float spinePanels = Mathf.Clamp01(1f - Mathf.Abs(u) * 7.6f) * Mathf.Clamp01(Mathf.InverseLerp(-0.72f, 0.54f, v)) * Mathf.Clamp01(Mathf.InverseLerp(0.18f, 0.02f, Mathf.Abs(Mathf.Sin((v + 0.58f) * 11f))));
            float sidePanels = Mathf.Clamp01(Mathf.InverseLerp(0.1f, 0.32f, Mathf.Abs(u))) * Mathf.Clamp01(Mathf.InverseLerp(-0.18f, 0.42f, v)) * 0.6f;
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(spinePanels * 0.6f + sidePanels * 1.1f),
                ShipId.Heavy => Mathf.Clamp01(spinePanels * 0.8f + Mathf.Clamp01(1f - Mathf.Abs(v + 0.08f) * 6f) * Mathf.Clamp01(Mathf.InverseLerp(0.18f, 0.42f, Mathf.Abs(u))) * 1.2f),
                _ => Mathf.Clamp01(spinePanels + sidePanels * 0.82f)
            };
        }

        private static float EvaluateShipTrimMask(ShipId shipId, float u, float v)
        {
            float noseLine = Mathf.Clamp01(1f - Mathf.Abs(u) * 12f) * Mathf.Clamp01(Mathf.InverseLerp(0.3f, 0.88f, v));
            float shoulderTrim = Mathf.Clamp01(Mathf.InverseLerp(0.22f, 0.38f, Mathf.Abs(u))) * Mathf.Clamp01(Mathf.InverseLerp(-0.08f, 0.3f, v));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(noseLine * 0.45f + shoulderTrim * 0.9f),
                ShipId.Heavy => Mathf.Clamp01(noseLine * 0.24f + shoulderTrim * 0.42f),
                _ => Mathf.Clamp01(noseLine * 0.62f + shoulderTrim * 0.48f)
            };
        }

        private static float EvaluateShipEngineMask(ShipId shipId, float u, float v)
        {
            float left = Mathf.Clamp01(1f - (((u + 0.18f) * (u + 0.18f)) / 0.015f + ((v + 0.74f) * (v + 0.74f)) / 0.024f));
            float right = Mathf.Clamp01(1f - (((u - 0.18f) * (u - 0.18f)) / 0.015f + ((v + 0.74f) * (v + 0.74f)) / 0.024f));
            float center = Mathf.Clamp01(1f - ((u * u) / 0.02f + ((v + 0.72f) * (v + 0.72f)) / 0.03f));
            return shipId switch
            {
                ShipId.Rapid => Mathf.Clamp01(left + right),
                ShipId.Heavy => Mathf.Clamp01(center + (left + right) * 0.4f),
                _ => Mathf.Clamp01((left + right) * 0.82f + center * 0.28f)
            };
        }

        private static float BoxMask(float u, float v, float halfWidth, float halfHeight)
        {
            return Mathf.Clamp01(Mathf.InverseLerp(0.03f, -0.03f, Mathf.Max(Mathf.Abs(u) - halfWidth, Mathf.Abs(v) - halfHeight)));
        }

        private static Color GetShipHullColor(ShipId shipId)
        {
            return shipId switch
            {
                ShipId.Rapid => new Color(0.58f, 0.64f, 0.72f, 1f),
                ShipId.Heavy => new Color(0.46f, 0.46f, 0.5f, 1f),
                _ => new Color(0.56f, 0.62f, 0.72f, 1f)
            };
        }

        private static Color GetShipAccentColor(ShipId shipId)
        {
            return shipId switch
            {
                ShipId.Rapid => new Color(1f, 0.72f, 0.26f, 1f),
                ShipId.Heavy => new Color(1f, 0.42f, 0.18f, 1f),
                _ => new Color(0.42f, 0.9f, 1f, 1f)
            };
        }

        private static Color GetShipCanopyColor(ShipId shipId)
        {
            return shipId switch
            {
                ShipId.Rapid => new Color(1f, 0.9f, 0.58f, 0.94f),
                ShipId.Heavy => new Color(1f, 0.78f, 0.42f, 0.92f),
                _ => new Color(0.72f, 0.94f, 1f, 0.94f)
            };
        }

        private static void BlendPixel(Color[] pixels, int x, int y, Color source)
        {
            int index = y * TextureSize + x;
            Color dst = pixels[index];
            float alpha = source.a + dst.a * (1f - source.a);
            if (alpha <= 0.0001f)
            {
                pixels[index] = Color.clear;
                return;
            }

            Color rgb = (source * source.a + dst * dst.a * (1f - source.a)) / alpha;
            rgb.a = alpha;
            pixels[index] = rgb;
        }
    }
}
