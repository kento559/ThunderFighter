using System.Collections;
using ThunderFighter.Combat;
using ThunderFighter.Core;
using ThunderFighter.Player;
using UnityEngine;

namespace ThunderFighter.Boss
{
    [RequireComponent(typeof(HealthComponent))]
    public class BossController : MonoBehaviour
    {
        [SerializeField] private BossPhaseConfig[] phases;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private bool usePooling = true;

        private enum BossAttackMode
        {
            Spread = 0,
            AimedBurst = 1,
            SweepingArc = 2,
            SideLances = 3,
            RainVolley = 4,
            OrbitalLattice = 5,
            AsteroidScatter = 6,
            PrismLance = 7
        }

        private int phaseIndex;
        private int announcedPhaseIndex = -1;
        private int appliedVisualPhaseIndex = -1;
        private HealthComponent health;
        private Collider2D rootCollider;
        private float nextFireAt;
        private float moveSeed;
        private float attackPatternSeed;
        private float nextAttackModeChangeAt;
        private float nextShieldToggleAt;
        private float shieldCurrentHp;
        private int coreHitCounter;
        private float nextCoreCounterWindowResetAt;
        private float nextCoreRetaliationAt;
        private BossAttackMode currentAttackMode;
        private BossAttackMode lastAnnouncedAttackMode;
        private bool hasAnnouncedAttackMode;
        private ProceduralShipVisual shipVisual;
        private float targetY;
        private float entranceProgress;
        private bool entering = true;
        private bool shieldActive = true;
        private bool coreRetaliating;
        private AudioSource entranceAudio;
        private int chapterIndex = 1;
        private SpriteRenderer shieldRenderer;
        private SpriteRenderer coreRenderer;
        private Collider2D shieldCollider;
        private Collider2D coreCollider;
        private static AudioClip rumbleClip;
        private static AudioClip warningClip;
        private static AudioClip shieldHitClip;
        private static AudioClip shieldBreakClip;
        private static AudioClip coreExposeClip;
        private static AudioClip laserLockClip;
        private static AudioClip coreChargeClip;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            if (health != null)
            {
                health.OnDied += HandleBossDied;
            }

            rootCollider = GetComponent<Collider2D>();
            if (rootCollider != null)
            {
                rootCollider.enabled = false;
            }

            SpriteRenderer baseRenderer = VisualDebugSprite.Ensure(gameObject, new Color(0.45f, 0.28f, 0.08f, 0.24f), 4, 1.6f);
            if (baseRenderer != null)
            {
                baseRenderer.sortingOrder = 4;
            }

            shipVisual = ProceduralShipVisual.Ensure(gameObject, ProceduralShipStyle.Boss, 70);
            ApplyPhaseArtVariant(0);
            if (RuntimeArtLibrary.Get(RuntimeArtSpriteId.BossShip) != null && baseRenderer != null)
            {
                baseRenderer.enabled = false;
            }

            SetupDefenseNodes();
            ApplyShieldState(true, false);

            if (GetComponent<DamageFeedback>() == null)
            {
                gameObject.AddComponent<DamageFeedback>();
            }

            entranceAudio = GetComponent<AudioSource>();
            if (entranceAudio == null)
            {
                entranceAudio = gameObject.AddComponent<AudioSource>();
            }

            entranceAudio.playOnAwake = false;
            entranceAudio.spatialBlend = 0f;
            EnsureCombatAudio();

