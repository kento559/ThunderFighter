using ThunderFighter.Combat;
using ThunderFighter.Config;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Enemy
{
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private WeaponController weapon;

        private HealthComponent health;
        private float shootAt;
        private Vector3 spawnPosition;
        private float movementSeed;
        private EnemyBehaviorType behaviorType;
        private ProceduralShipVisual shipVisual;
        private bool usingEliteVariant;
        private int chapterIndex = 1;
        public bool IsEliteVariant => usingEliteVariant;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            if (health != null)
            {
                health.OnDied += HandleDied;
            }

            SpriteRenderer baseRenderer = VisualDebugSprite.Ensure(gameObject, new Color(0.45f, 0.12f, 0.12f, 0.28f), 4, 0.8f);
            if (baseRenderer != null)
            {
                baseRenderer.sortingOrder = 4;
            }

            shipVisual = ProceduralShipVisual.Ensure(gameObject, ProceduralShipStyle.Enemy, 60);
            behaviorType = ResolveBehaviorType();
            usingEliteVariant = ResolveEliteVariant();
            chapterIndex = Mathf.Clamp(CampaignRuntime.CurrentLevel != null ? CampaignRuntime.CurrentLevel.ChapterIndex : 1, 1, 3);
            ApplyArtVariant();
            ApplyChapterVisuals();
            if (RuntimeArtLibrary.Get(usingEliteVariant ? RuntimeArtSpriteId.EnemyShipElite : RuntimeArtSpriteId.EnemyShip) != null && baseRenderer != null)
            {
                baseRenderer.enabled = false;
            }

            if (GetComponent<DamageFeedback>() == null)
            {
                gameObject.AddComponent<DamageFeedback>();
            }

            spawnPosition = transform.position;
            movementSeed = Random.Range(0f, 20f);
            shootAt = Time.time + Random.Range(0.2f, 0.8f);
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        private void Update()
        {
            if (config == null)
            {
                return;
            }

            Vector3 position = transform.position;
            float moveSpeed = config.MoveSpeed;

            if (behaviorType == EnemyBehaviorType.Strafe)
            {
                float offsetX = Mathf.Sin((Time.time + movementSeed) * config.StrafeFrequency) * config.StrafeAmplitude;
                switch (chapterIndex)
                {
                    case 2:
                        offsetX += Mathf.Cos((Time.time + movementSeed * 0.5f) * (config.StrafeFrequency * 0.6f)) * config.StrafeAmplitude * 0.45f;
                        moveSpeed *= 1.15f + 0.25f * Mathf.Abs(Mathf.Sin((Time.time + movementSeed) * 1.9f));
                        break;
                    case 3:
                        offsetX += Mathf.Sin((Time.time + movementSeed) * (config.StrafeFrequency * 1.8f)) * config.StrafeAmplitude * 0.32f;
                        moveSpeed *= 0.92f + 0.22f * Mathf.PingPong(Time.time * 0.9f, 1f);
                        break;
                    default:
                        offsetX += Mathf.Sin((Time.time + movementSeed) * (config.StrafeFrequency * 0.5f)) * config.StrafeAmplitude * 0.18f;
                        break;
                }

                position.x = spawnPosition.x + offsetX;
            }
            else if (chapterIndex == 3 && !usingEliteVariant)
            {
                position.x = spawnPosition.x + Mathf.Sin((Time.time + movementSeed) * 2.4f) * 0.35f;
            }

            position.y -= moveSpeed * Time.deltaTime;

            transform.position = position;
            float entryBoost = Mathf.InverseLerp(-1.5f, 6.5f, transform.position.y);
            float strafeBoost = behaviorType == EnemyBehaviorType.Strafe ? 0.18f : 0f;
            if (chapterIndex == 2 && usingEliteVariant)
            {
                strafeBoost += 0.12f;
            }
            shipVisual?.SetThrustBoost(Mathf.Clamp01(0.38f + entryBoost * 0.9f + strafeBoost));

            if (config.CanShoot && weapon != null && Time.time >= shootAt)
            {
                float interval = behaviorType == EnemyBehaviorType.Strafe ? config.ShootInterval * 0.85f : config.ShootInterval;
                if (chapterIndex == 3 && usingEliteVariant)
                {
                    interval *= 0.84f;
                }
                shootAt = Time.time + interval;
                FireByChapter();
            }

            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }

        private void HandleDied()
        {
            if (ScoreManager.Instance != null && config != null)
            {
                ScoreManager.Instance.AddKillScore(config.ScoreValue);
            }

            PickupSpawner.SpawnEnemyDrop(transform.position, usingEliteVariant, chapterIndex);

            Destroy(gameObject);
        }

        private EnemyBehaviorType ResolveBehaviorType()
        {
            if (config == null)
            {
                return EnemyBehaviorType.Straight;
            }

            if (config.BehaviorType != EnemyBehaviorType.Auto)
            {
                return config.BehaviorType;
            }

            return config.MaxHp >= 6 || config.ScoreValue >= 300 ? EnemyBehaviorType.Strafe : EnemyBehaviorType.Straight;
        }

        private bool ResolveEliteVariant()
        {
            if (config == null)
            {
                return false;
            }

            return behaviorType == EnemyBehaviorType.Strafe || config.MaxHp >= 6 || config.ScoreValue >= 300;
        }

        private void ApplyArtVariant()
        {
            if (shipVisual == null)
            {
                return;
            }

            if (usingEliteVariant)
            {
                shipVisual.SetArtVariant(
                    RuntimeArtSpriteId.EnemyShipElite,
                    RuntimeArtSpriteId.EnemyShipEliteDamaged,
                    RuntimeArtSpriteId.EnemyEliteFragmentA,
                    RuntimeArtSpriteId.EnemyEliteFragmentB);
            }
            else
            {
                shipVisual.SetArtVariant(
                    RuntimeArtSpriteId.EnemyShip,
                    RuntimeArtSpriteId.EnemyShipDamaged,
                    RuntimeArtSpriteId.EnemyFragmentA,
                    RuntimeArtSpriteId.EnemyFragmentB);
            }
        }

        private void ApplyChapterVisuals()
        {
            if (shipVisual == null)
            {
                return;
            }

            SpriteRenderer renderer = shipVisual.GetPrimaryRenderer();
            switch (chapterIndex)
            {
                case 2:
                    shipVisual.SetThrusterPalette(new Color(1f, 0.54f, 0.14f, 0.62f), new Color(1f, 0.82f, 0.56f, 0.94f));
                    if (renderer != null)
                    {
                        renderer.color = usingEliteVariant ? new Color(1f, 0.92f, 0.92f, 1f) : new Color(1f, 0.96f, 0.94f, 0.96f);
                    }
                    break;
                case 3:
                    shipVisual.SetThrusterPalette(new Color(0.46f, 0.84f, 1f, 0.64f), new Color(0.88f, 0.96f, 1f, 0.98f));
                    if (renderer != null)
                    {
                        renderer.color = usingEliteVariant ? new Color(0.9f, 0.98f, 1f, 1f) : new Color(0.86f, 0.94f, 1f, 0.96f);
                    }
                    break;
                default:
                    shipVisual.SetThrusterPalette(new Color(1f, 0.48f, 0.12f, 0.58f), new Color(1f, 0.8f, 0.5f, 0.94f));
                    if (renderer != null)
                    {
                        renderer.color = Color.white;
                    }
                    break;
            }
        }

        private void FireByChapter()
        {
            if (!usingEliteVariant)
            {
                weapon.TryFire();
                return;
            }

            switch (chapterIndex)
            {
                case 2:
                    weapon.FireBurstPattern(3, 22f, 0f, 0.94f, 1);
                    break;
                case 3:
                    weapon.FireBurstPattern(5, 34f, 0f, 1.08f, 1);
                    break;
                default:
                    weapon.FireBurstPattern(2, 8f, 0f, 1.02f, 0);
                    break;
            }
        }
    }
}
