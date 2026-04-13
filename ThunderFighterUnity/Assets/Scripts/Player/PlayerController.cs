
using System.Collections;
using ThunderFighter.Combat;
using ThunderFighter.Config;
using ThunderFighter.Core;
using ThunderFighter.InputSystem;
using UnityEngine;

namespace ThunderFighter.Player
{
    [RequireComponent(typeof(HealthComponent))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private KeyboardMouseInputProvider inputProvider;
        [SerializeField] private WeaponController weapon;

        private IInputProvider input;
        private HealthComponent health;
        private WeaponController cachedWeapon;
        private ProceduralShipVisual proceduralShipVisual;
        private PlayerBuffController buffController;
        private ShipDefinition selectedShip;
        private PlayerRuntimeLoadout runtimeLoadout;
        private Vector2 lastMoveInput = Vector2.up;
        private Vector2 dashDirection = Vector2.up;
        private float dashEndTime;
        private float nextDashAvailableTime;
        private bool isDashing;
        private float nextAfterimageAt;
        private float skillEnergy;
        private AudioSource skillAudio;
        private static AudioClip novaClip;
        private static AudioClip overdriveClip;
        private static AudioClip rapidClip;
        private static AudioClip heavyClip;
        private static AudioClip pickupClip;
        private static AudioClip balancedFireClip;
        private static AudioClip rapidFireClip;
        private static AudioClip heavyFireClip;
        private Transform overdriveLoopVisual;
        private SpriteRenderer overdriveLoopRenderer;
        private float fireFeedbackUntil;
        private float nextFireSfxAt;
        private bool skillTwoActive;
        private float skillTwoEndTime;
        private float activeSkillPrimaryMultiplier = 1f;
        private float activeSkillSupportMultiplier = 1f;
        private float activeSkillProjectileSpeedMultiplier = 1f;
        private int activeSkillDamageBonus;
        private float activeSkillDamageMultiplier = 1f;
        private float activeMoveSpeedMultiplier = 1f;
        private float currentVisualTilt;

        public ShipDefinition SelectedShip => selectedShip;
        public ShipId CurrentShipId => selectedShip != null ? selectedShip.ShipId : ShipId.Balanced;
        public Color CurrentAccentColor => selectedShip != null ? selectedShip.AccentColor : new Color(0.42f, 0.9f, 1f, 1f);

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            input = inputProvider;
            cachedWeapon = weapon;
            buffController = GetComponent<PlayerBuffController>();
            if (buffController == null)
            {
                buffController = gameObject.AddComponent<PlayerBuffController>();
            }

            skillAudio = GetComponent<AudioSource>();
            if (skillAudio == null)
            {
                skillAudio = gameObject.AddComponent<AudioSource>();
            }

            skillAudio.playOnAwake = false;
            skillAudio.spatialBlend = 0f;
            EnsureSkillAudio();

            gameObject.layer = 0;
            SpriteRenderer baseRenderer = VisualDebugSprite.Ensure(gameObject, new Color(0.35f, 0.9f, 1f, 1f), 40, 0.8f);
            if (baseRenderer != null)
            {
                baseRenderer.enabled = true;
                baseRenderer.sortingOrder = 5;
                baseRenderer.color = new Color(0.12f, 0.35f, 0.42f, 0.35f);
            }

            proceduralShipVisual = ProceduralShipVisual.Ensure(gameObject, ProceduralShipStyle.Player, 120);
            if (RuntimeArtLibrary.Get(RuntimeArtSpriteId.PlayerShip) != null && baseRenderer != null)
            {
                baseRenderer.enabled = false;
            }

            ConfigureWeaponHardpoints();
            ApplyCurrentShipSelection(true);

            if (GetComponent<DamageFeedback>() == null)
            {
                gameObject.AddComponent<DamageFeedback>();
            }
        }

        private void Start()
        {
            PublishSkillEnergy();
            PublishLoadout();
            PublishBuffSummary();
        }

        private void OnEnable()
        {
            LocalizationService.OnLanguageChanged += HandleLanguageChanged;
            if (health != null)
            {
                health.OnDied += OnPlayerDied;
            }
        }

        private void OnDisable()
        {
            LocalizationService.OnLanguageChanged -= HandleLanguageChanged;
            if (health != null)
            {
                health.OnDied -= OnPlayerDied;
            }
        }

