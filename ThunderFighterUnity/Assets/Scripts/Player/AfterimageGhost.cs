using UnityEngine;

namespace ThunderFighter.Player
{
    public class AfterimageGhost : MonoBehaviour
    {
        private float lifetime;
        private float age;
        private SpriteRenderer spriteRenderer;
        private Color baseColor;
        private Vector3 drift;

        public void Setup(float duration, Vector3 driftVelocity, int sortingOrder, Color color)
        {
            lifetime = Mathf.Max(0.05f, duration);
            drift = driftVelocity;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = sortingOrder;
                spriteRenderer.color = color;
                baseColor = color;
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += drift * Time.deltaTime;

            if (spriteRenderer != null)
            {
                float t = Mathf.Clamp01(age / lifetime);
                spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(baseColor.a, 0f, t));
            }

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
