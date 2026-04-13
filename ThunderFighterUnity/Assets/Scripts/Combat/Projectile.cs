using ThunderFighter.Boss;
using ThunderFighter.Config;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private enum ImpactProfile
        {
            Default,
            EliteEnemy,
            SupportEnemy,
            BossShield,
            BossBattery,
            BossCore
        }

        [SerializeField] private int damage = 1;
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private Faction faction = Faction.Player;

        private float expireAt;
        private Color visualTint = Color.white;
        private Vector3 visualScale = Vector3.one;
        private bool hasVisualOverride;
        private float nextTrailAt;
        private ShipArchetype sourceArchetype = ShipArchetype.Balanced;
        private int sourceWeaponLevel = 1;

        public Faction Faction => faction;
        public ShipArchetype SourceArchetype => sourceArchetype;
        public int SourceWeaponLevel => sourceWeaponLevel;

        private void Awake()
        {
            VisualDebugSprite.Ensure(gameObject, faction == Faction.Player ? Color.cyan : new Color(1f, 0.35f, 0.35f, 1f), 90, 0.38f, GeneratedSpriteKind.Bullet);
            ApplyArtSprite();
        }

        private void OnEnable()
        {
            expireAt = Time.time + lifetime;
            nextTrailAt = Time.time + 0.02f;
        }

        private void Update()
        {
            transform.Translate(Vector3.up * (speed * Time.deltaTime), Space.Self);
            UpdateTrail();

            if (Time.time >= expireAt)
            {
                Deactivate();
            }
        }

        public void Setup(Faction ownerFaction, int projectileDamage, float projectileSpeed)
        {
            faction = ownerFaction;
            damage = projectileDamage;
            speed = projectileSpeed;
            hasVisualOverride = false;
            visualTint = Color.white;
            visualScale = Vector3.one;
            sourceArchetype = ShipArchetype.Balanced;
            sourceWeaponLevel = 1;

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.white;
                renderer.sortingOrder = 90;
            }

            ApplyArtSprite();
        }

        public void SetSourceProfile(ShipArchetype archetype, int weaponLevel)
        {
            sourceArchetype = archetype;
            sourceWeaponLevel = Mathf.Clamp(weaponLevel, 1, 4);
        }

        public void SetVisualOverride(Color tint, Vector3 scaleMultiplier)
        {
            hasVisualOverride = true;
            visualTint = tint;
            visualScale = scaleMultiplier;
            ApplyArtSprite();
        }

        private void ApplyArtSprite()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                return;
            }

            Sprite sprite = RuntimeArtLibrary.Get(faction == Faction.Player ? RuntimeArtSpriteId.PlayerBullet : RuntimeArtSpriteId.EnemyBullet);
            if (sprite != null)
            {
                renderer.sprite = sprite;
            }

            Vector3 baseScale = faction == Faction.Player
                ? new Vector3(0.6f, 1.06f, 1f)
                : new Vector3(0.66f, 1.14f, 1f);
            renderer.color = hasVisualOverride ? visualTint : Color.white;
            transform.localScale = hasVisualOverride
                ? new Vector3(baseScale.x * visualScale.x, baseScale.y * visualScale.y, 1f)
                : baseScale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HealthComponent health = other.GetComponent<HealthComponent>();
            if (health != null && health.Faction == faction && faction != Faction.Neutral)
            {
                return;
            }

            IDamageable target = other.GetComponent<IDamageable>();
            if (target == null)
            {
                return;
            }

            SpawnImpactEffect(other.bounds.ClosestPoint(transform.position), ResolveImpactProfile(other));
            target.TakeDamage(damage, new DamageSource(gameObject, faction));
            Deactivate();
        }

        private void UpdateTrail()
        {
            if (Time.time < nextTrailAt)
            {
                return;
            }

            nextTrailAt = Time.time + (faction == Faction.Player ? 0.028f : 0.04f);
            GameObject trail = new GameObject(faction == Faction.Player ? "_PlayerBulletTrail" : "_EnemyBulletTrail");
            trail.transform.position = transform.position;
            trail.transform.rotation = transform.rotation;
            SpriteRenderer renderer = trail.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(faction == Faction.Player ? RuntimeArtSpriteId.PlayerBullet : RuntimeArtSpriteId.EnemyBullet) ?? GetComponent<SpriteRenderer>()?.sprite;
            renderer.color = faction == Faction.Player ? new Color(0.52f, 0.92f, 1f, 0.18f) : new Color(1f, 0.42f, 0.42f, 0.16f);
            renderer.sortingOrder = 88;
            trail.AddComponent<TransientSpriteEffect>().Setup(
                transform.localScale,
                new Vector3(transform.localScale.x * 0.4f, transform.localScale.y * 0.72f, 1f),
                renderer.color,
                new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f),
                faction == Faction.Player ? 0.1f : 0.12f,
                88);
        }

        private ImpactProfile ResolveImpactProfile(Collider2D other)
        {
            if (faction != Faction.Player)
            {
                return ImpactProfile.Default;
            }

            BossDamageNode bossNode = other.GetComponent<BossDamageNode>();
            if (bossNode != null)
            {
                return bossNode.NodeType switch
                {
                    BossNodeType.Core => ImpactProfile.BossCore,
                    BossNodeType.LeftBattery => ImpactProfile.BossBattery,
                    BossNodeType.RightBattery => ImpactProfile.BossBattery,
                    _ => ImpactProfile.BossShield
                };
            }

            Enemy.EnemyController enemy = other.GetComponent<Enemy.EnemyController>();
            if (enemy != null && enemy.IsSupportVariant)
            {
                return ImpactProfile.SupportEnemy;
            }

            if (enemy != null && enemy.IsEliteVariant)
            {
                return ImpactProfile.EliteEnemy;
            }

            return ImpactProfile.Default;
        }

        private void SpawnImpactEffect(Vector3 hitPosition, ImpactProfile profile)
        {
            Color flashColor = faction == Faction.Player ? new Color(0.92f, 1f, 1f, 0.94f) : new Color(1f, 0.78f, 0.62f, 0.9f);
            Color ringColor = faction == Faction.Player ? new Color(0.54f, 0.94f, 1f, 0.72f) : new Color(1f, 0.62f, 0.32f, 0.64f);
            Vector3 flashTargetScale = faction == Faction.Player ? new Vector3(0.72f, 0.72f, 1f) : new Vector3(0.64f, 0.64f, 1f);
            Vector3 ringTargetScale = new Vector3(0.74f, 0.74f, 1f);
            float ringLifetime = 0.16f;

            switch (profile)
            {
                case ImpactProfile.EliteEnemy:
                    flashColor = new Color(1f, 0.92f, 0.62f, 0.98f);
                    ringColor = new Color(1f, 0.72f, 0.24f, 0.74f);
                    flashTargetScale = new Vector3(0.9f, 0.9f, 1f);
                    ringTargetScale = new Vector3(0.92f, 0.92f, 1f);
                    ringLifetime = 0.18f;
                    break;
                case ImpactProfile.SupportEnemy:
                    flashColor = new Color(0.76f, 1f, 0.9f, 0.98f);
                    ringColor = new Color(0.32f, 1f, 0.82f, 0.74f);
                    flashTargetScale = new Vector3(0.94f, 0.94f, 1f);
                    ringTargetScale = new Vector3(0.98f, 0.98f, 1f);
                    ringLifetime = 0.18f;
                    break;
                case ImpactProfile.BossShield:
                    flashColor = new Color(0.52f, 0.9f, 1f, 0.9f);
                    ringColor = new Color(0.24f, 0.68f, 1f, 0.78f);
                    flashTargetScale = new Vector3(0.82f, 0.82f, 1f);
                    ringTargetScale = new Vector3(1.02f, 1.02f, 1f);
                    ringLifetime = 0.2f;
                    break;
                case ImpactProfile.BossBattery:
                    flashColor = new Color(1f, 0.86f, 0.56f, 0.96f);
                    ringColor = new Color(1f, 0.58f, 0.22f, 0.82f);
                    flashTargetScale = new Vector3(0.96f, 0.96f, 1f);
                    ringTargetScale = new Vector3(1.08f, 1.08f, 1f);
                    ringLifetime = 0.2f;
                    break;
                case ImpactProfile.BossCore:
                    flashColor = new Color(1f, 0.98f, 0.9f, 1f);
                    ringColor = new Color(1f, 0.5f, 0.18f, 0.82f);
                    flashTargetScale = new Vector3(1.04f, 1.04f, 1f);
                    ringTargetScale = new Vector3(1.16f, 1.16f, 1f);
                    ringLifetime = 0.22f;
                    break;
            }

            if (faction == Faction.Player)
            {
                if (sourceArchetype == ShipArchetype.Heavy && (profile == ImpactProfile.BossShield || profile == ImpactProfile.BossBattery || profile == ImpactProfile.BossCore))
                {
                    ringTargetScale *= 1.12f;
                    flashTargetScale *= 1.08f;
                }
                else if (sourceArchetype == ShipArchetype.Rapid && (profile == ImpactProfile.EliteEnemy || profile == ImpactProfile.SupportEnemy))
                {
                    ringLifetime += 0.03f;
                    flashTargetScale *= 1.04f;
                }
                else if (sourceArchetype == ShipArchetype.Balanced && sourceWeaponLevel >= 4)
                {
                    ringTargetScale *= 1.04f;
                }
            }

            GameObject flash = new GameObject(faction == Faction.Player ? "_PlayerImpactFlash" : "_EnemyImpactFlash");
            flash.transform.position = hitPosition;
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            renderer.color = flashColor;
            flash.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.16f, 0.16f, 1f),
                flashTargetScale,
                renderer.color,
                new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f),
                faction == Faction.Player ? 0.12f : 0.14f,
                160);

            GameObject ring = new GameObject(faction == Faction.Player ? "_PlayerImpactRing" : "_EnemyImpactRing");
            ring.transform.position = hitPosition;
            SpriteRenderer ringRenderer = ring.AddComponent<SpriteRenderer>();
            ringRenderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            ringRenderer.color = ringColor;
            ring.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.12f, 0.12f, 1f),
                ringTargetScale,
                ringRenderer.color,
                new Color(ringRenderer.color.r, ringRenderer.color.g, ringRenderer.color.b, 0f),
                ringLifetime,
                159);

            int sparkCount = profile == ImpactProfile.BossCore ? 5 : profile == ImpactProfile.BossShield || profile == ImpactProfile.BossBattery ? 4 : profile == ImpactProfile.EliteEnemy || profile == ImpactProfile.SupportEnemy ? 3 : 2;
            float sparkScale = profile == ImpactProfile.BossCore ? 0.42f : profile == ImpactProfile.BossShield || profile == ImpactProfile.BossBattery ? 0.34f : 0.26f;
            SpawnImpactSparks(hitPosition, ringColor, sparkCount, sparkScale);
        }

        private void SpawnImpactSparks(Vector3 hitPosition, Color color, int count, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject spark = new GameObject("_ImpactSpark");
                spark.transform.position = hitPosition;
                spark.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                SpriteRenderer sparkRenderer = spark.AddComponent<SpriteRenderer>();
                sparkRenderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
                sparkRenderer.color = new Color(color.r, color.g, color.b, 0.82f);
                float targetScale = scale * Random.Range(0.85f, 1.2f);
                spark.AddComponent<TransientSpriteEffect>().Setup(
                    new Vector3(0.08f, 0.16f, 1f),
                    new Vector3(targetScale, targetScale * 1.6f, 1f),
                    sparkRenderer.color,
                    new Color(color.r, color.g, color.b, 0f),
                    0.1f + i * 0.015f,
                    161);
            }
        }

        private void Deactivate()
        {
            if (!ProjectilePool.TryRecycle(this))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