        private void Update()
        {
            if (input == null || playerConfig == null || selectedShip == null)
            {
                return;
            }

            if (skillTwoActive && Time.time >= skillTwoEndTime)
            {
                SetSkillTwoMode(false);
            }

            RegenerateSkillEnergy();
            ApplyRuntimeWeaponState();

            Vector2 move = input.GetMoveVector();
            if (move.sqrMagnitude > 0.001f)
            {
                lastMoveInput = move.normalized;
            }

            if (input.IsDashPressed() && Time.time >= nextDashAvailableTime)
            {
                BeginDash(move.sqrMagnitude > 0.001f ? move : lastMoveInput);
            }

            if (input.IsSkillOnePressed())
            {
                TryCastSkillOne();
            }

            if (input.IsSkillTwoPressed())
            {
                TryCastSkillTwo();
            }

            float currentMoveSpeed = playerConfig.MoveSpeed * selectedShip.MoveSpeedMultiplier * activeMoveSpeedMultiplier;
            float dashSpeed = playerConfig.DashSpeed * (selectedShip.Archetype == ShipArchetype.Rapid ? 1.08f : 1f);
            Vector2 moveVector = isDashing ? dashDirection * dashSpeed : move * currentMoveSpeed;
            if (isDashing && Time.time >= dashEndTime)
            {
                isDashing = false;
            }

            Vector3 nextPos = transform.position + new Vector3(moveVector.x, moveVector.y, 0f) * Time.deltaTime;
            nextPos.x = Mathf.Clamp(nextPos.x, playerConfig.MoveBoundsX.x, playerConfig.MoveBoundsX.y);
            nextPos.y = Mathf.Clamp(nextPos.y, playerConfig.MoveBoundsY.x, playerConfig.MoveBoundsY.y);
            transform.position = nextPos;
            UpdateVisualDynamics(move, moveVector, currentMoveSpeed);

            if (input.IsFirePressed() && cachedWeapon != null && cachedWeapon.TryFire())
            {
                TriggerFireFeedback();
            }

            if (input.IsPausePressed())
            {
                GameFlowController flow = Object.FindFirstObjectByType<GameFlowController>();
                flow?.TogglePause();
            }
        }

        public void ApplyPickup(PickupDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            switch (definition.Kind)
            {
                case PickupKind.WeaponLevel:
                    if (runtimeLoadout != null && runtimeLoadout.WeaponLevel < 4)
                    {
                        runtimeLoadout.WeaponLevel++;
                        cachedWeapon?.SetWeaponLevel(runtimeLoadout.WeaponLevel);
                        PublishLoadout();
                        RaisePickupNotice(LocalizationService.IsChinese ? $"火力升级 Lv.{runtimeLoadout.WeaponLevel}" : $"Weapon Level {runtimeLoadout.WeaponLevel}");
                    }
                    else
                    {
                        health?.RestoreHp(10);
                        skillEnergy = Mathf.Min(GetSkillEnergyMax(), skillEnergy + 10f);
                        PublishSkillEnergy();
                        RaisePickupNotice(LocalizationService.Text("Power overflow converted", "满级火力转化"));
                    }
                    break;
                case PickupKind.Repair:
                    health?.RestoreHp(definition.IntValue);
                    RaisePickupNotice(definition.GetDisplayName(LocalizationService.IsChinese));
                    break;
                case PickupKind.SkillEnergy:
                    skillEnergy = Mathf.Min(GetSkillEnergyMax(), skillEnergy + definition.IntValue);
                    PublishSkillEnergy();
                    RaisePickupNotice(definition.GetDisplayName(LocalizationService.IsChinese));
                    break;
                case PickupKind.FireRateBuff:
                    buffController.AddOrRefresh(PickupBuffType.FireRate, definition.DurationSeconds, definition.Magnitude);
                    RaisePickupNotice(definition.GetDisplayName(LocalizationService.IsChinese));
                    break;
                case PickupKind.DamageBuff:
                    buffController.AddOrRefresh(PickupBuffType.Damage, definition.DurationSeconds, definition.Magnitude);
                    RaisePickupNotice(definition.GetDisplayName(LocalizationService.IsChinese));
                    break;
                case PickupKind.ProjectileSpeedBuff:
                    buffController.AddOrRefresh(PickupBuffType.ProjectileSpeed, definition.DurationSeconds, definition.Magnitude);
                    RaisePickupNotice(definition.GetDisplayName(LocalizationService.IsChinese));
                    break;
                case PickupKind.MagnetBuff:
                    buffController.AddOrRefresh(PickupBuffType.Magnet, definition.DurationSeconds, definition.Magnitude);
                    RaisePickupNotice(definition.GetDisplayName(LocalizationService.IsChinese));
                    break;
                case PickupKind.GuardBuff:
                    buffController.AddOrRefresh(PickupBuffType.Guard, definition.DurationSeconds, definition.Magnitude);
                    RaisePickupNotice(definition.GetDisplayName(LocalizationService.IsChinese));
                    break;
            }

            PlaySkillSfx(pickupClip, 0.22f, 1.08f);
            PublishBuffSummary();
            ApplyRuntimeWeaponState();
            SpawnPickupFlash(definition.AccentColor);
        }

        public float GetPickupMagnetMultiplier()
        {
            return buffController != null ? buffController.MagnetRadiusMultiplier : 1f;
        }

        private void HandleLanguageChanged(GameLanguage _)
        {
            PublishSkillEnergy();
            PublishLoadout();
            PublishBuffSummary();
        }

