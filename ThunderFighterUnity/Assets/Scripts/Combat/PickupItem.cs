using ThunderFighter.Config;
using ThunderFighter.Core;
using ThunderFighter.Player;
using UnityEngine;

namespace ThunderFighter.Combat
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class PickupItem : MonoBehaviour
    {
        private PickupDefinition definition;
        private PlayerController cachedPlayer;
        private CircleCollider2D trigger;
        private float spawnTime;
        private Vector3 origin;
        private float magnetRadius = 1.8f;
        private SpriteRenderer iconRenderer;
        private Transform outerRing;
        private Transform innerRing;
        private Transform badge;
        private float nextTrailAt;
        private static readonly System.Collections.Generic.Dictionary<PickupKind, AudioClip> PickupClips = new System.Collections.Generic.Dictionary<PickupKind, AudioClip>();

        public void Initialize(PickupDefinition pickupDefinition)
        {
            definition = pickupDefinition;
            spawnTime = Time.time;
            origin = transform.position;
            trigger = GetComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.32f;

            iconRenderer = gameObject.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = GeneratedSpriteLibrary.Get(definition.IconKind);
            iconRenderer.color = definition.AccentColor;
            iconRenderer.sortingOrder = 150;
            transform.localScale = new Vector3(0.58f, 0.58f, 1f);

            GameObject glow = new GameObject("_PickupGlow");
            glow.transform.SetParent(transform, false);
            SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            glowRenderer.color = new Color(definition.AccentColor.r, definition.AccentColor.g, definition.AccentColor.b, 0.22f);
            glowRenderer.sortingOrder = 147;
            glow.transform.localScale = new Vector3(1.18f, 1.18f, 1f);

            outerRing = CreateRing("_PickupOuterRing", new Color(definition.AccentColor.r, definition.AccentColor.g, definition.AccentColor.b, 0.42f), 149, 0.96f);
            innerRing = CreateRing("_PickupInnerRing", new Color(1f, 1f, 1f, 0.54f), 151, 0.68f);
            badge = CreateBadge();
            ApplyPickupPresentation();
        }

        private void Update()
        {
            if (definition == null)
            {
                Destroy(gameObject);
                return;
            }

            if (cachedPlayer == null)
            {
                cachedPlayer = Object.FindFirstObjectByType<PlayerController>();
            }

            float bob = Mathf.Sin((Time.time - spawnTime) * 4.2f) * 0.12f;
            transform.position = new Vector3(transform.position.x, transform.position.y - Time.deltaTime * 0.8f, 0f);
            transform.position = new Vector3(transform.position.x, origin.y + bob - (Time.time - spawnTime) * 0.8f, 0f);
            transform.Rotate(0f, 0f, 28f * Time.deltaTime);
            UpdateRings();

            if (cachedPlayer != null)
            {
                float radius = magnetRadius * PlayerControllerPickupExtensions.GetPickupMagnetMultiplier(cachedPlayer);
                float distance = Vector2.Distance(transform.position, cachedPlayer.transform.position);
                if (distance <= radius)
                {
                    if (Time.time >= nextTrailAt)
                    {
                        nextTrailAt = Time.time + 0.035f;
                        SpawnAttractTrail(cachedPlayer.transform.position);
                    }

                    transform.position = Vector3.MoveTowards(transform.position, cachedPlayer.transform.position, Time.deltaTime * (6f + radius * 2.4f));
                }
            }

            if (transform.position.y < -6.4f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || definition == null)
            {
                return;
            }

            PlayPickupSfx(definition.Kind);
            SpawnPickupLabel(player.transform.position, definition.GetShortLabel(LocalizationService.IsChinese), definition.AccentColor);
            PlayerControllerPickupExtensions.ApplyPickup(player, definition);
            Destroy(gameObject);
        }

        private Transform CreateRing(string name, Color color, int sortingOrder, float scale)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return go.transform;
        }

        private Transform CreateBadge()
        {
            GameObject go = new GameObject("_PickupBadge");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.46f, 0f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = ResolveBadgeSprite(definition.Kind);
            renderer.color = Color.white;
            renderer.sortingOrder = 152;
            go.transform.localScale = new Vector3(0.46f, 0.46f, 1f);
            return go.transform;
        }

        private void ApplyPickupPresentation()
        {
            if (iconRenderer == null || definition == null)
            {
                return;
            }

            Vector3 iconScale = definition.Kind switch
            {
                PickupKind.WeaponLevel => new Vector3(0.7f, 0.7f, 1f),
                PickupKind.Repair => new Vector3(0.62f, 0.62f, 1f),
                PickupKind.SkillEnergy => new Vector3(0.64f, 0.64f, 1f),
                _ => new Vector3(0.58f, 0.58f, 1f)
            };

            transform.localScale = iconScale;
            magnetRadius = definition.Kind == PickupKind.WeaponLevel ? 2.1f : 1.8f;
        }

        private void UpdateRings()
        {
            float t = Time.time - spawnTime;
            if (outerRing != null)
            {
                outerRing.Rotate(0f, 0f, 82f * Time.deltaTime);
                float outerScale = 0.92f + Mathf.Sin(t * 3.4f) * 0.06f;
                outerRing.localScale = new Vector3(outerScale, outerScale, 1f);
            }

            if (innerRing != null)
            {
                innerRing.Rotate(0f, 0f, -126f * Time.deltaTime);
                float innerScale = 0.66f + Mathf.Sin(t * 5.2f + 0.9f) * 0.05f;
                innerRing.localScale = new Vector3(innerScale, innerScale, 1f);
            }

            if (badge != null)
            {
                badge.localScale = Vector3.one * (0.42f + Mathf.Sin(t * 4.6f) * 0.03f);
            }
        }

        private static Sprite ResolveBadgeSprite(PickupKind kind)
        {
            return kind switch
            {
                PickupKind.WeaponLevel => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Nose),
                PickupKind.Repair => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Cockpit),
                PickupKind.SkillEnergy => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring),
                PickupKind.FireRateBuff => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash),
                PickupKind.DamageBuff => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Wing),
                PickupKind.ProjectileSpeedBuff => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Bullet),
                PickupKind.MagnetBuff => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Engine),
                PickupKind.GuardBuff => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Hull),
                _ => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring)
            };
        }

        private static void PlayPickupSfx(PickupKind kind)
        {
            if (!PickupClips.TryGetValue(kind, out AudioClip clip) || clip == null)
            {
                clip = BuildPickupClip(kind);
                PickupClips[kind] = clip;
            }

            if (clip == null)
            {
                return;
            }

            GameObject temp = new GameObject("_PickupSfx");
            AudioSource source = temp.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = GameSettingsService.SfxVolume;
            source.clip = clip;
            source.Play();
            Object.Destroy(temp, Mathf.Max(clip.length, 0.3f));
        }

        private void SpawnAttractTrail(Vector3 targetPosition)
        {
            Vector3 delta = targetPosition - transform.position;
            float length = delta.magnitude;
            if (length <= 0.08f)
            {
                return;
            }

            GameObject trail = new GameObject("_PickupAttractTrail");
            trail.transform.position = transform.position + delta * 0.5f;
            trail.transform.rotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            SpriteRenderer renderer = trail.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Bullet);
            renderer.color = new Color(definition.AccentColor.r, definition.AccentColor.g, definition.AccentColor.b, 0.34f);
            trail.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.1f, length * 0.32f, 1f),
                new Vector3(0.04f, length * 0.92f, 1f),
                renderer.color,
                new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f),
                0.09f,
                148);
        }

        private static void SpawnPickupLabel(Vector3 position, string label, Color color)
        {
            GameObject go = new GameObject("_PickupLabel");
            go.transform.position = position + Vector3.up * 0.55f;
            TextMesh mesh = go.AddComponent<TextMesh>();
            mesh.text = label;
            mesh.fontSize = 36;
            mesh.characterSize = 0.06f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.Lerp(color, Color.white, 0.28f);
            mesh.fontStyle = FontStyle.Bold;
            PickupFloatingLabel floating = go.AddComponent<PickupFloatingLabel>();
            floating.Setup(new Vector3(0f, 0.75f, 0f), 0.55f, mesh.color);
        }

        private static AudioClip BuildPickupClip(PickupKind kind)
        {
            float start = kind switch
            {
                PickupKind.WeaponLevel => 460f,
                PickupKind.Repair => 320f,
                PickupKind.SkillEnergy => 540f,
                PickupKind.FireRateBuff => 760f,
                PickupKind.DamageBuff => 420f,
                PickupKind.ProjectileSpeedBuff => 620f,
                PickupKind.MagnetBuff => 500f,
                PickupKind.GuardBuff => 380f,
                _ => 440f
            };
            float end = start + (kind == PickupKind.WeaponLevel ? 680f : 360f);
            int sampleRate = 44100;
            float duration = kind == PickupKind.WeaponLevel ? 0.22f : 0.14f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float freq = Mathf.Lerp(start, end, t);
                float envelope = Mathf.Sin(t * Mathf.PI) * (kind == PickupKind.WeaponLevel ? 0.34f : 0.24f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * envelope;
            }

            AudioClip clip = AudioClip.Create("pickup-" + kind, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }

    public class PickupFloatingLabel : MonoBehaviour
    {
        private Vector3 velocity;
        private float lifetime;
        private float spawnTime;
        private Color baseColor;
        private TextMesh textMesh;

        public void Setup(Vector3 moveVelocity, float duration, Color color)
        {
            velocity = moveVelocity;
            lifetime = duration;
            spawnTime = Time.time;
            baseColor = color;
            textMesh = GetComponent<TextMesh>();
        }

        private void Update()
        {
            transform.position += velocity * Time.deltaTime;
            float t = Mathf.Clamp01((Time.time - spawnTime) / Mathf.Max(0.01f, lifetime));
            if (textMesh != null)
            {
                textMesh.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);
            }

            if (Time.time - spawnTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
