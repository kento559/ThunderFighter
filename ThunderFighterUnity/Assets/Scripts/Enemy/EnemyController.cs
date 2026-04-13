using System.Collections.Generic;
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

        private const string BehaviorOverlayRootName = "_BehaviorOverlay";
        private HealthComponent health;
        private float shootAt;
        private Vector3 spawnPosition;
        private float movementSeed;
        private EnemyBehaviorType behaviorType;
        private ProceduralShipVisual shipVisual;
        private bool usingEliteVariant;
        private int chapterIndex = 1;
        private AudioSource audioSource;
        private Transform behaviorOverlayRoot;
        private static AudioClip flankFireClip;
        private static AudioClip diveFireClip;
        private static AudioClip supportPulseClip;
        private float supportPulseAt;
        private float externalSpeedMultiplier = 1f;
        private float externalFireIntervalMultiplier = 1f;
        private float supportBoostUntil;
        private bool announcedSupportThreat;
        public bool IsEliteVariant => usingEliteVariant;
        public EnemyBehaviorType CurrentBehaviorType => behaviorType;
        public bool IsSupportVariant => behaviorType == EnemyBehaviorType.Support;

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

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            EnsureCombatAudio();
            BuildBehaviorOverlay();

            spawnPosition = transform.position;
            movementSeed = Random.Range(0f, 20f);
            shootAt = Time.time + Random.Range(0.2f, 0.8f);
            supportPulseAt = Time.time + Random.Range(0.8f, 1.4f);
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
            float moveSpeed = config.MoveSpeed * GetExternalSpeedMultiplier();

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
            else if (behaviorType == EnemyBehaviorType.Flank)
            {
                float phase = Time.time + movementSeed;
                float entry = Mathf.Clamp01(Mathf.InverseLerp(6f, 2.2f, transform.position.y));
                float offsetX = Mathf.Sin(phase * (config.StrafeFrequency * 0.7f)) * config.StrafeAmplitude * 1.4f;
                position.x = spawnPosition.x + Mathf.Lerp(0f, offsetX, entry);
                position.y -= moveSpeed * (1.05f + entry * 0.22f) * Time.deltaTime;
            }
            else if (behaviorType == EnemyBehaviorType.Dive)
            {
                float phase = Time.time + movementSeed;
                float lockStrength = Mathf.Clamp01(Mathf.InverseLerp(4.8f, 1.4f, transform.position.y));
                position.x = spawnPosition.x + Mathf.Sin(phase * (config.StrafeFrequency * 0.45f)) * config.StrafeAmplitude * 0.55f;
                position.y -= moveSpeed * Mathf.Lerp(0.9f, 1.85f, lockStrength) * Time.deltaTime;
            }
            else if (behaviorType == EnemyBehaviorType.Support)
            {
                float phase = Time.time + movementSeed;
                position.x = spawnPosition.x + Mathf.Sin(phase * (config.StrafeFrequency * 0.85f)) * config.StrafeAmplitude * 0.78f;
                position.y -= moveSpeed * (0.72f + 0.08f * Mathf.Sin(phase * 1.2f)) * Time.deltaTime;
                if (Time.time >= supportPulseAt)
                {
                    supportPulseAt = Time.time + (chapterIndex == 3 ? 1.5f : 1.9f);
                    EmitSupportPulse();
                }
            }
            else if (chapterIndex == 3 && !usingEliteVariant)
            {
                position.x = spawnPosition.x + Mathf.Sin((Time.time + movementSeed) * 2.4f) * 0.35f;
                position.y -= moveSpeed * Time.deltaTime;
            }
            else
            {
                position.y -= moveSpeed * Time.deltaTime;
            }

            transform.position = position;
            float entryBoost = Mathf.InverseLerp(-1.5f, 6.5f, transform.position.y);
            float strafeBoost = behaviorType == EnemyBehaviorType.Strafe ? 0.18f : behaviorType == EnemyBehaviorType.Flank ? 0.24f : behaviorType == EnemyBehaviorType.Dive ? 0.3f : behaviorType == EnemyBehaviorType.Support ? 0.12f : 0f;
            if (chapterIndex == 2 && usingEliteVariant)
            {
                strafeBoost += 0.12f;
            }
            shipVisual?.SetThrustBoost(Mathf.Clamp01(0.38f + entryBoost * 0.9f + strafeBoost));

            if (behaviorType == EnemyBehaviorType.Support && !announcedSupportThreat && transform.position.y < 4.6f)
            {
                announcedSupportThreat = true;
                GameEvents.RaiseCombatAnnouncement(LocalizationService.Text("Support craft enhancing hostiles", "支援敌机正在强化敌群"));
            }

            if (config.CanShoot && weapon != null && Time.time >= shootAt && behaviorType != EnemyBehaviorType.Support)
            {
                float interval = behaviorType == EnemyBehaviorType.Strafe ? config.ShootInterval * 0.85f :
                    behaviorType == EnemyBehaviorType.Flank ? config.ShootInterval * 0.8f :
                    behaviorType == EnemyBehaviorType.Dive ? config.ShootInterval * 0.72f :
                    config.ShootInterval;
                if (chapterIndex == 3 && usingEliteVariant)
                {
                    interval *= 0.84f;
                }
                interval *= GetExternalFireIntervalMultiplier();
                shootAt = Time.time + interval;
                FireByChapter();
                PlayBehaviorFireSfx();
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

            PickupSpawner.SpawnEnemyDrop(transform.position, usingEliteVariant, chapterIndex, behaviorType);

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

            if (CampaignRuntime.CurrentLevel != null && CampaignRuntime.CurrentLevel.ChapterIndex >= 2 && config.ScoreValue >= 240 && config.ScoreValue < 360 && Random.value < 0.18f)
            {
                return EnemyBehaviorType.Support;
            }

            if (config.ScoreValue >= 520 || config.MaxHp >= 9)
            {
                return EnemyBehaviorType.Dive;
            }

            if (config.ScoreValue >= 360 || (config.MaxHp >= 6 && CampaignRuntime.CurrentLevel != null && CampaignRuntime.CurrentLevel.ChapterIndex >= 2))
            {
                return EnemyBehaviorType.Flank;
            }

            return config.MaxHp >= 6 || config.ScoreValue >= 300 ? EnemyBehaviorType.Strafe : EnemyBehaviorType.Straight;
        }

        private bool ResolveEliteVariant()
        {
            if (config == null)
            {
                return false;
            }

            return behaviorType == EnemyBehaviorType.Strafe || behaviorType == EnemyBehaviorType.Support || config.MaxHp >= 6 || config.ScoreValue >= 300;
        }

        public void ForceRuntimeBehavior(EnemyBehaviorType forcedBehavior, bool forceElite = false)
        {
            behaviorType = forcedBehavior;
            usingEliteVariant = forceElite || forcedBehavior == EnemyBehaviorType.Support || usingEliteVariant;
            ApplyArtVariant();
            ApplyChapterVisuals();
            BuildBehaviorOverlay();
            supportPulseAt = Time.time + Random.Range(0.55f, 1.05f);
        }

        public void ApplySupportBoost(float duration, float moveMultiplier, float fireRateMultiplier)
        {
            supportBoostUntil = Mathf.Max(supportBoostUntil, Time.time + duration);
            externalSpeedMultiplier = Mathf.Max(externalSpeedMultiplier, moveMultiplier);
            externalFireIntervalMultiplier = Mathf.Min(externalFireIntervalMultiplier, fireRateMultiplier);
        }

        private float GetExternalSpeedMultiplier()
        {
            if (Time.time > supportBoostUntil)
            {
                externalSpeedMultiplier = 1f;
                externalFireIntervalMultiplier = 1f;
            }

            return externalSpeedMultiplier;
        }

        private float GetExternalFireIntervalMultiplier()
        {
            if (Time.time > supportBoostUntil)
            {
                externalSpeedMultiplier = 1f;
                externalFireIntervalMultiplier = 1f;
            }

            return externalFireIntervalMultiplier;
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
                        renderer.color = behaviorType == EnemyBehaviorType.Support ? new Color(0.84f, 1f, 0.94f, 1f) : usingEliteVariant ? new Color(1f, 0.92f, 0.92f, 1f) : new Color(1f, 0.96f, 0.94f, 0.96f);
                    }
                    break;
                case 3:
                    shipVisual.SetThrusterPalette(new Color(0.46f, 0.84f, 1f, 0.64f), new Color(0.88f, 0.96f, 1f, 0.98f));
                    if (renderer != null)
                    {
                        renderer.color = behaviorType == EnemyBehaviorType.Support ? new Color(0.86f, 1f, 0.96f, 1f) : usingEliteVariant ? new Color(0.9f, 0.98f, 1f, 1f) : new Color(0.86f, 0.94f, 1f, 0.96f);
                    }
                    break;
                default:
                    shipVisual.SetThrusterPalette(new Color(1f, 0.48f, 0.12f, 0.58f), new Color(1f, 0.8f, 0.5f, 0.94f));
                    if (renderer != null)
                    {
                        renderer.color = behaviorType == EnemyBehaviorType.Support ? new Color(0.92f, 1f, 0.96f, 1f) : Color.white;
                    }
                    break;
            }
        }

        private void BuildBehaviorOverlay()
        {
            Transform existing = transform.Find(BehaviorOverlayRootName);
            if (existing == null)
            {
                GameObject root = new GameObject(BehaviorOverlayRootName);
                root.transform.SetParent(transform, false);
                behaviorOverlayRoot = root.transform;
            }
            else
            {
                behaviorOverlayRoot = existing;
            }

            for (int i = behaviorOverlayRoot.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(behaviorOverlayRoot.GetChild(i).gameObject);
            }

            Color trim = behaviorType == EnemyBehaviorType.Dive
                ? new Color(1f, 0.76f, 0.42f, 0.78f)
                : behaviorType == EnemyBehaviorType.Support
                    ? new Color(0.58f, 1f, 0.86f, 0.76f)
                    : new Color(0.72f, 0.86f, 1f, 0.72f);

            if (behaviorType == EnemyBehaviorType.Flank)
            {
                CreateOverlayPart("FlankBladeLeft", new Vector3(-0.62f, 0.2f, 0f), new Vector3(0.24f, 0.54f, 1f), 28f, trim, 67, GeneratedSpriteKind.Wing);
                CreateOverlayPart("FlankBladeRight", new Vector3(0.62f, 0.2f, 0f), new Vector3(0.24f, 0.54f, 1f), -28f, trim, 67, GeneratedSpriteKind.Wing);
                CreateOverlayPart("FlankNose", new Vector3(0f, -0.46f, 0f), new Vector3(0.18f, 0.34f, 1f), 180f, new Color(1f, 0.84f, 0.54f, 0.82f), 68, GeneratedSpriteKind.Nose);
            }
            else if (behaviorType == EnemyBehaviorType.Dive)
            {
                CreateOverlayPart("DiveSpurLeft", new Vector3(-0.36f, 0.44f, 0f), new Vector3(0.18f, 0.46f, 1f), 16f, trim, 67, GeneratedSpriteKind.Engine);
                CreateOverlayPart("DiveSpurRight", new Vector3(0.36f, 0.44f, 0f), new Vector3(0.18f, 0.46f, 1f), -16f, trim, 67, GeneratedSpriteKind.Engine);
                CreateOverlayPart("DiveNose", new Vector3(0f, -0.58f, 0f), new Vector3(0.2f, 0.48f, 1f), 180f, new Color(1f, 0.62f, 0.22f, 0.9f), 68, GeneratedSpriteKind.Nose);
            }
            else if (behaviorType == EnemyBehaviorType.Support)
            {
                CreateOverlayPart("SupportHaloOuter", new Vector3(0f, 0.05f, 0f), new Vector3(1.18f, 1.18f, 1f), 0f, new Color(0.42f, 0.98f, 0.82f, 0.36f), 66, GeneratedSpriteKind.Ring);
                CreateOverlayPart("SupportHaloInner", new Vector3(0f, 0.05f, 0f), new Vector3(0.76f, 0.76f, 1f), 0f, new Color(0.82f, 1f, 0.92f, 0.44f), 67, GeneratedSpriteKind.Ring);
                CreateOverlayPart("SupportNode", new Vector3(0f, -0.18f, 0f), new Vector3(0.28f, 0.42f, 1f), 180f, new Color(0.56f, 1f, 0.88f, 0.88f), 68, GeneratedSpriteKind.Cockpit);
            }
        }

        private void CreateOverlayPart(string name, Vector3 localPosition, Vector3 localScale, float rotationZ, Color color, int sortingOrder, GeneratedSpriteKind spriteKind)
        {
            if (behaviorOverlayRoot == null)
            {
                return;
            }

            GameObject part = new GameObject(name);
            part.transform.SetParent(behaviorOverlayRoot, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            part.transform.localScale = localScale;
            SpriteRenderer renderer = VisualDebugSprite.Ensure(part, color, sortingOrder, 0.2f, spriteKind);
            renderer.color = color;
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
                    if (behaviorType == EnemyBehaviorType.Flank)
                    {
                        weapon.FireBurstPattern(4, 18f, Mathf.Sin(Time.time * 1.6f) * 10f, 1.02f, 1);
                        break;
                    }
                    weapon.FireBurstPattern(3, 22f, 0f, 0.94f, 1);
                    break;
                case 3:
                    if (behaviorType == EnemyBehaviorType.Dive)
                    {
                        weapon.FireBurstPattern(3, 10f, 0f, 1.22f, 2);
                        break;
                    }
                    weapon.FireBurstPattern(5, 34f, 0f, 1.08f, 1);
                    break;
                default:
                    if (behaviorType == EnemyBehaviorType.Flank)
                    {
                        weapon.FireBurstPattern(3, 14f, Mathf.Sin(Time.time * 1.8f) * 8f, 1f, 1);
                        break;
                    }
                    weapon.FireBurstPattern(2, 8f, 0f, 1.02f, 0);
                    break;
            }
        }

        private void PlayBehaviorFireSfx()
        {
            if (audioSource == null || !audioSource.isActiveAndEnabled)
            {
                return;
            }

            AudioClip clip = null;
            float volume = 0.08f;
            float pitch = 1f;

            if (behaviorType == EnemyBehaviorType.Flank)
            {
                clip = flankFireClip;
                pitch = 1.08f;
            }
            else if (behaviorType == EnemyBehaviorType.Dive)
            {
                clip = diveFireClip;
                volume = 0.1f;
                pitch = 0.88f;
            }
            else if (behaviorType == EnemyBehaviorType.Support)
            {
                clip = supportPulseClip;
                volume = 0.11f;
                pitch = 1.02f;
            }

            if (clip == null)
            {
                return;
            }

            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip, volume * GameSettingsService.SfxVolume);
            audioSource.pitch = 1f;
        }

        private static void EnsureCombatAudio()
        {
            if (flankFireClip == null)
            {
                flankFireClip = BuildTone("enemy-flank", 920f, 0.08f, 0.07f);
            }

            if (diveFireClip == null)
            {
                diveFireClip = BuildTone("enemy-dive", 420f, 0.11f, 0.1f);
            }

            if (supportPulseClip == null)
            {
                supportPulseClip = BuildTone("enemy-support", 620f, 0.18f, 0.09f);
            }
        }

        private void EmitSupportPulse()
        {
            if (behaviorOverlayRoot != null)
            {
                behaviorOverlayRoot.Rotate(0f, 0f, 24f);
            }

            GameObject ring = new GameObject("_SupportPulse");
            ring.transform.position = transform.position;
            SpriteRenderer renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            renderer.color = new Color(0.46f, 1f, 0.86f, 0.72f);
            ring.AddComponent<TransientSpriteEffect>().Setup(
                new Vector3(0.32f, 0.32f, 1f),
                new Vector3(2.2f, 2.2f, 1f),
                renderer.color,
                new Color(0.24f, 0.78f, 0.64f, 0f),
                0.32f,
                104);

            PlayBehaviorFireSfx();
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, chapterIndex >= 3 ? 3.2f : 2.6f);
            HashSet<EnemyController> buffed = new HashSet<EnemyController>();
            for (int i = 0; i < colliders.Length; i++)
            {
                EnemyController ally = colliders[i] != null ? colliders[i].GetComponent<EnemyController>() : null;
                if (ally == null || ally == this || buffed.Contains(ally))
                {
                    continue;
                }

                buffed.Add(ally);
                ally.ApplySupportBoost(chapterIndex >= 3 ? 2.2f : 1.7f, chapterIndex >= 3 ? 1.3f : 1.18f, chapterIndex >= 3 ? 0.72f : 0.82f);
            }
        }

        private static AudioClip BuildTone(string clipName, float frequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