        private void ApplyCurrentShipSelection(bool resetLoadout)
        {
            selectedShip = ShipCatalog.GetSelected() ?? ShipCatalog.GetById(ShipId.Balanced);
            runtimeLoadout = CampaignRuntime.CurrentLoadout;
            if (runtimeLoadout == null || runtimeLoadout.ShipId != selectedShip.ShipId || resetLoadout)
            {
                runtimeLoadout = new PlayerRuntimeLoadout(selectedShip.ShipId);
                CampaignRuntime.CurrentLoadout = runtimeLoadout;
            }

            if (health != null)
            {
                int maxHp = CampaignProgressService.GetPlayerMaxHp() + selectedShip.MaxHpBonus;
                health.ForceSetMaxHp(Mathf.Max(60, maxHp), true);
            }

            skillEnergy = GetSkillEnergyMax();
            ApplyShipVisuals();
            cachedWeapon?.ApplyLoadout(selectedShip);
            cachedWeapon?.SetWeaponLevel(runtimeLoadout.WeaponLevel);
            ApplyCampaignUpgrades();
            ApplyRuntimeWeaponState();
        }

        private void ApplyShipVisuals()
        {
            if (proceduralShipVisual == null || selectedShip == null)
            {
                return;
            }

            RuntimeArtSpriteId shipSpriteId = RuntimeArtLibrary.GetPlayerShipSpriteId(selectedShip.ShipId);
            RuntimeArtSpriteId damagedSpriteId = RuntimeArtLibrary.GetPlayerShipDamagedSpriteId(selectedShip.ShipId);
            RuntimeArtSpriteId[] fragmentIds = RuntimeArtLibrary.GetPlayerFragmentIds(selectedShip.ShipId);
            proceduralShipVisual.SetArtVariant(
                shipSpriteId,
                damagedSpriteId,
                fragmentIds);

            SpriteRenderer renderer = proceduralShipVisual.GetPrimaryRenderer();
            if (renderer != null)
            {
                Sprite shipSprite = RuntimeArtLibrary.Get(shipSpriteId) ?? GeneratedSpriteLibrary.GetShipPresentationSprite(selectedShip.ShipId, false);
                if (shipSprite != null)
                {
                    renderer.sprite = shipSprite;
                }

                renderer.color = Color.white;
            }
        }

        private void ConfigureWeaponHardpoints()
        {
            if (cachedWeapon == null || proceduralShipVisual == null)
            {
                return;
            }

            cachedWeapon.ConfigurePlayerFirePoints(
                proceduralShipVisual.GetMainMuzzleAnchor(),
                proceduralShipVisual.GetSupportLeftMuzzleAnchor(),
                proceduralShipVisual.GetSupportRightMuzzleAnchor());
        }

        private void ApplyCampaignUpgrades()
        {
            if (cachedWeapon != null)
            {
                cachedWeapon.SetProgressionModifiers(CampaignProgressService.GetFireRateMultiplier(), CampaignProgressService.GetFirepowerBonus());
            }
        }

        private void ApplyRuntimeWeaponState()
        {
            if (cachedWeapon == null || selectedShip == null || runtimeLoadout == null)
            {
                return;
            }

            float buffPrimary = buffController != null ? buffController.FireRateIntervalMultiplier : 1f;
            float buffSpeed = buffController != null ? buffController.ProjectileSpeedMultiplier : 1f;
            float buffDamage = buffController != null ? buffController.DamageMultiplier : 1f;
            cachedWeapon.SetRuntimeModifiers(
                activeSkillPrimaryMultiplier * buffPrimary,
                activeSkillSupportMultiplier * buffPrimary,
                activeSkillProjectileSpeedMultiplier * buffSpeed,
                activeSkillDamageBonus,
                activeSkillDamageMultiplier * buffDamage);
            cachedWeapon.SetWeaponLevel(runtimeLoadout.WeaponLevel);
        }

        private void RegenerateSkillEnergy()
        {
            float old = skillEnergy;
            skillEnergy = Mathf.Min(GetSkillEnergyMax(), skillEnergy + (playerConfig.SkillEnergyRegenPerSecond * CampaignProgressService.GetSkillRegenMultiplier()) * Time.deltaTime);
            if (!Mathf.Approximately(old, skillEnergy))
            {
                PublishSkillEnergy();
            }
        }

        private float GetSkillEnergyMax()
        {
            return playerConfig != null ? playerConfig.SkillEnergyMax : 100f;
        }

        private void TryCastSkillOne()
        {
            if (selectedShip == null)
            {
                return;
            }

            if (skillEnergy < selectedShip.SkillOneCost)
            {
                GameEvents.RaiseCombatAnnouncement(LocalizationService.IsChinese ? $"{selectedShip.GetSkillOneName(true)} 能量不足" : $"{selectedShip.GetSkillOneName(false).ToUpperInvariant()} ENERGY LOW");
                return;
            }

            skillEnergy -= selectedShip.SkillOneCost;
            PublishSkillEnergy();

            switch (selectedShip.Archetype)
            {
                case ShipArchetype.Rapid:
                    StartCoroutine(FireRapidBarrageRoutine());
                    SpawnRapidSkillVisual();
                    PlaySkillSfx(rapidClip, 0.32f, 1.08f);
                    break;
                case ShipArchetype.Heavy:
                    FireHeavyBreaker();
                    SpawnHeavySkillVisual();
                    PlaySkillSfx(heavyClip, 0.34f, 0.92f);
                    break;
                default:
                    SpawnNovaVisual();
                    DamageNearbyTargets(playerConfig.NovaDamage + Mathf.RoundToInt(selectedShip.SkillOnePower), playerConfig.NovaRadius * Mathf.Lerp(1f, 1.2f, selectedShip.SkillOnePower - 1f));
                    ClearNearbyEnemyProjectiles(playerConfig.NovaRadius * Mathf.Lerp(1f, 1.15f, selectedShip.SkillOnePower - 1f));
                    PlaySkillSfx(novaClip, 0.36f, 1f);
                    break;
            }

            GameEvents.RaiseCombatAnnouncement(selectedShip.GetSkillOneName(LocalizationService.IsChinese));
        }

