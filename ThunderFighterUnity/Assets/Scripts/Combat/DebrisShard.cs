using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Combat
{
    public class DebrisShard : MonoBehaviour
    {
        private Vector3 velocity;
        private float angularVelocity;
        private float lifetime;
        private float age;
        private SpriteRenderer spriteRenderer;
        private Color baseColor = Color.white;
        private TrailRenderer smokeTrail;
        private TrailRenderer sparkTrail;
        private SpriteRenderer shadowRenderer;
        private Vector3 shadowOffset;

        public void Setup(Vector3 initialVelocity, float spin, float duration, int sortingOrder)
        {
            velocity = initialVelocity;
            angularVelocity = spin;
            lifetime = Mathf.Max(0.1f, duration);
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = sortingOrder;
                baseColor = spriteRenderer.color;
            }

            shadowOffset = new Vector3(0.06f, -0.08f, 0f);
            EnsureShadow(sortingOrder - 2);
            EnsureTrailRenderers(sortingOrder - 1, sortingOrder);
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 1.8f);

            if (spriteRenderer != null)
            {
                float alpha = 1f - Mathf.Clamp01(age / lifetime);
                spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            }

            if (shadowRenderer != null)
            {
                float alpha = (1f - Mathf.Clamp01(age / lifetime)) * 0.28f;
                shadowRenderer.transform.position = transform.position + shadowOffset;
                shadowRenderer.color = new Color(0f, 0f, 0f, alpha);
            }

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void EnsureTrailRenderers(int smokeOrder, int sparkOrder)
        {
            smokeTrail = gameObject.AddComponent<TrailRenderer>();
            smokeTrail.time = Mathf.Max(0.12f, lifetime * 0.75f);
            smokeTrail.startWidth = 0.14f;
            smokeTrail.endWidth = 0.03f;
            smokeTrail.material = new Material(Shader.Find("Sprites/Default"));
            smokeTrail.sortingOrder = smokeOrder;
            smokeTrail.autodestruct = false;
            smokeTrail.numCapVertices = 4;
            smokeTrail.startColor = new Color(0.24f, 0.22f, 0.22f, 0.34f);
            smokeTrail.endColor = new Color(0.1f, 0.1f, 0.1f, 0f);

            GameObject sparkObject = new GameObject("_SparkTrail");
            sparkObject.transform.SetParent(transform, false);
            sparkObject.transform.localPosition = Vector3.zero;
            sparkTrail = sparkObject.AddComponent<TrailRenderer>();
            sparkTrail.time = Mathf.Max(0.06f, lifetime * 0.35f);
            sparkTrail.startWidth = 0.05f;
            sparkTrail.endWidth = 0.015f;
            sparkTrail.material = new Material(Shader.Find("Sprites/Default"));
            sparkTrail.sortingOrder = sparkOrder;
            sparkTrail.autodestruct = false;
            sparkTrail.numCapVertices = 3;
            sparkTrail.startColor = new Color(1f, 0.84f, 0.46f, 0.92f);
            sparkTrail.endColor = new Color(1f, 0.3f, 0.06f, 0f);
        }

        private void EnsureShadow(int sortingOrder)
        {
            GameObject shadow = new GameObject("_DebrisShadow");
            shadow.transform.SetParent(transform, false);
            shadow.transform.localPosition = shadowOffset;
            shadow.transform.localScale = new Vector3(0.72f, 0.22f, 1f);
            shadowRenderer = VisualDebugSprite.Ensure(shadow, new Color(0f, 0f, 0f, 0.28f), sortingOrder, 0.2f, GeneratedSpriteKind.Hull);
            if (shadowRenderer != null)
            {
                shadowRenderer.color = new Color(0f, 0f, 0f, 0.28f);
            }
        }
    }
}