            moveSeed = Random.Range(0f, 100f);
            attackPatternSeed = Random.Range(0f, 100f);
            chapterIndex = Mathf.Clamp(CampaignRuntime.CurrentLevel != null ? CampaignRuntime.CurrentLevel.ChapterIndex : 1, 1, 3);
            targetY = transform.position.y;
            transform.position = new Vector3(transform.position.x, targetY + 4.2f, transform.position.z);
            nextShieldToggleAt = Time.time + 2.8f;
            StartCoroutine(PlayEntranceSequence());
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDied -= HandleBossDied;
            }
        }

        private void Update()
        {
            if (phases == null || phases.Length == 0 || health == null)
            {
                return;
            }

            if (entering)
            {
                entranceProgress += Time.deltaTime / 1.25f;
                float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(entranceProgress), 3f);
                float y = Mathf.Lerp(targetY + 4.2f, targetY, eased);
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
                shipVisual?.SetThrustBoost(1f);
                AnimateDefenseNodes();
                if (entranceProgress < 1f)
                {
                    return;
                }

                entering = false;
            }

            UpdatePhaseIndex();
            BossPhaseConfig phase = phases[Mathf.Clamp(phaseIndex, 0, phases.Length - 1)];
            AnnouncePhaseIfNeeded();
            ApplyPhaseArtVariant(phaseIndex);
            UpdateAttackMode();
            UpdateShieldCycle();

            float x = Mathf.Sin((Time.time + moveSeed) * phase.MoveSpeed) * 4.5f;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            float strafeMotion = Mathf.Abs(Mathf.Cos((Time.time + moveSeed) * phase.MoveSpeed));
            shipVisual?.SetThrustBoost(Mathf.Clamp01(0.54f + strafeMotion * 0.28f + phaseIndex * 0.1f));
            AnimateDefenseNodes();

            if (Time.time >= nextFireAt && !coreRetaliating)
            {
                nextFireAt = Time.time + phase.FireInterval;
                ExecuteAttackPattern(phase);
            }

            if (Time.time >= nextCoreCounterWindowResetAt)
            {
                coreHitCounter = 0;
            }

            float ratio = health.MaxHp > 0 ? (float)health.CurrentHp / health.MaxHp : 0f;
            GameEvents.RaiseBossHpChanged(ratio);
        }

        public void ProcessNodeHit(int amount, DamageSource source, bool weakPoint, Vector3 hitPosition)
        {
            if (weakPoint)
            {
                if (shieldActive)
                {
                    SpawnShieldBlockEffect(hitPosition, true);
                    PlayBossSfx(shieldHitClip, 0.2f, 1.18f);
                    return;
                }

                int boostedDamage = Mathf.Max(1, amount + (phaseIndex >= 1 ? 1 : 0));
                SpawnCoreHitEffect(hitPosition);
                PlayBossSfx(coreExposeClip, 0.18f, 1.08f);
                health.TakeDamage(boostedDamage, source);
                coreHitCounter += boostedDamage;
                nextCoreCounterWindowResetAt = Time.time + 3f;
                TryTriggerCoreRetaliation();
                return;
            }

            if (!shieldActive)
            {
                SpawnShieldBlockEffect(hitPosition, false);
                return;
            }

            shieldCurrentHp -= Mathf.Max(1, amount);
            SpawnShieldBlockEffect(hitPosition, false);
            PlayBossSfx(shieldHitClip, 0.22f, 0.96f);
            if (shieldCurrentHp <= 0f)
            {
                BreakShield(hitPosition);
            }
        }

        private IEnumerator PlayEntranceSequence()
        {
            GameEvents.RaiseCombatAnnouncement("WARNING: BOSS APPROACH");
            yield return new WaitForSeconds(0.12f);

            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = Object.FindFirstObjectByType<Camera>();
            }

            CameraShakeController.Ensure(cam)?.Shake(0.24f, 0.48f);
            if (warningClip != null)
            {
                entranceAudio.pitch = 0.92f;
                entranceAudio.PlayOneShot(warningClip, 0.36f * GameSettingsService.SfxVolume);
            }

            if (rumbleClip != null)
            {
                entranceAudio.pitch = 0.72f;
                entranceAudio.PlayOneShot(rumbleClip, 0.52f * GameSettingsService.SfxVolume);
            }

            SpawnEntranceBurst();
            yield return new WaitForSeconds(0.6f);
            CameraShakeController.Ensure(cam)?.Shake(0.12f, 0.26f);
        }

        private void SetupDefenseNodes()
        {
            EnsureDamageNode("_ShieldNode", false, out shieldRenderer, out shieldCollider, 92, 0.86f, GeneratedSpriteKind.Ring, new Vector3(2.55f, 2.55f, 1f));
            EnsureDamageNode("_CoreNode", true, out coreRenderer, out coreCollider, 95, 0.22f, GeneratedSpriteKind.Cockpit, new Vector3(0.62f, 0.62f, 1f));

            if (shieldRenderer != null)
            {
                shieldRenderer.color = new Color(0.42f, 0.78f, 1f, 0.38f);
            }

            if (coreRenderer != null)
            {
                coreRenderer.color = new Color(1f, 0.84f, 0.34f, 0.85f);
            }
        }

        private void EnsureDamageNode(string name, bool weakPoint, out SpriteRenderer renderer, out Collider2D collider2d, int order, float radius, GeneratedSpriteKind spriteKind, Vector3 localScale)
        {
            Transform child = transform.Find(name);
            if (child == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = weakPoint ? new Vector3(0f, 0.15f, 0f) : new Vector3(0f, 0.06f, 0f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = localScale;
                child = go.transform;
            }

            renderer = VisualDebugSprite.Ensure(child.gameObject, Color.white, order, 1f, spriteKind);
            collider2d = child.GetComponent<CircleCollider2D>();
            if (collider2d == null)
            {
                CircleCollider2D circle = child.gameObject.AddComponent<CircleCollider2D>();
                circle.isTrigger = true;
                circle.radius = radius;
                collider2d = circle;
            }
            else
            {
                collider2d.isTrigger = true;
            }

            BossDamageNode node = child.GetComponent<BossDamageNode>();
            if (node == null)
            {
                node = child.gameObject.AddComponent<BossDamageNode>();
            }

            node.Initialize(this, weakPoint);
        }

        private void UpdatePhaseIndex()
        {
            float ratio = health.MaxHp > 0 ? (float)health.CurrentHp / health.MaxHp : 0f;
            int selected = 0;
            for (int i = 0; i < phases.Length; i++)
            {
                if (ratio <= phases[i].TriggerHpRatio)
                {
                    selected = i;
                }
            }

            phaseIndex = selected;
        }

        private void UpdateAttackMode()
        {
            if (Time.time < nextAttackModeChangeAt)
            {
                return;
            }

            BossAttackMode[] phaseModes;
            if (chapterIndex == 1)
            {
                phaseModes = phaseIndex <= 0
                    ? new[] { BossAttackMode.Spread, BossAttackMode.AimedBurst, BossAttackMode.OrbitalLattice }
                    : new[] { BossAttackMode.AimedBurst, BossAttackMode.SweepingArc, BossAttackMode.OrbitalLattice, BossAttackMode.SideLances };
            }
            else if (chapterIndex == 2)
            {
                phaseModes = phaseIndex <= 0
                    ? new[] { BossAttackMode.Spread, BossAttackMode.AsteroidScatter, BossAttackMode.AimedBurst }
                    : new[] { BossAttackMode.AsteroidScatter, BossAttackMode.SideLances, BossAttackMode.RainVolley, BossAttackMode.SweepingArc };
            }
            else
            {
                phaseModes = phaseIndex <= 0
                    ? new[] { BossAttackMode.PrismLance, BossAttackMode.AimedBurst, BossAttackMode.SweepingArc }
                    : new[] { BossAttackMode.PrismLance, BossAttackMode.RainVolley, BossAttackMode.SideLances, BossAttackMode.SweepingArc, BossAttackMode.AimedBurst };
            }

            int index = Mathf.Abs(Mathf.FloorToInt((Time.time + attackPatternSeed) * 0.73f)) % phaseModes.Length;
            BossAttackMode nextMode = phaseModes[index];
            if (!hasAnnouncedAttackMode || nextMode != currentAttackMode)
            {
                currentAttackMode = nextMode;
                AnnounceAttackMode(currentAttackMode);
            }
            else
            {
                currentAttackMode = nextMode;
            }
            nextAttackModeChangeAt = Time.time + Mathf.Lerp(2.8f, 1.45f, Mathf.Clamp01(phaseIndex / 2f));
        }

        private void AnnounceAttackMode(BossAttackMode mode)
        {
            if (hasAnnouncedAttackMode && lastAnnouncedAttackMode == mode)
            {
                return;
            }

            lastAnnouncedAttackMode = mode;
            hasAnnouncedAttackMode = true;
            string message;
            switch (mode)
            {
                case BossAttackMode.AimedBurst:
                    message = LocalizationService.Text("BOSS LOCKING BURST VECTOR", "Boss 锁定齐射轨迹");
                    break;
                case BossAttackMode.SweepingArc:
                    message = LocalizationService.Text("BOSS ARC SWEEP INBOUND", "Boss 弧线扫荡来袭");
                    break;
                case BossAttackMode.SideLances:
                    message = LocalizationService.Text("BOSS SIDE BATTERIES ONLINE", "Boss 侧舷炮列阵启动");
                    break;
                case BossAttackMode.RainVolley:
                    message = LocalizationService.Text("BOSS VOLLEY CURTAIN FORMING", "Boss 弹幕雨幕生成");
                    break;
                case BossAttackMode.OrbitalLattice:
                    message = LocalizationService.Text("ORBITAL GRID FIRING", "轨道格网火力开启");
                    break;
                case BossAttackMode.AsteroidScatter:
                    message = LocalizationService.Text("ASTEROID SCATTER PATTERN", "陨石散裂火力展开");
                    break;
                case BossAttackMode.PrismLance:
                    message = LocalizationService.Text("PRISM LANCE LOCK", "棱镜枪阵开始锁定");
                    break;
                default:
                    message = LocalizationService.Text("BOSS SPREAD VOLLEY", "Boss 扇形压制弹幕");
                    break;
            }

            GameEvents.RaiseCombatAnnouncement(message);
        }

        private void UpdateShieldCycle()
        {
            if (shieldActive)
            {
                return;
            }

            if (Time.time >= nextShieldToggleAt && !coreRetaliating)
            {
                ApplyShieldState(true, true);
            }
        }

        private float GetShieldMaxHp()
        {
            if (phaseIndex >= 2)
            {
                return 22f;
            }

            if (phaseIndex >= 1)
            {
                return 17f;
            }

            return 13f;
        }

        private float GetCoreExposeDuration()
        {
            if (phaseIndex >= 2)
            {
                return 3f;
            }

            if (phaseIndex >= 1)
            {
                return 2.5f;
            }

            return 2f;
        }

        private void ApplyShieldState(bool active, bool announce)
        {
            shieldActive = active;
            if (active)
            {
                shieldCurrentHp = GetShieldMaxHp();
                nextShieldToggleAt = Time.time + 999f;
            }
            else
            {
                nextShieldToggleAt = Time.time + GetCoreExposeDuration();
            }

            if (shieldCollider != null)
            {
                shieldCollider.enabled = active;
            }

            if (coreCollider != null)
            {
                coreCollider.enabled = true;
            }

            if (shieldRenderer != null)
            {
                shieldRenderer.enabled = active;
            }

            if (announce)
            {
                GameEvents.RaiseCombatAnnouncement(active ? "BOSS SHIELD REBUILT" : "CORE EXPOSED");
            }
        }

        private void BreakShield(Vector3 hitPosition)
        {
            ApplyShieldState(false, false);
            SpawnShieldShatter(hitPosition);
            PlayBossSfx(shieldBreakClip, 0.34f, 0.86f);
            PlayBossSfx(coreExposeClip, 0.2f, 1.12f);
            GameEvents.RaiseCombatAnnouncement("SHIELD BREAK - CORE EXPOSED");
        }

        private void TryTriggerCoreRetaliation()
        {
            if (coreRetaliating || Time.time < nextCoreRetaliationAt)
            {
                return;
            }

            int threshold = phaseIndex >= 2 ? 5 : 4;
            if (coreHitCounter < threshold)
            {
                return;
            }

            coreHitCounter = 0;
            nextCoreRetaliationAt = Time.time + 4f;
            if (phaseIndex >= 1)
            {
                StartCoroutine(LockOnLaserRetaliation());
            }
            else
            {
                StartCoroutine(ChargedCannonRetaliation());
            }
        }

        private IEnumerator LockOnLaserRetaliation()
        {
            coreRetaliating = true;
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                coreRetaliating = false;
                yield break;
            }

            float targetX = player.transform.position.x;
            GameEvents.RaiseCombatAnnouncement("LOCK-ON LASER");
            GameEvents.RaiseThreatEdgePulse(0.72f, true);
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = Object.FindFirstObjectByType<Camera>();
            }
            if (cam != null)
            {
                float viewportX = cam.WorldToViewportPoint(new Vector3(targetX, player.transform.position.y, 0f)).x;
                GameEvents.RaiseBossLockOnWarning(viewportX, 0.55f);
            }
            PlayBossSfx(laserLockClip, 0.34f, 1f);
            GameObject warning = BuildBeam(new Color(0.48f, 0.9f, 1f, 0.34f), targetX, 0.22f, 9f, 176);
            float warningDuration = 0.55f;
            float elapsed = 0f;
            while (elapsed < warningDuration)
            {
                elapsed += Time.deltaTime;
                if (warning != null)
                {
                    warning.transform.position = new Vector3(targetX, 0.3f, 0f);
                }
                yield return null;
            }

            if (warning != null)
            {
                Destroy(warning);
            }

            GameObject beam = BuildBeam(new Color(0.82f, 0.98f, 1f, 0.88f), targetX, 0.48f, 10.5f, 178);
            beam.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.48f, 10.5f, 1f),
                new Vector3(0.7f, 10.9f, 1f),
                new Color(0.82f, 0.98f, 1f, 0.88f),
                new Color(0.52f, 0.94f, 1f, 0f),
                0.22f,
                178);

            if (player != null && Mathf.Abs(player.transform.position.x - targetX) <= 0.65f)
            {
                HealthComponent playerHp = player.GetComponent<HealthComponent>();
                if (playerHp != null)
                {
                    playerHp.TakeDamage(2, new DamageSource(gameObject, Faction.Enemy));
                    GameEvents.RaiseBossLaserHit(0.95f);
                }
            }

            yield return new WaitForSeconds(0.22f);
            coreRetaliating = false;
        }

        private IEnumerator ChargedCannonRetaliation()
        {
            coreRetaliating = true;
            GameEvents.RaiseCombatAnnouncement("CORE CHARGING");
            GameEvents.RaiseThreatEdgePulse(0.5f, false);
            PlayBossSfx(coreChargeClip, 0.34f, 0.9f);
            Vector3 pos = coreRenderer != null ? coreRenderer.transform.position : transform.position;
            SpawnCoreChargeVisual(pos);
            yield return new WaitForSeconds(0.65f);

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            Vector2 toPlayer = player != null ? (player.transform.position - firePoint.position).normalized : Vector2.down;
            float baseAngle = ClampDownwardAngle(Vector2.SignedAngle(Vector2.down, toPlayer));
            for (int i = -1; i <= 1; i++)
            {
                float angle = baseAngle + i * 10f;
                SpawnBossProjectile(firePoint.position, firePoint.rotation * Quaternion.Euler(0f, 0f, angle), 1.45f, 11.2f);
            }

            yield return new WaitForSeconds(0.15f);
            coreRetaliating = false;
        }

        private void AnimateDefenseNodes()
        {
            if (shieldRenderer != null)
            {
                float pulse = 0.9f + Mathf.Sin(Time.time * 4.6f) * 0.08f;
                float breakRatio = shieldActive ? Mathf.Clamp01(shieldCurrentHp / Mathf.Max(1f, GetShieldMaxHp())) : 0f;
                shieldRenderer.transform.localScale = Vector3.one * 2.55f * pulse;
                shieldRenderer.color = shieldActive
                    ? new Color(0.42f, 0.78f, 1f, 0.18f + 0.28f * breakRatio + Mathf.PingPong(Time.time * 0.5f, 0.12f))
                    : new Color(0.42f, 0.78f, 1f, 0f);
            }

            if (coreRenderer != null)
            {
                float pulse = 0.92f + Mathf.Sin(Time.time * 8.4f) * 0.12f;
                coreRenderer.transform.localScale = Vector3.one * (shieldActive ? 0.52f : 0.7f) * pulse;
                coreRenderer.color = shieldActive
                    ? new Color(1f, 0.84f, 0.34f, 0.34f)
                    : new Color(1f, 0.96f, 0.62f, 0.96f);
            }
        }

        private void AnnouncePhaseIfNeeded()
        {
            if (announcedPhaseIndex == phaseIndex)
            {
                return;
            }

            announcedPhaseIndex = phaseIndex;
            if (phaseIndex == 0)
            {
                GameEvents.RaiseCombatAnnouncement("BOSS ENGAGED");
            }
            else
            {
                GameEvents.RaiseCombatAnnouncement($"BOSS PHASE {phaseIndex + 1}");
            }
        }

        private void ApplyPhaseArtVariant(int index)
        {
            if (shipVisual == null || appliedVisualPhaseIndex == index)
            {
                return;
            }

            appliedVisualPhaseIndex = index;
            if (index >= 1)
            {
                shipVisual.SetArtVariant(RuntimeArtSpriteId.BossShipPhase2, RuntimeArtSpriteId.BossShipPhase2Damaged, RuntimeArtSpriteId.BossPhase2FragmentA, RuntimeArtSpriteId.BossPhase2FragmentB);
                if (chapterIndex == 2)
                {
                    shipVisual.SetThrusterPalette(new Color(1f, 0.46f, 0.18f, 0.62f), new Color(1f, 0.88f, 0.68f, 0.96f));
                }
                else
                {
                    shipVisual.SetThrusterPalette(new Color(0.36f, 0.88f, 1f, 0.62f), new Color(0.86f, 0.94f, 1f, 0.96f));
                }
            }
            else
            {
                shipVisual.SetArtVariant(RuntimeArtSpriteId.BossShip, RuntimeArtSpriteId.BossShipDamaged, RuntimeArtSpriteId.BossFragmentA, RuntimeArtSpriteId.BossFragmentB);
                if (chapterIndex == 3)
                {
                    shipVisual.SetThrusterPalette(new Color(0.48f, 0.84f, 1f, 0.5f), new Color(0.84f, 0.94f, 1f, 0.95f));
                }
                else
                {
                    shipVisual.SetThrusterPalette(new Color(1f, 0.46f, 0.14f, 0.5f), new Color(1f, 0.78f, 0.54f, 0.95f));
                }
            }
        }

        private void ExecuteAttackPattern(BossPhaseConfig phase)
        {
            switch (currentAttackMode)
            {
                case BossAttackMode.AimedBurst:
                    FireAimedBurst(phase);
                    break;
                case BossAttackMode.SweepingArc:
                    FireSweepingArc(phase);
                    break;
                case BossAttackMode.SideLances:
                    FireSideLances(phase);
                    break;
                case BossAttackMode.RainVolley:
                    FireRainVolley(phase);
                    break;
                case BossAttackMode.OrbitalLattice:
                    FireOrbitalLattice(phase);
                    break;
                case BossAttackMode.AsteroidScatter:
                    FireAsteroidScatter(phase);
                    break;
                case BossAttackMode.PrismLance:
                    FirePrismLance(phase);
                    break;
                default:
                    FireSpread(phase.BulletsPerShot, phase.SpreadAngle, 0f);
                    break;
            }
        }

        private void FireSpread(int bullets, float spreadAngle, float extraRotation)
        {
            if (projectilePrefab == null || firePoint == null || bullets <= 0)
            {
                return;
            }

            float startAngle = -spreadAngle * 0.5f;
            float step = bullets > 1 ? spreadAngle / (bullets - 1) : 0f;
            for (int i = 0; i < bullets; i++)
            {
                float angle = ClampDownwardAngle(startAngle + step * i + extraRotation);
                SpawnBossProjectile(firePoint.position, firePoint.rotation * Quaternion.Euler(0f, 0f, angle), phaseIndex >= 1 ? 1.16f : 1f);
            }
        }

        private void FireAimedBurst(BossPhaseConfig phase)
        {
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null || firePoint == null)
            {
                FireSpread(phase.BulletsPerShot, phase.SpreadAngle, 0f);
                return;
            }

            Vector2 toPlayer = (player.transform.position - firePoint.position).normalized;
            float baseAngle = ClampDownwardAngle(Vector2.SignedAngle(Vector2.down, toPlayer));
            int bullets = Mathf.Max(3, phase.BulletsPerShot + 1);
            float spread = Mathf.Max(10f, phase.SpreadAngle * 0.6f);
            FireSpread(bullets, spread, baseAngle);
        }

        private void FireSweepingArc(BossPhaseConfig phase)
        {
            int bullets = Mathf.Max(5, phase.BulletsPerShot + 2);
            float center = Mathf.Sin((Time.time + attackPatternSeed) * 2.2f) * 58f;
            FireSpread(bullets, Mathf.Max(34f, phase.SpreadAngle * 1.25f), center);
        }

        private void FireSideLances(BossPhaseConfig phase)
        {
            if (firePoint == null)
            {
                return;
            }

            float lateral = 1.35f;
            Vector3 left = firePoint.position - transform.right * lateral;
            Vector3 right = firePoint.position + transform.right * lateral;
            SpawnBossProjectile(left, firePoint.rotation * Quaternion.Euler(0f, 0f, -18f), 1.32f, 9.6f + phaseIndex * 0.7f);
            SpawnBossProjectile(right, firePoint.rotation * Quaternion.Euler(0f, 0f, 18f), 1.32f, 9.6f + phaseIndex * 0.7f);
            FireSpread(Mathf.Max(3, phase.BulletsPerShot - 1), Mathf.Max(18f, phase.SpreadAngle * 0.7f), 0f);
        }

        private void FireRainVolley(BossPhaseConfig phase)
        {
            if (firePoint == null)
            {
                return;
            }

            int columns = Mathf.Max(4, phase.BulletsPerShot);
            float width = 3.2f;
            for (int i = 0; i < columns; i++)
            {
                float t = columns == 1 ? 0.5f : (float)i / (columns - 1);
                float xOffset = Mathf.Lerp(-width, width, t);
                Vector3 spawn = firePoint.position + transform.right * xOffset;
                float drift = Mathf.Lerp(-12f, 12f, t);
                SpawnBossProjectile(spawn, firePoint.rotation * Quaternion.Euler(0f, 0f, drift), 0.96f, 8.8f + phaseIndex * 0.55f);
            }
        }

        private void FireOrbitalLattice(BossPhaseConfig phase)
        {
            if (firePoint == null)
            {
                return;
            }

            float[] offsets = { -2.7f, -0.9f, 0.9f, 2.7f };
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector3 spawn = firePoint.position + transform.right * offsets[i];
                float drift = offsets[i] > 0f ? -12f : 12f;
                SpawnBossProjectile(spawn, firePoint.rotation * Quaternion.Euler(0f, 0f, drift), 0.92f, 8.4f + Mathf.Abs(offsets[i]) * 0.3f);
            }

            FireSpread(Mathf.Max(3, phase.BulletsPerShot - 1), Mathf.Max(18f, phase.SpreadAngle * 0.75f), 0f);
        }

        private void FireAsteroidScatter(BossPhaseConfig phase)
        {
            if (firePoint == null)
            {
                return;
            }

            int salvo = Mathf.Max(5, phase.BulletsPerShot + 1);
            for (int i = 0; i < salvo; i++)
            {
                float t = salvo == 1 ? 0.5f : (float)i / (salvo - 1);
                float xOffset = Mathf.Lerp(-3.4f, 3.4f, t);
                float angle = Mathf.Lerp(-28f, 28f, t);
                Vector3 spawn = firePoint.position + transform.right * xOffset + Vector3.down * Random.Range(0f, 0.45f);
                SpawnBossProjectile(spawn, firePoint.rotation * Quaternion.Euler(0f, 0f, angle), 1.04f, 8.3f + Random.Range(0.35f, 1.2f));
            }
        }

        private void FirePrismLance(BossPhaseConfig phase)
        {
            if (firePoint == null)
            {
                return;
            }

            float[] angles = { -32f, -16f, 0f, 16f, 32f };
            for (int i = 0; i < angles.Length; i++)
            {
                Vector3 spawn = firePoint.position + transform.right * (i - 2) * 0.78f;
                SpawnBossProjectile(spawn, firePoint.rotation * Quaternion.Euler(0f, 0f, angles[i]), 1.14f, 9.2f + phaseIndex * 0.8f);
            }

            if (phaseIndex >= 1)
            {
                FireSpread(3, 20f, Mathf.Sin(Time.time * 2.6f) * 18f);
            }
        }

        private void SpawnBossProjectile(Vector3 position, Quaternion rotation, float scaleMultiplier, float overrideSpeed = -1f)
        {
            Projectile projectile = null;
            if (usePooling && ProjectilePool.Instance != null)
            {
                projectile = ProjectilePool.Instance.Spawn(projectilePrefab, position, rotation);
            }

            if (projectile == null)
            {
                projectile = Instantiate(projectilePrefab, position, rotation);
            }

            float speed = overrideSpeed > 0f ? overrideSpeed : 8.5f;
            projectile.Setup(Faction.Enemy, 1, speed);
            if (chapterIndex == 3)
            {
                Color color = phaseIndex >= 1 ? new Color(0.64f, 0.94f, 1f, 1f) : new Color(0.78f, 0.84f, 1f, 1f);
                projectile.SetVisualOverride(color, new Vector3(scaleMultiplier * 1.12f, scaleMultiplier * 1.2f, 1f));
            }
            else if (chapterIndex == 2)
            {
                Color color = phaseIndex >= 1 ? new Color(1f, 0.46f, 0.18f, 1f) : new Color(1f, 0.72f, 0.34f, 1f);
                projectile.SetVisualOverride(color, new Vector3(scaleMultiplier * 1.08f, scaleMultiplier * 1.16f, 1f));
            }
            else if (phaseIndex >= 1)
            {
                projectile.SetVisualOverride(new Color(0.94f, 0.88f, 0.4f, 1f), new Vector3(scaleMultiplier * 1.08f, scaleMultiplier * 1.18f, 1f));
            }
            else
            {
                projectile.SetVisualOverride(new Color(1f, 0.78f, 0.46f, 1f), new Vector3(scaleMultiplier, scaleMultiplier * 1.08f, 1f));
            }
        }

        private float ClampDownwardAngle(float angle)
        {
            return Mathf.Clamp(angle, -78f, 78f);
        }

        private void SpawnShieldBlockEffect(Vector3 position, bool coreBlocked)
        {
            GameObject ring = new GameObject(coreBlocked ? "_CoreBlocked" : "_ShieldBlock");
            ring.transform.position = position;
            SpriteRenderer renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            renderer.color = coreBlocked ? new Color(0.78f, 0.92f, 1f, 0.88f) : new Color(0.42f, 0.8f, 1f, 0.72f);
            TransientSpriteEffect effect = ring.AddComponent<TransientSpriteEffect>();
            effect.Setup(new Vector3(0.22f, 0.22f, 1f), new Vector3(coreBlocked ? 0.9f : 1.3f, coreBlocked ? 0.9f : 1.3f, 1f), renderer.color, new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f), coreBlocked ? 0.16f : 0.22f, 170);
        }

        private void SpawnShieldShatter(Vector3 position)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 offset = Quaternion.Euler(0f, 0f, angle) * Vector3.up * 0.45f;
                GameObject shard = new GameObject("_ShieldShard");
                shard.transform.position = position + offset;
                SpriteRenderer renderer = shard.AddComponent<SpriteRenderer>();
                renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
                renderer.color = new Color(0.56f, 0.9f, 1f, 0.82f);
                TransientSpriteEffect effect = shard.AddComponent<TransientSpriteEffect>();
                effect.Setup(new Vector3(0.18f, 0.18f, 1f), new Vector3(0.95f, 0.95f, 1f), renderer.color, new Color(0.56f, 0.9f, 1f, 0f), 0.28f, 171);
            }
        }

        private void SpawnCoreHitEffect(Vector3 position)
        {
            GameObject flash = new GameObject("_CoreHit");
            flash.transform.position = position;
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            renderer.color = new Color(1f, 0.94f, 0.72f, 0.9f);
            TransientSpriteEffect effect = flash.AddComponent<TransientSpriteEffect>();
            effect.Setup(new Vector3(0.28f, 0.28f, 1f), new Vector3(1.05f, 1.05f, 1f), renderer.color, new Color(1f, 0.45f, 0.1f, 0f), 0.18f, 172);
        }

        private void SpawnCoreChargeVisual(Vector3 position)
        {
            GameObject flash = new GameObject("_CoreCharge");
            flash.transform.position = position;
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Cockpit);
            renderer.color = new Color(1f, 0.96f, 0.72f, 0.92f);
            TransientSpriteEffect effect = flash.AddComponent<TransientSpriteEffect>();
            effect.Setup(new Vector3(0.35f, 0.35f, 1f), new Vector3(1.2f, 1.2f, 1f), renderer.color, new Color(1f, 0.82f, 0.24f, 0f), 0.6f, 174);
        }

        private GameObject BuildBeam(Color color, float x, float width, float length, int order)
        {
            GameObject beam = new GameObject("_BossBeam");
            beam.transform.position = new Vector3(x, 0.3f, 0f);
            SpriteRenderer renderer = beam.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Bullet);
            renderer.color = color;
            renderer.sortingOrder = order;
            beam.transform.localScale = new Vector3(width, length, 1f);
            return beam;
        }

        private void SpawnEntranceBurst()
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject burst = new GameObject($"_BossEntranceBurst_{i}");
                burst.transform.position = transform.position + new Vector3((i - 1) * 0.72f, 1.08f + Mathf.Abs(i - 1) * 0.08f, 0f);
                SpriteRenderer renderer = burst.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
                renderer.color = new Color(1f, 0.88f, 0.56f, 0.96f);

                TransientSpriteEffect effect = burst.AddComponent<TransientSpriteEffect>();
                effect.Setup(new Vector3(0.3f, 0.65f, 1f), new Vector3(1.35f, 3f, 1f), renderer.color, new Color(1f, 0.38f, 0.06f, 0f), 0.44f, 149);
            }
        }

        private void HandleBossDied()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddKillScore(5000);
            }

            PickupSpawner.SpawnBossDrops(transform.position, Mathf.Clamp(CampaignRuntime.CurrentLevel != null ? CampaignRuntime.CurrentLevel.ChapterIndex : 1, 1, 3));

            Destroy(gameObject);
        }

        private void PlayBossSfx(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || entranceAudio == null)
            {
                return;
            }

            entranceAudio.pitch = pitch;
            entranceAudio.PlayOneShot(clip, volume * GameSettingsService.SfxVolume);
        }

        private static void EnsureCombatAudio()
        {
            if (warningClip == null)
            {
                warningClip = BuildSweep("boss-warning", 780f, 520f, 0.34f, 0.08f);
            }

            if (rumbleClip == null)
            {
                rumbleClip = BuildRumble("boss-rumble", 72f, 0.72f, 0.16f);
            }

            if (shieldHitClip == null)
            {
                shieldHitClip = BuildTone("boss-shield-hit", 1180f, 0.08f, 0.08f);
            }

            if (shieldBreakClip == null)
            {
                shieldBreakClip = BuildSweep("boss-shield-break", 920f, 220f, 0.28f, 0.12f);
            }

            if (coreExposeClip == null)
            {
                coreExposeClip = BuildSweep("boss-core-expose", 320f, 620f, 0.18f, 0.1f);
            }

            if (laserLockClip == null)
            {
                laserLockClip = BuildSweep("boss-laser-lock", 420f, 1080f, 0.26f, 0.1f);
            }

            if (coreChargeClip == null)
            {
                coreChargeClip = BuildSweep("boss-core-charge", 260f, 840f, 0.42f, 0.1f);
            }
        }

        private static AudioClip BuildSweep(string clipName, float startFrequency, float endFrequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                float envelope = Mathf.Sin(progress * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildTone(string clipName, float frequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(1f - (t / duration));
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildRumble(string clipName, float frequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(1f - (t / duration));
                float tone = Mathf.Sin(2f * Mathf.PI * frequency * t);
                float harmonic = Mathf.Sin(2f * Mathf.PI * frequency * 0.5f * t) * 0.6f;
                data[i] = (tone + harmonic) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
