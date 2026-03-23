using UnityEngine;

namespace ThunderFighter.Core
{
    public class ThrusterFlameAnimator : MonoBehaviour
    {
        private SpriteRenderer glowRenderer;
        private SpriteRenderer coreRenderer;
        private float directionSign = -1f;
        private float baseLength = 0.36f;
        private float baseWidth = 0.16f;
        private float boost = 0.5f;
        private float flickerOffset;
        private Color glowBaseColor;
        private Color coreBaseColor;

        public void Setup(float flameDirectionSign, int sortingOrder, Color glowColor, Color coreColor)
        {
            directionSign = Mathf.Approximately(flameDirectionSign, 0f) ? -1f : Mathf.Sign(flameDirectionSign);
            flickerOffset = Random.Range(0f, 10f);
            glowBaseColor = glowColor;
            coreBaseColor = coreColor;

            glowRenderer = CreateLayer("_FlameGlow", sortingOrder, glowColor, 0.26f, 0.72f);
            coreRenderer = CreateLayer("_FlameCore", sortingOrder + 1, coreColor, 0.14f, 0.48f);
            Apply(0.75f);
        }

        public void SetBoost(float intensity)
        {
            boost = Mathf.Clamp01(intensity);
        }

        public void SetPalette(Color glowColor, Color coreColor)
        {
            glowBaseColor = glowColor;
            coreBaseColor = coreColor;
            Apply(0.75f + boost * 0.45f);
        }

        private void Update()
        {
            float pulse = 0.7f + Mathf.Sin((Time.time * 22f) + flickerOffset) * 0.18f;
            float flicker = 0.15f + Mathf.PerlinNoise(flickerOffset, Time.time * 8f) * 0.3f;
            Apply(Mathf.Clamp01(pulse + flicker + boost * 0.45f));
        }

        private void Apply(float strength)
        {
            if (glowRenderer != null)
            {
                glowRenderer.transform.localPosition = new Vector3(0f, directionSign * (0.08f + strength * 0.08f), 0f);
                glowRenderer.transform.localScale = new Vector3(baseWidth * (1.2f + boost * 0.4f), baseLength * (0.9f + strength * 0.8f), 1f);
                glowRenderer.color = new Color(glowBaseColor.r, glowBaseColor.g, glowBaseColor.b, 0.42f + boost * 0.18f);
            }

            if (coreRenderer != null)
            {
                coreRenderer.transform.localPosition = new Vector3(0f, directionSign * (0.05f + strength * 0.06f), 0f);
                coreRenderer.transform.localScale = new Vector3(baseWidth * (0.72f + boost * 0.18f), baseLength * (0.66f + strength * 0.62f), 1f);
                coreRenderer.color = new Color(coreBaseColor.r, coreBaseColor.g, coreBaseColor.b, 0.85f);
            }
        }

        private SpriteRenderer CreateLayer(string name, int order, Color color, float width, float length)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, directionSign * 0.06f, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = new Vector3(width, length, 1f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Thruster);
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