        private void TryCastSkillTwo()
        {
            if (selectedShip == null || skillTwoActive)
            {
                return;
            }

            if (skillEnergy < selectedShip.SkillTwoCost)
            {
                GameEvents.RaiseCombatAnnouncement(LocalizationService.IsChinese ? $"{selectedShip.GetSkillTwoName(true)} 能量不足" : $"{selectedShip.GetSkillTwoName(false).ToUpperInvariant()} ENERGY LOW");
                return;
            }

            skillEnergy -= selectedShip.SkillTwoCost;
            PublishSkillEnergy();
            SetSkillTwoMode(true);
            skillTwoEndTime = Time.time + selectedShip.SkillTwoDuration;
            PlaySkillSfx(overdriveClip, 0.34f, selectedShip.Archetype == ShipArchetype.Heavy ? 0.92f : 1f);
            GameEvents.RaiseCombatAnnouncement(selectedShip.GetSkillTwoName(LocalizationService.IsChinese));
        }

        private void SetSkillTwoMode(bool active)
        {
            skillTwoActive = active;
            activeSkillPrimaryMultiplier = 1f;
            activeSkillSupportMultiplier = 1f;
            activeSkillProjectileSpeedMultiplier = 1f;
            activeSkillDamageBonus = 0;
            activeSkillDamageMultiplier = 1f;
            activeMoveSpeedMultiplier = 1f;

            if (active && selectedShip != null)
            {
                switch (selectedShip.Archetype)
                {
                    case ShipArchetype.Rapid:
                        activeSkillPrimaryMultiplier = 0.48f;
                        activeSkillSupportMultiplier = 0.52f;
                        activeSkillProjectileSpeedMultiplier = 1.18f;
                        activeMoveSpeedMultiplier = 1.18f;
                        activeSkillDamageMultiplier = 1.1f;
                        break;
                    case ShipArchetype.Heavy:
                        activeSkillPrimaryMultiplier = 0.88f;
                        activeSkillSupportMultiplier = 0.9f;
                        activeSkillProjectileSpeedMultiplier = 1.08f;
                        activeSkillDamageBonus = 2;
                        activeSkillDamageMultiplier = 1.22f;
                        activeMoveSpeedMultiplier = 0.96f;
                        buffController.AddOrRefresh(PickupBuffType.Guard, selectedShip.SkillTwoDuration, 0.24f * selectedShip.SkillTwoPower);
                        break;
                    default:
                        activeSkillPrimaryMultiplier = playerConfig.OverdriveFireRateMultiplier;
                        activeSkillSupportMultiplier = Mathf.Lerp(1f, playerConfig.OverdriveFireRateMultiplier, 0.8f);
                        activeSkillProjectileSpeedMultiplier = playerConfig.OverdriveProjectileSpeedMultiplier;
                        activeSkillDamageBonus = 1;
                        activeMoveSpeedMultiplier = 1.08f;
                        break;
                }

                SpawnOverdriveVisual();
            }
            else
            {
                DisableOverdriveLoopVisual();
            }

            PublishSkillEnergy();
            PublishBuffSummary();
            ApplyRuntimeWeaponState();
        }

