using UnityEngine;

namespace ThunderFighter.Combat
{
    public class TransientSpriteEffect : MonoBehaviour
    {
        private Vector3 startScale = Vector3.one;
        private Vector3 endScale = Vector3.one;
        private Color startColor = Color.white;
        private Color endColor = new Color(1f, 1f, 1f, 0f);
        private float duration = 0.2f;
        private float age;
        private SpriteRenderer spriteRenderer;

        public void Setup(Vector3 fromScale, Vector3 toScale, Color fromColor, Color toColor, float lifetime, int sortingOrder)
        {
            startScale = fromScale;
            endScale = toScale;
            startColor = fromColor;
            endColor = toColor;
            duration = Mathf.Max(0.02f, lifetime);
            transform.localScale = startScale;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = startColor;
                spriteRenderer.sortingOrder = sortingOrder;
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            transform.localScale = Vector3.Lerp(startScale, endScale, eased);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(startColor, endColor, eased);
            }

            if (age >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
