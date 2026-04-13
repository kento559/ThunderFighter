using UnityEngine;

namespace ThunderFighter.Spawning
{
    public class BackgroundMeteor : MonoBehaviour
    {
        private Vector3 velocity;
        private float spin;
        private float lifetime;
        private float age;
        private SpriteRenderer spriteRenderer;
        private Color baseColor;

        public void Setup(Vector3 moveVelocity, float angularVelocity, float duration)
        {
            velocity = moveVelocity;
            spin = angularVelocity;
            lifetime = Mathf.Max(0.2f, duration);
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(0f, 0f, spin * Time.deltaTime);

            if (spriteRenderer != null)
            {
                float fade = 1f - Mathf.Clamp01(age / lifetime);
                spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * fade);
            }

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