        private void DamageNearbyTargets(int damage, float radius)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null || colliders[i].gameObject == gameObject)
                {
                    continue;
                }

                HealthComponent hp = colliders[i].GetComponent<HealthComponent>();
                if (hp != null && hp.Faction == Faction.Player)
                {
                    continue;
                }

                IDamageable damageable = colliders[i].GetComponent<IDamageable>();
                if (damageable == null)
                {
                    continue;
                }

                SpawnNovaArcEffect(colliders[i].bounds.center, selectedShip != null ? selectedShip.AccentColor : new Color(0.62f, 0.96f, 1f, 1f));
                damageable.TakeDamage(damage, new DamageSource(gameObject, Faction.Player));
            }
        }

        private void ClearNearbyEnemyProjectiles(float radius)
        {
            Projectile[] projectiles = Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i] == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(transform.position, projectiles[i].transform.position);
                if (distance <= radius && projectiles[i].Faction == Faction.Enemy)
                {
                    Object.Destroy(projectiles[i].gameObject);
                }
            }
        }

        private IEnumerator FireRapidBarrageRoutine()
        {
            for (int i = 0; i < 3; i++)
            {
                cachedWeapon?.FireBurstPattern(9, 90f - i * 10f, 0f, 1.12f, 0);
                yield return new WaitForSeconds(0.08f);
            }
        }

        private void FireHeavyBreaker()
        {
            cachedWeapon?.FireBurstPattern(3, 14f, 0f, 1.18f, 4);
            StartCoroutine(FireHeavySecondWave());
        }

        private IEnumerator FireHeavySecondWave()
        {
            yield return new WaitForSeconds(0.12f);
            cachedWeapon?.FireBurstPattern(1, 0f, 0f, 1.3f, 6);
        }

        private void PublishSkillEnergy()
        {
            if (selectedShip == null)
            {
                return;
            }

            string label = LocalizationService.IsChinese
                ? string.Format("K {0} {1:0}  |  L {2} {3:0}", selectedShip.GetSkillOneName(true), selectedShip.SkillOneCost, selectedShip.GetSkillTwoName(true), selectedShip.SkillTwoCost)
                : string.Format("K {0} {1:0}  |  L {2} {3:0}", selectedShip.GetSkillOneName(false).ToUpperInvariant(), selectedShip.SkillOneCost, selectedShip.GetSkillTwoName(false).ToUpperInvariant(), selectedShip.SkillTwoCost);
            if (skillTwoActive)
            {
                label = string.Format("{0}  [{1}]", label, selectedShip.GetSkillTwoName(LocalizationService.IsChinese));
            }

            GameEvents.RaiseSkillEnergyChanged(skillEnergy / Mathf.Max(1f, GetSkillEnergyMax()), label);
        }

        private void PublishLoadout()
        {
            if (selectedShip == null || runtimeLoadout == null)
            {
                return;
            }

            GameEvents.RaiseLoadoutChanged(selectedShip.GetDisplayName(LocalizationService.IsChinese), runtimeLoadout.WeaponLevel);
        }

        private void PublishBuffSummary()
        {
            if (buffController == null)
            {
                return;
            }

            GameEvents.RaiseBuffStatusChanged(buffController.GetSummaryLabel(LocalizationService.IsChinese), buffController.GetActiveTypes());
        }

        private void RaisePickupNotice(string label)
        {
            GameEvents.RaisePickupCollected(label);
            GameEvents.RaiseCombatAnnouncement(label);
        }

        private void BeginDash(Vector2 requestedDirection)
        {
            dashDirection = requestedDirection.sqrMagnitude > 0.001f ? requestedDirection.normalized : Vector2.up;
            isDashing = true;
            dashEndTime = Time.time + playerConfig.DashDuration;
            nextDashAvailableTime = Time.time + playerConfig.DashCooldown;
            health?.GrantInvincibility(playerConfig.DashInvincibleDuration);
            SpawnDashBurst();
        }

        private void UpdateVisualDynamics(Vector2 moveInput, Vector2 actualMove, float currentMoveSpeed)
        {
            if (proceduralShipVisual == null || playerConfig == null || selectedShip == null)
            {
                return;
            }

            float normalizedX = Mathf.Clamp(moveInput.x, -1f, 1f);
            float tilt = -normalizedX * 18f;
            if (isDashing)
            {
                tilt += -dashDirection.x * 10f;
            }

            currentVisualTilt = Mathf.Lerp(currentVisualTilt, tilt, 12f * Time.deltaTime);
            proceduralShipVisual.SetVisualTilt(currentVisualTilt);
            float boost = isDashing ? 1f : Mathf.Clamp01(actualMove.magnitude / Mathf.Max(0.01f, currentMoveSpeed));
            if (Time.time < fireFeedbackUntil)
            {
                boost = Mathf.Max(boost, skillTwoActive ? 1f : 0.78f);
            }

            Color thrusterGlow = selectedShip.ShipId switch
            {
                ShipId.Rapid => new Color(1f, 0.72f, 0.22f, 0.7f),
                ShipId.Heavy => new Color(1f, 0.34f, 0.14f, 0.72f),
                _ => new Color(0.32f, 0.84f, 1f, 0.68f)
            };
            Color thrusterCore = selectedShip.ShipId switch
            {
                ShipId.Rapid => new Color(1f, 0.96f, 0.78f, 0.98f),
                ShipId.Heavy => new Color(1f, 0.82f, 0.62f, 0.98f),
                _ => new Color(0.82f, 0.98f, 1f, 0.98f)
            };
            if (skillTwoActive)
            {
                thrusterGlow = Color.Lerp(selectedShip.AccentColor, Color.white, 0.2f);
                thrusterCore = Color.Lerp(selectedShip.AccentColor, Color.white, 0.62f);
            }

            proceduralShipVisual.SetThrusterPalette(thrusterGlow, thrusterCore);
            proceduralShipVisual.SetThrustBoost(0.35f + boost * 0.8f);
            UpdateOverdriveLoopVisual();

            if ((isDashing || actualMove.sqrMagnitude > currentMoveSpeed * currentMoveSpeed * 0.42f || skillTwoActive) && Time.time >= nextAfterimageAt)
            {
                nextAfterimageAt = Time.time + (isDashing ? 0.04f : skillTwoActive ? 0.05f : 0.07f);
                SpawnAfterimage(actualMove);
            }
        }

        private void SpawnOverdriveVisual()
        {
            GameObject aura = new GameObject("_OverdriveAura");
            aura.transform.position = transform.position;
            aura.transform.SetParent(transform, true);
            SpriteRenderer renderer = aura.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.SkillOverdriveCast) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Thruster);
            renderer.color = new Color(selectedShip.AccentColor.r, selectedShip.AccentColor.g, selectedShip.AccentColor.b, 0.82f);
            aura.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.55f, 0.55f, 1f),
                new Vector3(2.2f, 2.8f, 1f),
                renderer.color,
                new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f),
                0.46f,
                189);

            EnsureOverdriveLoopVisual();
        }

        private void EnsureOverdriveLoopVisual()
        {
            if (overdriveLoopVisual != null)
            {
                overdriveLoopVisual.gameObject.SetActive(true);
                return;
            }

            GameObject aura = new GameObject("_OverdriveLoop");
            aura.transform.SetParent(transform, false);
            aura.transform.localPosition = Vector3.zero;
            overdriveLoopVisual = aura.transform;
            overdriveLoopRenderer = aura.AddComponent<SpriteRenderer>();
            overdriveLoopRenderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.SkillOverdriveCast) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Thruster);
            overdriveLoopRenderer.sortingOrder = 118;
            overdriveLoopRenderer.color = new Color(selectedShip.AccentColor.r, selectedShip.AccentColor.g, selectedShip.AccentColor.b, 0f);
        }

        private void UpdateOverdriveLoopVisual()
        {
            if (overdriveLoopRenderer == null)
            {
                return;
            }

            if (!skillTwoActive)
            {
                DisableOverdriveLoopVisual();
                return;
            }

            EnsureOverdriveLoopVisual();
            float pulse = 0.42f + Mathf.PingPong(Time.time * 2.6f, 0.28f);
            overdriveLoopRenderer.color = new Color(selectedShip.AccentColor.r, selectedShip.AccentColor.g, selectedShip.AccentColor.b, pulse);
            if (overdriveLoopVisual != null)
            {
                float scale = 1.6f + Mathf.Sin(Time.time * 6.2f) * 0.14f;
                overdriveLoopVisual.localScale = new Vector3(scale, scale * 1.25f, 1f);
                overdriveLoopVisual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 2f) * 4f);
            }
        }

        private void DisableOverdriveLoopVisual()
        {
            if (overdriveLoopRenderer != null)
            {
                Color color = selectedShip != null ? selectedShip.AccentColor : Color.white;
                overdriveLoopRenderer.color = new Color(color.r, color.g, color.b, 0f);
            }

            if (overdriveLoopVisual != null)
            {
                overdriveLoopVisual.gameObject.SetActive(false);
            }
        }

        private void SpawnDashBurst()
        {
            GameObject flash = new GameObject("_DashBurst");
            flash.transform.position = transform.position;
            flash.transform.localScale = new Vector3(1.2f, 1.8f, 1f);
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            Sprite flashSprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash);
            if (flashSprite != null)
            {
                renderer.sprite = flashSprite;
            }
            else
            {
                VisualDebugSprite.Ensure(renderer);
            }

            renderer.color = new Color(selectedShip.AccentColor.r, selectedShip.AccentColor.g, selectedShip.AccentColor.b, 0.72f);
            renderer.sortingOrder = 175;
            Destroy(flash, 0.12f);
        }

        private void SpawnPickupFlash(Color color)
        {
            GameObject flash = new GameObject("_PickupFlash");
            flash.transform.position = transform.position;
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            renderer.color = new Color(color.r, color.g, color.b, 0.64f);
            flash.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.35f, 0.35f, 1f),
                new Vector3(1.8f, 1.8f, 1f),
                renderer.color,
                new Color(color.r, color.g, color.b, 0f),
                0.22f,
                170);
        }

        private void PlaySkillSfx(AudioClip clip, float volume, float pitch)
        {
            if (skillAudio == null || clip == null)
            {
                return;
            }

            skillAudio.pitch = pitch;
            skillAudio.PlayOneShot(clip, volume * GameSettingsService.SfxVolume);
            skillAudio.pitch = 1f;
        }

        private static void EnsureSkillAudio()
        {
            if (novaClip == null)
            {
                novaClip = BuildSkillSweep("player-skill-nova", 360f, 1220f, 0.34f, 0.2f);
            }

            if (overdriveClip == null)
            {
                overdriveClip = BuildSkillSweep("player-skill-overdrive", 220f, 920f, 0.42f, 0.18f);
            }

            if (rapidClip == null)
            {
                rapidClip = BuildSkillSweep("player-skill-rapid", 820f, 1480f, 0.24f, 0.18f);
            }

            if (heavyClip == null)
            {
                heavyClip = BuildSkillSweep("player-skill-heavy", 180f, 620f, 0.36f, 0.22f);
            }

            if (pickupClip == null)
            {
                pickupClip = BuildSkillSweep("player-pickup", 540f, 980f, 0.12f, 0.14f);
            }

            if (balancedFireClip == null)
            {
                balancedFireClip = BuildSkillSweep("player-fire-balanced", 540f, 860f, 0.08f, 0.1f);
            }

            if (rapidFireClip == null)
            {
                rapidFireClip = BuildSkillSweep("player-fire-rapid", 860f, 1280f, 0.06f, 0.08f);
            }

            if (heavyFireClip == null)
            {
                heavyFireClip = BuildSkillSweep("player-fire-heavy", 220f, 420f, 0.12f, 0.12f);
            }
        }

        private static AudioClip BuildSkillSweep(string clipName, float startFrequency, float endFrequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                float envelope = Mathf.Sin(t * Mathf.PI) * volume;
                float main = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate);
                float harmonic = Mathf.Sin(2f * Mathf.PI * frequency * 1.8f * i / sampleRate) * 0.28f;
                samples[i] = (main + harmonic) * envelope;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void TriggerFireFeedback()
        {
            fireFeedbackUntil = Time.time + (skillTwoActive ? 0.12f : 0.08f);
            PlayFireSfx();

            Transform mainMuzzle = proceduralShipVisual != null ? proceduralShipVisual.GetMainMuzzleAnchor() : null;
            if (mainMuzzle != null)
            {
                SpawnWeaponPulse(mainMuzzle.position, ResolvePrimaryMuzzleColor(), ResolvePrimaryMuzzleScale(), 196);
            }

            Transform left = proceduralShipVisual != null ? proceduralShipVisual.GetSupportLeftMuzzleAnchor() : null;
            Transform right = proceduralShipVisual != null ? proceduralShipVisual.GetSupportRightMuzzleAnchor() : null;
            if (left != null)
            {
                SpawnWeaponPulse(left.position, ResolveSupportMuzzleColor(), ResolveSupportMuzzleScale(), 195);
            }

            if (right != null)
            {
                SpawnWeaponPulse(right.position, ResolveSupportMuzzleColor(), ResolveSupportMuzzleScale(), 195);
            }
        }

        private void PlayFireSfx()
        {
            if (skillAudio == null || Time.time < nextFireSfxAt || selectedShip == null)
            {
                return;
            }

            AudioClip clip = selectedShip.ShipId switch
            {
                ShipId.Rapid => rapidFireClip,
                ShipId.Heavy => heavyFireClip,
                _ => balancedFireClip
            };

            float volume = selectedShip.ShipId switch
            {
                ShipId.Rapid => 0.08f,
                ShipId.Heavy => 0.13f,
                _ => 0.1f
            };

            float pitch = selectedShip.ShipId switch
            {
                ShipId.Rapid => 1.15f,
                ShipId.Heavy => 0.82f,
                _ => 1f
            };

            nextFireSfxAt = Time.time + (selectedShip.ShipId == ShipId.Rapid ? 0.055f : selectedShip.ShipId == ShipId.Heavy ? 0.1f : 0.075f);
            skillAudio.pitch = pitch;
            skillAudio.PlayOneShot(clip, volume * GameSettingsService.SfxVolume);
            skillAudio.pitch = 1f;
        }

        private Color ResolvePrimaryMuzzleColor()
        {
            if (selectedShip == null)
            {
                return Color.white;
            }

            Color baseColor = selectedShip.ShipId switch
            {
                ShipId.Rapid => new Color(1f, 0.78f, 0.28f, 1f),
                ShipId.Heavy => new Color(1f, 0.42f, 0.16f, 1f),
                _ => new Color(0.52f, 0.92f, 1f, 1f)
            };

            return Color.Lerp(baseColor, Color.white, skillTwoActive ? 0.48f : 0.28f);
        }

        private Color ResolveSupportMuzzleColor()
        {
            return Color.Lerp(ResolvePrimaryMuzzleColor(), Color.white, 0.18f);
        }

        private float ResolvePrimaryMuzzleScale()
        {
            float baseScale = selectedShip != null && selectedShip.ShipId == ShipId.Heavy ? 0.52f : selectedShip != null && selectedShip.ShipId == ShipId.Rapid ? 0.32f : 0.38f;
            return skillTwoActive ? baseScale * 1.16f : baseScale;
        }

        private float ResolveSupportMuzzleScale()
        {
            float baseScale = selectedShip != null && selectedShip.ShipId == ShipId.Heavy ? 0.28f : selectedShip != null && selectedShip.ShipId == ShipId.Rapid ? 0.24f : 0.26f;
            return skillTwoActive ? baseScale * 1.08f : baseScale;
        }

        private void SpawnWeaponPulse(Vector3 position, Color color, float scale, int sortingOrder)
        {
            GameObject pulse = new GameObject("_PlayerWeaponPulse");
            pulse.transform.position = position;
            SpriteRenderer renderer = pulse.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            renderer.color = color;
            pulse.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(scale * 0.42f, scale * 0.42f, 1f),
                new Vector3(scale, scale * 1.18f, 1f),
                color,
                new Color(color.r, color.g, color.b, 0f),
                skillTwoActive ? 0.08f : 0.06f,
                sortingOrder);
        }

        private void SpawnAfterimage(Vector2 moveVector)
        {
            SpriteRenderer source = proceduralShipVisual != null ? proceduralShipVisual.GetPrimaryRenderer() : null;
            if (source == null || source.sprite == null)
            {
                return;
            }

            GameObject ghost = new GameObject("_PlayerAfterimage");
            ghost.transform.position = source.transform.position;
            ghost.transform.rotation = source.transform.rotation;
            ghost.transform.localScale = source.transform.lossyScale;

            SpriteRenderer renderer = ghost.AddComponent<SpriteRenderer>();
            renderer.sprite = source.sprite;
            renderer.flipX = source.flipX;
            renderer.flipY = source.flipY;

            AfterimageGhost afterimage = ghost.AddComponent<AfterimageGhost>();
            Color color = skillTwoActive
                ? new Color(selectedShip.AccentColor.r, selectedShip.AccentColor.g, selectedShip.AccentColor.b, isDashing ? 0.54f : 0.36f)
                : new Color(selectedShip.AccentColor.r, selectedShip.AccentColor.g, selectedShip.AccentColor.b, isDashing ? 0.42f : 0.26f);
            afterimage.Setup(isDashing ? 0.2f : 0.14f, new Vector3(-moveVector.x, -moveVector.y, 0f) * 0.08f, 110, color);
        }

        private void OnPlayerDied()
        {
            DisableOverdriveLoopVisual();
            gameObject.SetActive(false);
        }

        private void SpawnNovaVisual()
        {
            float radius = playerConfig.NovaRadius * Mathf.Lerp(1f, 1.2f, selectedShip.SkillOnePower - 1f);
            GameObject ring = new GameObject("_NovaBurst");
            ring.transform.position = transform.position;
            SpriteRenderer renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.SkillNovaCast) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            renderer.color = selectedShip.AccentColor;
            ring.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.65f, 0.65f, 1f),
                new Vector3(radius * 1.05f, radius * 1.05f, 1f),
                renderer.color,
                new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f),
                0.38f,
                190);
            StartCoroutine(SpawnNovaPulseSequence(radius, renderer.color));
        }

        private void SpawnRapidSkillVisual()
        {
            GameObject ring = new GameObject("_RapidSkillWave");
            ring.transform.position = transform.position;
            SpriteRenderer renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.SkillNovaCast) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            renderer.color = selectedShip.AccentColor;
            ring.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.42f, 0.42f, 1f),
                new Vector3(3.8f, 3.8f, 1f),
                renderer.color,
                new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f),
                0.28f,
                190);
        }

        private void SpawnHeavySkillVisual()
        {
            GameObject flash = new GameObject("_HeavyBreakerFlash");
            flash.transform.position = transform.position + Vector3.up * 0.8f;
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            renderer.color = selectedShip.AccentColor;
            flash.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.6f, 1.2f, 1f),
                new Vector3(1.4f, 3.2f, 1f),
                renderer.color,
                new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f),
                0.32f,
                191);
        }

        private IEnumerator SpawnNovaPulseSequence(float radius, Color color)
        {
            for (int i = 0; i < 2; i++)
            {
                yield return new WaitForSeconds(0.11f + i * 0.06f);
                GameObject pulse = new GameObject("_NovaPulse");
                pulse.transform.position = transform.position;
                SpriteRenderer renderer = pulse.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.SkillNovaCast) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
                renderer.color = new Color(color.r, color.g, color.b, i == 0 ? 0.48f : 0.3f);
                pulse.AddComponent<TransientSpriteEffect>().Setup(
                    new Vector3(0.85f + i * 0.18f, 0.85f + i * 0.18f, 1f),
                    new Vector3(radius * (1.15f + i * 0.1f), radius * (1.15f + i * 0.1f), 1f),
                    renderer.color,
                    new Color(color.r, color.g, color.b, 0f),
                    0.32f + i * 0.05f,
                    188 - i);
            }
        }

        private void SpawnNovaArcEffect(Vector3 targetPosition, Color color)
        {
            Vector3 delta = targetPosition - transform.position;
            float length = delta.magnitude;
            if (length <= 0.15f)
            {
                return;
            }

            GameObject arc = new GameObject("_NovaArc");
            arc.transform.position = transform.position + delta * 0.5f;
            arc.transform.rotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            SpriteRenderer renderer = arc.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.PlayerBullet) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Bullet);
            renderer.color = new Color(color.r, color.g, color.b, 0.88f);
            arc.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.18f, length * 0.55f, 1f),
                new Vector3(0.06f, length * 1.05f, 1f),
                renderer.color,
                new Color(color.r, color.g, color.b, 0f),
                0.16f,
                193);

            GameObject spark = new GameObject("_NovaArcSpark");
            spark.transform.position = targetPosition;
            SpriteRenderer sparkRenderer = spark.AddComponent<SpriteRenderer>();
            sparkRenderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            sparkRenderer.color = new Color(0.88f, 1f, 1f, 0.82f);
            spark.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.18f, 0.18f, 1f),
                new Vector3(0.7f, 0.7f, 1f),
                sparkRenderer.color,
                new Color(color.r, color.g, color.b, 0f),
                0.14f,
                194);
        }
    }
}

