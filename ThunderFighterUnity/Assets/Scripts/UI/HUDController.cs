using System.Collections;
using System.Collections.Generic;
using ThunderFighter.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderFighter.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text hpText;
        [SerializeField] private Slider bossHpSlider;
        [SerializeField] private Slider bossDefenseSlider;
        [SerializeField] private Slider chapterProgressSlider;
        [SerializeField] private GameObject pausePanel;

        private Text comboText;
        private Text announcementText;
        private Text missionText;
        private Text objectiveProgressText;
        private Text bossStateText;
        private Text bossWindowText;
        private Text bossAttackModeText;
        private Text bossAttackWarningText;
        private Text skillText;
        private Text loadoutText;
        private Text weaponLevelText;
        private Text buffSummaryText;
        private Text pickupText;
        private Slider skillSlider;
        private Image skillNovaIcon;
        private Image skillOverdriveIcon;
        private Image skillGlowOverlay;
        private Image damageOverlay;
        private Image warningOverlay;
        private Image laserBurnOverlay;
        private Image edgeWarningTop;
        private Image edgeWarningBottom;
        private Image edgeWarningLeft;
        private Image edgeWarningRight;
        private Image bossLockFrame;
        private readonly List<Image> lockCrosshairParts = new List<Image>();
        private Image scorePanel;
        private Image hpPanel;
        private Image comboPanel;
        private Image announcementPanel;
        private Image missionPanel;
        private Image bossPanel;
        private Image bossStatePanel;
        private Image bossWindowPanel;
        private Image bossAttackModePanel;
        private Image bossAttackWarningPanel;
        private Image skillPanel;
        private Image pickupPanel;
        private Image scanlineOverlay;
        private readonly List<Image> bossSegmentMarkers = new List<Image>();
        private readonly List<Image> buffIcons = new List<Image>();
        private Coroutine comboRoutine;
        private Coroutine announcementRoutine;
        private Coroutine damageRoutine;
        private Coroutine warningRoutine;
        private Coroutine scorePulseRoutine;
        private Coroutine hpPulseRoutine;
        private Coroutine bossAlertRoutine;
        private Coroutine edgeWarningRoutine;
        private Coroutine bossLockRoutine;
        private Coroutine laserHitRoutine;
        private AudioSource hudAudio;
        private static AudioClip laserBurnClip;
        private int displayedScore;
        private int displayedHp;
        private int displayedMaxHp = 100;
        private float lastBossRatio = 1f;

        private void OnEnable()
        {
            LocalizationService.OnLanguageChanged += HandleLanguageChanged;
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnPlayerHpChanged += HandlePlayerHpChanged;
            GameEvents.OnPlayerDamaged += HandlePlayerDamaged;
            GameEvents.OnComboChanged += HandleComboChanged;
            GameEvents.OnBossHpChanged += HandleBossHpChanged;
            GameEvents.OnCombatAnnouncement += HandleCombatAnnouncement;
            GameEvents.OnSkillEnergyChanged += HandleSkillEnergyChanged;
            GameEvents.OnLoadoutChanged += HandleLoadoutChanged;
            GameEvents.OnBuffStatusChanged += HandleBuffStatusChanged;
            GameEvents.OnPickupCollected += HandlePickupCollected;
            GameEvents.OnThreatEdgePulse += HandleThreatEdgePulse;
            GameEvents.OnBossLockOnWarning += HandleBossLockOnWarning;
            GameEvents.OnBossLaserHit += HandleBossLaserHit;
            GameEvents.OnBossDefenseStateChanged += HandleBossDefenseStateChanged;
            GameEvents.OnTacticalProgressChanged += HandleTacticalProgressChanged;
            GameEvents.OnBossAttackTelemetryChanged += HandleBossAttackTelemetryChanged;
            GameEvents.OnBossPhaseTelemetryChanged += HandleBossPhaseTelemetryChanged;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            LocalizationService.OnLanguageChanged -= HandleLanguageChanged;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnPlayerHpChanged -= HandlePlayerHpChanged;
            GameEvents.OnPlayerDamaged -= HandlePlayerDamaged;
            GameEvents.OnComboChanged -= HandleComboChanged;
            GameEvents.OnBossHpChanged -= HandleBossHpChanged;
            GameEvents.OnCombatAnnouncement -= HandleCombatAnnouncement;
            GameEvents.OnSkillEnergyChanged -= HandleSkillEnergyChanged;
            GameEvents.OnLoadoutChanged -= HandleLoadoutChanged;
            GameEvents.OnBuffStatusChanged -= HandleBuffStatusChanged;
            GameEvents.OnPickupCollected -= HandlePickupCollected;
            GameEvents.OnThreatEdgePulse -= HandleThreatEdgePulse;
            GameEvents.OnBossLockOnWarning -= HandleBossLockOnWarning;
            GameEvents.OnBossLaserHit -= HandleBossLaserHit;
            GameEvents.OnBossDefenseStateChanged -= HandleBossDefenseStateChanged;
            GameEvents.OnTacticalProgressChanged -= HandleTacticalProgressChanged;
            GameEvents.OnBossAttackTelemetryChanged -= HandleBossAttackTelemetryChanged;
            GameEvents.OnBossPhaseTelemetryChanged -= HandleBossPhaseTelemetryChanged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            EnsureHudAudio();
            StyleHud();
            EnsureRuntimeHudElements();
            HandleScoreChanged(0);
            HandlePlayerHpChanged(100, 100);
            HandleComboChanged(0, 1f);
            HandleBossHpChanged(0f);
            HandleSkillEnergyChanged(1f, LocalizationService.IsChinese ? "K 新星 40  |  L 过载 60" : "K NOVA 40  |  L OVERDRIVE 60");
            HandleLoadoutChanged(LocalizationService.IsChinese ? "先锋型" : "VANGUARD", 1);
            HandleBuffStatusChanged(LocalizationService.IsChinese ? "无临时增幅" : "No active buffs", new PickupBuffType[0]);
            if (missionText != null)
            {
                missionText.text = LocalizationService.IsChinese ? "战术目标: 清空当前空域威胁" : "TACTICAL: CLEAR HOSTILE AIRSPACE";
            }
            if (objectiveProgressText != null)
            {
                objectiveProgressText.text = LocalizationService.IsChinese ? "阶段 1 / 1" : "PHASE 1 / 1";
            }
            if (chapterProgressSlider != null)
            {
                chapterProgressSlider.value = 0f;
            }
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (scanlineOverlay != null)
            {
                RectTransform rect = scanlineOverlay.rectTransform;
                rect.anchoredPosition = new Vector2(0f, Mathf.Repeat(Time.unscaledTime * -26f, 16f));
            }
        }

        private void EnsureRuntimeHudElements()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                return;
            }

            TerminalUiFactory.EnsureCanvasRuntimeSettings(canvas);

            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            bool compact = aspect < 1.72f;
            bool ultraCompact = aspect < 1.5f;

            if (comboText == null)
            {
                comboText = CreateRuntimeText(canvas.transform, "ComboText", ultraCompact ? new Vector2(0.865f, 0.87f) : new Vector2(0.88f, 0.885f), ultraCompact ? new Vector2(180f, 34f) : new Vector2(220f, 38f), ultraCompact ? 16 : 18, TextAnchor.MiddleRight);
                comboText.text = string.Empty;
                comboPanel = EnsureBackingPanel(comboText, new Color(0.16f, 0.1f, 0.02f, 0f), new Vector2(18f, 10f));
            }

            if (announcementText == null)
            {
                announcementText = CreateRuntimeText(canvas.transform, "AnnouncementText", ultraCompact ? new Vector2(0.5f, 0.77f) : new Vector2(0.5f, 0.805f), ultraCompact ? new Vector2(420f, 46f) : new Vector2(540f, 50f), ultraCompact ? 19 : 23, TextAnchor.MiddleCenter);
                announcementText.text = string.Empty;
                announcementPanel = EnsureBackingPanel(announcementText, new Color(0.2f, 0.12f, 0.04f, 0f), new Vector2(28f, 14f));
            }

            if (missionText == null)
            {
                missionText = CreateRuntimeText(canvas.transform, "MissionText", ultraCompact ? new Vector2(0.5f, 0.72f) : new Vector2(0.5f, 0.75f), ultraCompact ? new Vector2(450f, 28f) : new Vector2(560f, 30f), ultraCompact ? 12 : 14, TextAnchor.MiddleCenter);
                missionText.color = new Color(0.72f, 0.88f, 0.98f, 0.84f);
                missionPanel = EnsureBackingPanel(missionText, new Color(0.04f, 0.08f, 0.14f, 0.44f), new Vector2(20f, 10f));
            }

            if (objectiveProgressText == null)
            {
                objectiveProgressText = CreateRuntimeText(canvas.transform, "ObjectiveProgressText", ultraCompact ? new Vector2(0.5f, 0.69f) : new Vector2(0.5f, 0.715f), ultraCompact ? new Vector2(360f, 24f) : new Vector2(440f, 26f), ultraCompact ? 11 : 12, TextAnchor.MiddleCenter);
                objectiveProgressText.color = new Color(1f, 0.88f, 0.62f, 0.82f);
                EnsureBackingPanel(objectiveProgressText, new Color(0.1f, 0.08f, 0.04f, 0.38f), new Vector2(16f, 8f));
            }

            if (chapterProgressSlider == null)
            {
                GameObject root = new GameObject("ChapterProgressSlider");
                root.transform.SetParent(canvas.transform, false);
                RectTransform rect = root.AddComponent<RectTransform>();
                rect.anchorMin = ultraCompact ? new Vector2(0.5f, 0.655f) : new Vector2(0.5f, 0.678f);
                rect.anchorMax = ultraCompact ? new Vector2(0.5f, 0.655f) : new Vector2(0.5f, 0.678f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = ultraCompact ? new Vector2(300f, 8f) : new Vector2(380f, 10f);

                GameObject background = new GameObject("Background");
                background.transform.SetParent(root.transform, false);
                RectTransform bgRect = background.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                Image bgImage = background.AddComponent<Image>();
                bgImage.color = new Color(0.06f, 0.1f, 0.16f, 0.8f);

                GameObject fillArea = new GameObject("Fill Area");
                fillArea.transform.SetParent(root.transform, false);
                RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
                fillAreaRect.anchorMin = new Vector2(0.02f, 0.16f);
                fillAreaRect.anchorMax = new Vector2(0.98f, 0.84f);
                fillAreaRect.offsetMin = Vector2.zero;
                fillAreaRect.offsetMax = Vector2.zero;

                GameObject fill = new GameObject("Fill");
                fill.transform.SetParent(fillArea.transform, false);
                RectTransform fillRect = fill.AddComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                Image fillImage = fill.AddComponent<Image>();
                fillImage.color = new Color(0.52f, 0.9f, 1f, 0.96f);

                chapterProgressSlider = root.AddComponent<Slider>();
                chapterProgressSlider.transition = Selectable.Transition.None;
                chapterProgressSlider.fillRect = fillRect;
                chapterProgressSlider.targetGraphic = fillImage;
                chapterProgressSlider.direction = Slider.Direction.LeftToRight;
                chapterProgressSlider.minValue = 0f;
                chapterProgressSlider.maxValue = 1f;
                chapterProgressSlider.value = 0f;
                EnsureBackingPanel(fillImage, new Color(0.04f, 0.08f, 0.14f, 0.38f), new Vector2(16f, 8f));
            }

            if (skillSlider == null)
            {
                GameObject root = new GameObject("SkillSlider");
                root.transform.SetParent(canvas.transform, false);
                RectTransform rect = root.AddComponent<RectTransform>();
                rect.anchorMin = ultraCompact ? new Vector2(0.5f, 0.06f) : new Vector2(0.5f, 0.07f);
                rect.anchorMax = ultraCompact ? new Vector2(0.5f, 0.06f) : new Vector2(0.5f, 0.07f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = ultraCompact ? new Vector2(260f, 14f) : new Vector2(320f, 16f);

                GameObject background = new GameObject("Background");
                background.transform.SetParent(root.transform, false);
                RectTransform bgRect = background.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                Image bgImage = background.AddComponent<Image>();
                bgImage.color = new Color(0.08f, 0.11f, 0.18f, 0.92f);

                GameObject fillArea = new GameObject("Fill Area");
                fillArea.transform.SetParent(root.transform, false);
                RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
                fillAreaRect.anchorMin = new Vector2(0.02f, 0.14f);
                fillAreaRect.anchorMax = new Vector2(0.98f, 0.86f);
                fillAreaRect.offsetMin = Vector2.zero;
                fillAreaRect.offsetMax = Vector2.zero;

                GameObject fill = new GameObject("Fill");
                fill.transform.SetParent(fillArea.transform, false);
                RectTransform fillRect = fill.AddComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                Image fillImage = fill.AddComponent<Image>();
                fillImage.color = new Color(0.42f, 0.88f, 1f, 1f);

                skillSlider = root.AddComponent<Slider>();
                skillSlider.transition = Selectable.Transition.None;
                skillSlider.fillRect = fillRect;
                skillSlider.targetGraphic = fillImage;
                skillSlider.direction = Slider.Direction.LeftToRight;
                skillSlider.minValue = 0f;
                skillSlider.maxValue = 1f;
                skillSlider.value = 1f;
                skillPanel = EnsureBackingPanel(fillImage, new Color(0.04f, 0.08f, 0.16f, 0.78f), new Vector2(18f, 14f));

                GameObject glow = new GameObject("SkillGlow");
                glow.transform.SetParent(root.transform, false);
                RectTransform glowRect = glow.AddComponent<RectTransform>();
                glowRect.anchorMin = new Vector2(0.02f, 0.14f);
                glowRect.anchorMax = new Vector2(0.98f, 0.86f);
                glowRect.offsetMin = Vector2.zero;
                glowRect.offsetMax = Vector2.zero;
                skillGlowOverlay = glow.AddComponent<Image>();
                skillGlowOverlay.color = new Color(0.62f, 0.94f, 1f, 0.08f);
                skillGlowOverlay.raycastTarget = false;

                skillText = CreateRuntimeText(canvas.transform, "SkillText", ultraCompact ? new Vector2(0.5f, 0.028f) : new Vector2(0.5f, 0.036f), ultraCompact ? new Vector2(300f, 24f) : new Vector2(380f, 28f), ultraCompact ? 12 : 15, TextAnchor.MiddleCenter);
                skillPanel = skillPanel ?? EnsureBackingPanel(skillText, new Color(0.04f, 0.08f, 0.16f, 0.78f), new Vector2(18f, 14f));
                skillNovaIcon = CreateSkillIcon(canvas.transform, "NovaIcon", ultraCompact ? new Vector2(0.35f, 0.06f) : new Vector2(0.375f, 0.07f), GeneratedSpriteKind.Ring, new Color(0.52f, 0.9f, 1f, 0.92f));
                skillOverdriveIcon = CreateSkillIcon(canvas.transform, "OverdriveIcon", ultraCompact ? new Vector2(0.65f, 0.06f) : new Vector2(0.625f, 0.07f), GeneratedSpriteKind.Thruster, new Color(1f, 0.82f, 0.36f, 0.92f));
            }

            if (loadoutText == null)
            {
                loadoutText = CreateRuntimeText(canvas.transform, "LoadoutText", ultraCompact ? new Vector2(0.14f, 0.85f) : new Vector2(0.135f, 0.855f), ultraCompact ? new Vector2(200f, 24f) : new Vector2(220f, 26f), ultraCompact ? 13 : 15, TextAnchor.MiddleLeft);
                loadoutText.color = new Color(0.84f, 0.96f, 1f, 1f);
                EnsureBackingPanel(loadoutText, new Color(0.04f, 0.08f, 0.16f, 0.72f), new Vector2(18f, 12f));
            }

            if (weaponLevelText == null)
            {
                weaponLevelText = CreateRuntimeText(canvas.transform, "WeaponLevelText", ultraCompact ? new Vector2(0.14f, 0.815f) : new Vector2(0.135f, 0.815f), ultraCompact ? new Vector2(200f, 24f) : new Vector2(220f, 26f), ultraCompact ? 12 : 14, TextAnchor.MiddleLeft);
                weaponLevelText.color = new Color(1f, 0.86f, 0.58f, 0.96f);
                EnsureBackingPanel(weaponLevelText, new Color(0.1f, 0.08f, 0.04f, 0.7f), new Vector2(18f, 12f));
            }

            if (buffSummaryText == null)
            {
                buffSummaryText = CreateRuntimeText(canvas.transform, "BuffSummaryText", ultraCompact ? new Vector2(0.84f, 0.155f) : new Vector2(0.86f, 0.155f), ultraCompact ? new Vector2(210f, 36f) : new Vector2(260f, 40f), ultraCompact ? 11 : 13, TextAnchor.MiddleRight);
                buffSummaryText.color = new Color(0.82f, 0.94f, 1f, 0.95f);
                EnsureBackingPanel(buffSummaryText, new Color(0.04f, 0.08f, 0.16f, 0.72f), new Vector2(18f, 14f));
            }

            if (pickupText == null)
            {
                pickupText = CreateRuntimeText(canvas.transform, "PickupText", ultraCompact ? new Vector2(0.5f, 0.135f) : new Vector2(0.5f, 0.145f), ultraCompact ? new Vector2(320f, 28f) : new Vector2(380f, 30f), ultraCompact ? 13 : 15, TextAnchor.MiddleCenter);
                pickupText.color = new Color(1f, 0.94f, 0.72f, 0f);
                pickupPanel = EnsureBackingPanel(pickupText, new Color(0.12f, 0.08f, 0.04f, 0f), new Vector2(22f, 12f));
            }

            if (buffIcons.Count == 0)
            {
                float[] xs = ultraCompact ? new[] { 0.72f, 0.775f, 0.83f, 0.885f, 0.94f } : new[] { 0.72f, 0.775f, 0.83f, 0.885f, 0.94f };
                for (int i = 0; i < xs.Length; i++)
                {
                    buffIcons.Add(CreateSkillIcon(canvas.transform, "BuffIcon_" + i, new Vector2(xs[i], ultraCompact ? 0.095f : 0.105f), GeneratedSpriteKind.Engine, new Color(0.5f, 0.74f, 0.9f, 0f)));
                }
            }

            if (damageOverlay == null)
            {
                GameObject overlay = new GameObject("DamageOverlay");
                overlay.transform.SetParent(canvas.transform, false);
                RectTransform rect = overlay.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                damageOverlay = overlay.AddComponent<Image>();
                damageOverlay.color = new Color(1f, 0.08f, 0.08f, 0f);
                overlay.transform.SetAsFirstSibling();
            }

            if (warningOverlay == null)
            {
                GameObject overlay = new GameObject("WarningOverlay");
                overlay.transform.SetParent(canvas.transform, false);
                RectTransform rect = overlay.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                warningOverlay = overlay.AddComponent<Image>();
                warningOverlay.color = new Color(1f, 0.36f, 0.08f, 0f);
                overlay.transform.SetAsFirstSibling();
            }

            if (laserBurnOverlay == null)
            {
                GameObject overlay = new GameObject("LaserBurnOverlay");
                overlay.transform.SetParent(canvas.transform, false);
                RectTransform rect = overlay.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                laserBurnOverlay = overlay.AddComponent<Image>();
                laserBurnOverlay.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
                laserBurnOverlay.type = Image.Type.Sliced;
                laserBurnOverlay.color = new Color(1f, 0.76f, 0.42f, 0f);
                overlay.transform.SetAsFirstSibling();
            }

            if (scanlineOverlay == null)
            {
                GameObject overlay = new GameObject("ScanlineOverlay");
                overlay.transform.SetParent(canvas.transform, false);
                RectTransform rect = overlay.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                scanlineOverlay = overlay.AddComponent<Image>();
                scanlineOverlay.sprite = BuildScanlineSprite();
                scanlineOverlay.type = Image.Type.Tiled;
                scanlineOverlay.color = new Color(0.7f, 0.92f, 1f, 0.055f);
                overlay.transform.SetAsFirstSibling();
            }

            if (edgeWarningTop == null)
            {
                edgeWarningTop = CreateEdgeWarning(canvas.transform, "EdgeWarningTop", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(760f, 16f));
                edgeWarningBottom = CreateEdgeWarning(canvas.transform, "EdgeWarningBottom", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(760f, 16f));
                edgeWarningLeft = CreateEdgeWarning(canvas.transform, "EdgeWarningLeft", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(16f, 420f));
                edgeWarningRight = CreateEdgeWarning(canvas.transform, "EdgeWarningRight", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(16f, 420f));
            }

            if (bossLockFrame == null)
            {
                BuildBossLockUi(canvas.transform);
            }

            if (bossStateText == null)
            {
                bossStateText = CreateRuntimeText(canvas.transform, "BossStateText", ultraCompact ? new Vector2(0.5f, 0.875f) : new Vector2(0.5f, 0.89f), ultraCompact ? new Vector2(340f, 24f) : new Vector2(420f, 26f), ultraCompact ? 12 : 14, TextAnchor.MiddleCenter);
                bossStateText.color = new Color(1f, 0.88f, 0.62f, 0f);
                bossStatePanel = EnsureBackingPanel(bossStateText, new Color(0.12f, 0.08f, 0.04f, 0f), new Vector2(18f, 10f));
            }

            if (bossDefenseSlider == null)
            {
                GameObject root = new GameObject("BossDefenseSlider");
                root.transform.SetParent(canvas.transform, false);
                RectTransform rect = root.AddComponent<RectTransform>();
                rect.anchorMin = ultraCompact ? new Vector2(0.5f, 0.855f) : new Vector2(0.5f, 0.868f);
                rect.anchorMax = ultraCompact ? new Vector2(0.5f, 0.855f) : new Vector2(0.5f, 0.868f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = ultraCompact ? new Vector2(260f, 10f) : new Vector2(320f, 12f);

                GameObject background = new GameObject("Background");
                background.transform.SetParent(root.transform, false);
                RectTransform bgRect = background.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                Image bgImage = background.AddComponent<Image>();
                bgImage.color = new Color(0.08f, 0.11f, 0.18f, 0.88f);

                GameObject fillArea = new GameObject("Fill Area");
                fillArea.transform.SetParent(root.transform, false);
                RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
                fillAreaRect.anchorMin = new Vector2(0.02f, 0.12f);
                fillAreaRect.anchorMax = new Vector2(0.98f, 0.88f);
                fillAreaRect.offsetMin = Vector2.zero;
                fillAreaRect.offsetMax = Vector2.zero;

                GameObject fill = new GameObject("Fill");
                fill.transform.SetParent(fillArea.transform, false);
                RectTransform fillRect = fill.AddComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                Image fillImage = fill.AddComponent<Image>();
                fillImage.color = new Color(0.54f, 0.88f, 1f, 1f);

                bossDefenseSlider = root.AddComponent<Slider>();
                bossDefenseSlider.transition = Selectable.Transition.None;
                bossDefenseSlider.fillRect = fillRect;
                bossDefenseSlider.targetGraphic = fillImage;
                bossDefenseSlider.direction = Slider.Direction.LeftToRight;
                bossDefenseSlider.minValue = 0f;
                bossDefenseSlider.maxValue = 1f;
                bossDefenseSlider.value = 1f;
                Image defensePanel = EnsureBackingPanel(fillImage, new Color(0.04f, 0.08f, 0.16f, 0f), new Vector2(16f, 10f));
                if (defensePanel != null)
                {
                    defensePanel.gameObject.SetActive(false);
                }
            }

            if (bossWindowText == null)
            {
                bossWindowText = CreateRuntimeText(canvas.transform, "BossWindowText", ultraCompact ? new Vector2(0.5f, 0.835f) : new Vector2(0.5f, 0.845f), ultraCompact ? new Vector2(280f, 22f) : new Vector2(320f, 24f), ultraCompact ? 11 : 12, TextAnchor.MiddleCenter);
                bossWindowText.color = new Color(1f, 0.9f, 0.66f, 0f);
                bossWindowPanel = EnsureBackingPanel(bossWindowText, new Color(0.12f, 0.08f, 0.04f, 0f), new Vector2(16f, 8f));
            }

            if (bossAttackModeText == null)
            {
                bossAttackModeText = CreateRuntimeText(canvas.transform, "BossAttackModeText", ultraCompact ? new Vector2(0.5f, 0.807f) : new Vector2(0.5f, 0.818f), ultraCompact ? new Vector2(300f, 22f) : new Vector2(360f, 24f), ultraCompact ? 11 : 12, TextAnchor.MiddleCenter);
                bossAttackModeText.color = new Color(0.76f, 0.92f, 1f, 0f);
                bossAttackModePanel = EnsureBackingPanel(bossAttackModeText, new Color(0.06f, 0.1f, 0.16f, 0f), new Vector2(16f, 8f));
            }

            if (bossAttackWarningText == null)
            {
                bossAttackWarningText = CreateRuntimeText(canvas.transform, "BossAttackWarningText", ultraCompact ? new Vector2(0.5f, 0.78f) : new Vector2(0.5f, 0.79f), ultraCompact ? new Vector2(340f, 22f) : new Vector2(400f, 24f), ultraCompact ? 11 : 12, TextAnchor.MiddleCenter);
                bossAttackWarningText.color = new Color(1f, 0.86f, 0.62f, 0f);
                bossAttackWarningPanel = EnsureBackingPanel(bossAttackWarningText, new Color(0.12f, 0.08f, 0.04f, 0f), new Vector2(16f, 8f));
            }
        }

        private void StyleHud()
        {
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            bool compact = aspect < 1.72f;
            bool ultraCompact = aspect < 1.5f;

            if (scoreText != null)
            {
                RectTransform scoreRect = scoreText.rectTransform;
                scoreRect.anchorMin = ultraCompact ? new Vector2(0.86f, 0.945f) : new Vector2(0.88f, 0.95f);
                scoreRect.anchorMax = ultraCompact ? new Vector2(0.86f, 0.945f) : new Vector2(0.88f, 0.95f);
                scoreRect.pivot = new Vector2(0.5f, 0.5f);
                scoreRect.sizeDelta = ultraCompact ? new Vector2(220f, 40f) : new Vector2(260f, 44f);
                scoreText.alignment = TextAnchor.MiddleRight;
            }

            if (hpText != null)
            {
                RectTransform hpRect = hpText.rectTransform;
                hpRect.anchorMin = ultraCompact ? new Vector2(0.14f, 0.945f) : new Vector2(0.12f, 0.95f);
                hpRect.anchorMax = ultraCompact ? new Vector2(0.14f, 0.945f) : new Vector2(0.12f, 0.95f);
                hpRect.pivot = new Vector2(0.5f, 0.5f);
                hpRect.sizeDelta = ultraCompact ? new Vector2(220f, 40f) : new Vector2(260f, 44f);
                hpText.alignment = TextAnchor.MiddleLeft;
            }

            if (bossHpSlider != null)
            {
                RectTransform bossRect = bossHpSlider.GetComponent<RectTransform>();
                if (bossRect != null)
                {
                    bossRect.anchorMin = ultraCompact ? new Vector2(0.5f, 0.915f) : new Vector2(0.5f, 0.93f);
                    bossRect.anchorMax = ultraCompact ? new Vector2(0.5f, 0.915f) : new Vector2(0.5f, 0.93f);
                    bossRect.pivot = new Vector2(0.5f, 0.5f);
                    bossRect.sizeDelta = ultraCompact ? new Vector2(380f, 18f) : (compact ? new Vector2(450f, 20f) : new Vector2(520f, 20f));
                }
            }

            StyleText(scoreText, new Color(0.45f, 0.95f, 1f, 1f));
            StyleText(hpText, new Color(0.67f, 1f, 0.84f, 1f));
            scorePanel = EnsureBackingPanel(scoreText, new Color(0.04f, 0.1f, 0.16f, 0.72f), new Vector2(18f, 12f));
            hpPanel = EnsureBackingPanel(hpText, new Color(0.04f, 0.12f, 0.12f, 0.72f), new Vector2(18f, 12f));

            if (bossHpSlider != null)
            {
                Graphic targetGraphic = bossHpSlider.fillRect != null ? bossHpSlider.fillRect.GetComponent<Graphic>() : bossHpSlider.GetComponent<Graphic>();
                bossPanel = EnsureBackingPanel(targetGraphic, new Color(0.12f, 0.08f, 0.04f, 0.72f), new Vector2(22f, 16f));
                Image[] images = bossHpSlider.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i].name == "Fill")
                    {
                        images[i].color = new Color(1f, 0.34f, 0.18f, 1f);
                    }
                    else if (images[i].name == "Background")
                    {
                        images[i].color = new Color(0.08f, 0.11f, 0.18f, 0.92f);
                    }
                }

                BuildBossSegments();
            }

            if (pausePanel != null)
            {
                Image panelImage = pausePanel.GetComponent<Image>();
                if (panelImage != null)
                {
                    panelImage.color = new Color(0.01f, 0.04f, 0.08f, 0.62f);
                }
            }
        }

        private static void StyleText(Text text, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.color = color;
            text.fontStyle = FontStyle.Bold;
            Outline outline = text.GetComponent<Outline>() ?? text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.06f, 0.12f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            Shadow shadow = text.GetComponent<Shadow>() ?? text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -4f);
        }

        private Text CreateRuntimeText(Transform parent, string objectName, Vector2 anchor, Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Text text = go.AddComponent<Text>();
            text.font = TerminalUiFactory.GetUiFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            StyleText(text, Color.white);
            return text;
        }

        private Image CreateSkillIcon(Transform parent, string objectName, Vector2 anchor, GeneratedSpriteKind spriteKind, Color color)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(24f, 24f);
            Image image = go.AddComponent<Image>();
            image.sprite = GeneratedSpriteLibrary.Get(spriteKind);
            image.color = color;
            EnsureBackingPanel(image, new Color(0.04f, 0.08f, 0.16f, 0.78f), new Vector2(12f, 12f));
            return image;
        }

        private Image CreateEdgeWarning(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 0.42f, 0.08f, 0f);
            image.raycastTarget = false;
            go.transform.SetAsLastSibling();
            return image;
        }

        private void BuildBossLockUi(Transform parent)
        {
            GameObject frame = new GameObject("BossLockFrame");
            frame.transform.SetParent(parent, false);
            RectTransform rect = frame.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.44f);
            rect.anchorMax = new Vector2(0.5f, 0.44f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(92f, 150f);
            bossLockFrame = frame.AddComponent<Image>();
            bossLockFrame.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            bossLockFrame.type = Image.Type.Sliced;
            bossLockFrame.color = new Color(0.56f, 0.96f, 1f, 0f);
            bossLockFrame.raycastTarget = false;
            frame.transform.SetAsLastSibling();

            lockCrosshairParts.Clear();
            Vector2[] anchors =
            {
                new Vector2(0.5f, 0.56f),
                new Vector2(0.5f, 0.56f),
                new Vector2(0.5f, 0.56f),
                new Vector2(0.5f, 0.56f)
            };
            Vector2[] starts =
            {
                new Vector2(0f, 120f),
                new Vector2(0f, -120f),
                new Vector2(-120f, 0f),
                new Vector2(120f, 0f)
            };
            Vector2[] sizes =
            {
                new Vector2(12f, 52f),
                new Vector2(12f, 52f),
                new Vector2(52f, 12f),
                new Vector2(52f, 12f)
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject part = new GameObject("LockCrosshair_" + i);
                part.transform.SetParent(parent, false);
                RectTransform partRect = part.AddComponent<RectTransform>();
                partRect.anchorMin = anchors[i];
                partRect.anchorMax = anchors[i];
                partRect.pivot = new Vector2(0.5f, 0.5f);
                partRect.anchoredPosition = starts[i];
                partRect.sizeDelta = sizes[i];
                Image image = part.AddComponent<Image>();
                image.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
                image.type = Image.Type.Sliced;
                image.color = new Color(0.6f, 0.96f, 1f, 0f);
                image.raycastTarget = false;
                part.transform.SetAsLastSibling();
                lockCrosshairParts.Add(image);
            }
        }

        private Image EnsureBackingPanel(Graphic target, Color color, Vector2 padding)
        {
            if (target == null)
            {
                return null;
            }

            string panelName = string.Format("{0}_Panel", target.name);
            Transform existing = target.transform.parent != null ? target.transform.parent.Find(panelName) : null;
            if (existing != null)
            {
                Image existingImage = existing.GetComponent<Image>();
                if (existingImage != null)
                {
                    existingImage.color = color;
                    return existingImage;
                }
            }

            RectTransform targetRect = target.rectTransform;
            GameObject go = new GameObject(panelName);
            go.transform.SetParent(targetRect.parent, false);
            go.transform.SetSiblingIndex(targetRect.GetSiblingIndex());
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = targetRect.anchorMin;
            rect.anchorMax = targetRect.anchorMax;
            rect.pivot = targetRect.pivot;
            rect.anchoredPosition = targetRect.anchoredPosition;
            rect.sizeDelta = targetRect.sizeDelta + padding;
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Hull);
            image.type = Image.Type.Sliced;
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.65f, 0.82f, 1f, 0.12f);
            outline.effectDistance = new Vector2(1f, -1f);
            AddPanelCorners(go.transform, image.color.a > 0.01f ? color : new Color(0.22f, 0.78f, 1f, 0.24f));
            return image;
        }

        private void BuildBossSegments()
        {
            if (bossHpSlider == null || bossHpSlider.transform.parent == null || bossSegmentMarkers.Count > 0)
            {
                return;
            }

            RectTransform fillArea = bossHpSlider.fillRect != null ? bossHpSlider.fillRect.parent as RectTransform : null;
            if (fillArea == null)
            {
                return;
            }

            float[] thresholds = { 0.7f, 0.4f };
            for (int i = 0; i < thresholds.Length; i++)
            {
                GameObject marker = new GameObject("BossSegment_" + i);
                marker.transform.SetParent(fillArea, false);
                RectTransform rect = marker.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(thresholds[i], 0.08f);
                rect.anchorMax = new Vector2(thresholds[i], 0.92f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(5f, 0f);
                Image img = marker.AddComponent<Image>();
                img.color = new Color(1f, 0.92f, 0.68f, 0.42f);
                bossSegmentMarkers.Add(img);
            }
        }

        private void HandleScoreChanged(int score)
        {
            if (scoreText == null)
            {
                return;
            }

            if (scorePulseRoutine != null)
            {
                StopCoroutine(scorePulseRoutine);
            }

            scorePulseRoutine = StartCoroutine(AnimateNumericText(scoreText, displayedScore, score, LocalizationService.IsChinese ? "分数: " : "Score: ", new Color(0.6f, 0.96f, 1f, 1f), 0.22f));
            displayedScore = score;
        }

        private void HandlePlayerHpChanged(int currentHp, int maxHp)
        {
            if (hpText == null)
            {
                return;
            }

            bool hpDropped = currentHp < displayedHp;
            displayedHp = currentHp;
            displayedMaxHp = maxHp;
            if (hpPulseRoutine != null)
            {
                StopCoroutine(hpPulseRoutine);
            }

            hpPulseRoutine = StartCoroutine(PulseText(hpText, hpDropped ? new Color(1f, 0.44f, 0.44f, 1f) : new Color(0.72f, 1f, 0.84f, 1f), hpDropped ? 1.18f : 1.08f, 0.2f));
            hpText.text = LocalizationService.IsChinese ? string.Format("生命: {0}/{1}", currentHp, maxHp) : string.Format("HP: {0}/{1}", currentHp, maxHp);

            if (hpPanel != null)
            {
                float danger = maxHp > 0 ? 1f - ((float)currentHp / maxHp) : 1f;
                hpPanel.color = Color.Lerp(new Color(0.04f, 0.12f, 0.12f, 0.72f), new Color(0.3f, 0.08f, 0.08f, 0.92f), danger);
            }
        }

        private void HandlePlayerDamaged(int currentHp, int maxHp)
        {
            if (damageOverlay == null)
            {
                return;
            }

            if (damageRoutine != null)
            {
                StopCoroutine(damageRoutine);
            }

            damageRoutine = StartCoroutine(FlashDamageOverlay(currentHp <= 2 ? 0.34f : 0.22f));
        }

        private void HandleComboChanged(int comboCount, float multiplier)
        {
            if (comboText == null)
            {
                return;
            }

            if (comboCount <= 1)
            {
                comboText.text = string.Empty;
                if (comboPanel != null)
                {
                    comboPanel.color = new Color(comboPanel.color.r, comboPanel.color.g, comboPanel.color.b, 0f);
                }
                return;
            }

            comboText.text = LocalizationService.IsChinese ? string.Format("连击 x{0:0.0}", multiplier) : string.Format("COMBO x{0:0.0}", multiplier);
            if (comboPanel != null)
            {
                comboPanel.color = new Color(0.16f, 0.1f, 0.02f, 0.62f);
            }

            if (comboRoutine != null)
            {
                StopCoroutine(comboRoutine);
            }

            comboRoutine = StartCoroutine(PulseText(comboText, new Color(1f, 0.9f, 0.35f, 1f), 1.12f, 0.2f));
        }

        private void HandleBossHpChanged(float ratio)
        {
            if (bossHpSlider == null)
            {
                return;
            }

            bool hasBoss = ratio > 0f;
            bossHpSlider.gameObject.SetActive(hasBoss);
            if (bossPanel != null)
            {
                bossPanel.gameObject.SetActive(hasBoss);
            }

            for (int i = 0; i < bossSegmentMarkers.Count; i++)
            {
                bossSegmentMarkers[i].gameObject.SetActive(hasBoss);
            }

            if (!hasBoss)
            {
                lastBossRatio = 1f;
                bossHpSlider.value = 0f;
                if (bossStateText != null)
                {
                    bossStateText.text = string.Empty;
                    bossStateText.color = new Color(bossStateText.color.r, bossStateText.color.g, bossStateText.color.b, 0f);
                }
                if (bossStatePanel != null)
                {
                    bossStatePanel.color = new Color(bossStatePanel.color.r, bossStatePanel.color.g, bossStatePanel.color.b, 0f);
                }
                if (bossDefenseSlider != null)
                {
                    bossDefenseSlider.gameObject.SetActive(false);
                }
                if (bossWindowText != null)
                {
                    bossWindowText.text = string.Empty;
                    bossWindowText.color = new Color(bossWindowText.color.r, bossWindowText.color.g, bossWindowText.color.b, 0f);
                }
                if (bossWindowPanel != null)
                {
                    bossWindowPanel.color = new Color(bossWindowPanel.color.r, bossWindowPanel.color.g, bossWindowPanel.color.b, 0f);
                }
                if (bossAttackModeText != null)
                {
                    bossAttackModeText.text = string.Empty;
                    bossAttackModeText.color = new Color(bossAttackModeText.color.r, bossAttackModeText.color.g, bossAttackModeText.color.b, 0f);
                }
                if (bossAttackModePanel != null)
                {
                    bossAttackModePanel.color = new Color(bossAttackModePanel.color.r, bossAttackModePanel.color.g, bossAttackModePanel.color.b, 0f);
                }
                if (bossAttackWarningText != null)
                {
                    bossAttackWarningText.text = string.Empty;
                    bossAttackWarningText.color = new Color(bossAttackWarningText.color.r, bossAttackWarningText.color.g, bossAttackWarningText.color.b, 0f);
                }
                if (bossAttackWarningPanel != null)
                {
                    bossAttackWarningPanel.color = new Color(bossAttackWarningPanel.color.r, bossAttackWarningPanel.color.g, bossAttackWarningPanel.color.b, 0f);
                }
                return;
            }

            bossHpSlider.value = ratio;
            UpdateBossFillVisual(ratio);
            UpdateBossStateText(ratio);

            if ((lastBossRatio > 0.7f && ratio <= 0.7f) || (lastBossRatio > 0.4f && ratio <= 0.4f))
            {
                if (bossAlertRoutine != null)
                {
                    StopCoroutine(bossAlertRoutine);
                }
                bossAlertRoutine = StartCoroutine(PulseBossBarAlert());
            }

            lastBossRatio = ratio;
        }

        private void HandleSkillEnergyChanged(float normalized, string label)
        {
            Sprite novaSprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.SkillNovaIcon);
            if (skillNovaIcon != null && novaSprite != null && skillNovaIcon.sprite != novaSprite)
            {
                skillNovaIcon.sprite = novaSprite;
                skillNovaIcon.preserveAspect = true;
            }

            Sprite overdriveSprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.SkillOverdriveIcon);
            if (skillOverdriveIcon != null && overdriveSprite != null && skillOverdriveIcon.sprite != overdriveSprite)
            {
                skillOverdriveIcon.sprite = overdriveSprite;
                skillOverdriveIcon.preserveAspect = true;
            }

            if (skillSlider != null)
            {
                skillSlider.value = normalized;
                if (skillSlider.fillRect != null)
                {
                    Image fill = skillSlider.fillRect.GetComponent<Image>();
                    if (fill != null)
                    {
                        fill.color = Color.Lerp(new Color(0.16f, 0.44f, 0.8f, 1f), new Color(0.42f, 0.9f, 1f, 1f), normalized);
                    }
                }
            }

            if (skillGlowOverlay != null)
            {
                if (normalized >= 0.999f)
                {
                    float alpha = 0.16f + Mathf.PingPong(Time.unscaledTime * 0.22f, 0.14f);
                    skillGlowOverlay.color = new Color(0.72f, 0.98f, 1f, alpha);
                }
                else
                {
                    float alpha = 0.04f + Mathf.PingPong(Time.unscaledTime * 0.08f, 0.04f);
                    skillGlowOverlay.color = new Color(0.42f, 0.8f, 1f, alpha);
                }
            }

            if (skillNovaIcon != null)
            {
                skillNovaIcon.color = normalized >= 0.4f ? new Color(0.62f, 0.94f, 1f, 1f) : new Color(0.28f, 0.4f, 0.54f, 0.72f);
            }

            if (skillOverdriveIcon != null)
            {
                skillOverdriveIcon.color = normalized >= 0.6f ? new Color(1f, 0.82f, 0.36f, 1f) : new Color(0.44f, 0.34f, 0.18f, 0.72f);
            }

            if (skillText != null)
            {
                skillText.text = LocalizationService.TranslateLiteral(label);
            }
        }

        private void HandleLoadoutChanged(string shipName, int weaponLevel)
        {
            if (loadoutText != null)
            {
                loadoutText.text = LocalizationService.IsChinese ? $"机型  {shipName}" : $"FRAME  {shipName}";
            }

            if (weaponLevelText != null)
            {
                weaponLevelText.text = LocalizationService.IsChinese ? $"火力等级  Lv.{weaponLevel}" : $"WEAPON LEVEL  Lv.{weaponLevel}";
            }
        }

        private void HandleBuffStatusChanged(string summary, PickupBuffType[] activeTypes)
        {
            if (buffSummaryText != null)
            {
                bool hasBuffs = activeTypes != null && activeTypes.Length > 0;
                buffSummaryText.text = hasBuffs ? summary : string.Empty;
                buffSummaryText.color = new Color(0.82f, 0.94f, 1f, hasBuffs ? 0.95f : 0f);
            }

            for (int i = 0; i < buffIcons.Count; i++)
            {
                if (i >= activeTypes.Length)
                {
                    buffIcons[i].color = new Color(0.5f, 0.74f, 0.9f, 0f);
                    continue;
                }

                buffIcons[i].sprite = GetBuffSprite(activeTypes[i]);
                buffIcons[i].color = GetBuffColor(activeTypes[i]);
            }
        }

        private void HandlePickupCollected(string label)
        {
            if (pickupText == null)
            {
                return;
            }

            pickupText.text = label;
            pickupText.color = new Color(1f, 0.94f, 0.72f, 1f);
            if (pickupPanel != null)
            {
                pickupPanel.color = new Color(0.12f, 0.08f, 0.04f, 0.7f);
            }
            StartCoroutine(FadePickupText());
        }

        private void HandleThreatEdgePulse(float duration, bool cyan)
        {
            if (edgeWarningRoutine != null)
            {
                StopCoroutine(edgeWarningRoutine);
            }

            edgeWarningRoutine = StartCoroutine(PulseEdgeWarnings(duration, cyan));
        }

        private void HandleBossLockOnWarning(float viewportX, float duration)
        {
            if (bossLockRoutine != null)
            {
                StopCoroutine(bossLockRoutine);
            }

            bossLockRoutine = StartCoroutine(AnimateBossLock(viewportX, duration));
        }

        private void HandleBossLaserHit(float intensity)
        {
            if (hudAudio != null)
            {
                hudAudio.pitch = 0.92f;
                hudAudio.PlayOneShot(laserBurnClip, Mathf.Lerp(0.18f, 0.34f, intensity) * GameSettingsService.SfxVolume);
                hudAudio.pitch = 1f;
            }

            if (laserHitRoutine != null)
            {
                StopCoroutine(laserHitRoutine);
            }

            laserHitRoutine = StartCoroutine(FlashLaserBurn(Mathf.Clamp01(intensity)));
        }

        private void HandleBossDefenseStateChanged(bool shieldActive, float normalizedValue, string stateLabel, float timerRemaining)
        {
            if (bossStateText == null)
            {
                return;
            }

            string localized = LocalizationService.TranslateLiteral(stateLabel);
            bossStateText.text = shieldActive
                ? string.Format(LocalizationService.IsChinese ? "Boss 防御: {0}  {1:0%}" : "BOSS DEFENSE: {0}  {1:0%}", localized, normalizedValue)
                : string.Format(LocalizationService.IsChinese ? "Boss 核心: {0}" : "BOSS CORE: {0}", localized);
            bossStateText.color = shieldActive ? new Color(0.72f, 0.9f, 1f, 0.96f) : new Color(1f, 0.9f, 0.66f, 0.96f);

            if (bossStatePanel != null)
            {
                bossStatePanel.color = shieldActive
                    ? new Color(0.06f, 0.1f, 0.16f, 0.72f)
                    : new Color(0.12f, 0.08f, 0.04f, 0.78f);
            }

            if (bossDefenseSlider != null)
            {
                bossDefenseSlider.gameObject.SetActive(true);
                bossDefenseSlider.value = normalizedValue;
                if (bossDefenseSlider.fillRect != null)
                {
                    Image fill = bossDefenseSlider.fillRect.GetComponent<Image>();
                    if (fill != null)
                    {
                        fill.color = shieldActive
                            ? Color.Lerp(new Color(0.24f, 0.66f, 0.9f, 1f), new Color(0.58f, 0.92f, 1f, 1f), normalizedValue)
                            : new Color(1f, 0.78f, 0.38f, 1f);
                    }
                }
                Transform panel = bossDefenseSlider.transform.parent != null ? bossDefenseSlider.transform.parent.Find("Fill_Panel") : null;
                if (panel != null)
                {
                    panel.gameObject.SetActive(true);
                }
            }

            if (bossWindowText != null)
            {
                if (shieldActive)
                {
                    bossWindowText.text = LocalizationService.IsChinese ? "核心窗口: 待触发" : "CORE WINDOW: LOCKED";
                    bossWindowText.color = new Color(0.72f, 0.9f, 1f, 0.82f);
                    if (bossWindowPanel != null)
                    {
                        bossWindowPanel.color = new Color(0.06f, 0.1f, 0.16f, 0.56f);
                    }
                }
                else
                {
                    bossWindowText.text = LocalizationService.IsChinese
                        ? $"脆弱窗口剩余: {timerRemaining:0.0}s"
                        : $"VULNERABLE WINDOW: {timerRemaining:0.0}s";
                    bossWindowText.color = new Color(1f, 0.9f, 0.66f, 0.96f);
                    if (bossWindowPanel != null)
                    {
                        bossWindowPanel.color = new Color(0.12f, 0.08f, 0.04f, 0.74f);
                    }
                }
            }
        }

        private void HandleTacticalProgressChanged(string objectiveLabel, int phaseIndex, int phaseCount, float phaseProgress, int remainingEnemies, int totalEnemies)
        {
            if (missionText != null)
            {
                missionText.text = LocalizationService.IsChinese
                    ? $"战术目标: {LocalizationService.TranslateLiteral(objectiveLabel)}"
                    : $"TACTICAL: {LocalizationService.TranslateLiteral(objectiveLabel)}";
            }

            if (objectiveProgressText != null)
            {
                string phaseLabel = LocalizationService.IsChinese
                    ? $"阶段 {Mathf.Max(1, phaseIndex)} / {Mathf.Max(1, phaseCount)}"
                    : $"PHASE {Mathf.Max(1, phaseIndex)} / {Mathf.Max(1, phaseCount)}";
                string remainingLabel = totalEnemies > 0
                    ? (LocalizationService.IsChinese
                        ? $"剩余敌机 {remainingEnemies}/{totalEnemies}"
                        : $"HOSTILES {remainingEnemies}/{totalEnemies}")
                    : (LocalizationService.IsChinese ? "Boss 阶段" : "BOSS STAGE");
                objectiveProgressText.text = string.Format("{0}   {1:0%}   {2}", phaseLabel, Mathf.Clamp01(phaseProgress), remainingLabel);
            }

            if (chapterProgressSlider != null)
            {
                float normalizedChapterProgress = Mathf.Clamp01(((Mathf.Max(1, phaseIndex) - 1f) + Mathf.Clamp01(phaseProgress)) / Mathf.Max(1f, phaseCount));
                chapterProgressSlider.value = normalizedChapterProgress;
                if (chapterProgressSlider.fillRect != null)
                {
                    Image fill = chapterProgressSlider.fillRect.GetComponent<Image>();
                    if (fill != null)
                    {
                        fill.color = Color.Lerp(new Color(0.34f, 0.72f, 0.94f, 0.96f), new Color(1f, 0.78f, 0.42f, 0.96f), normalizedChapterProgress);
                    }
                }
            }
        }

        private void HandleBossAttackTelemetryChanged(string currentModeLabel, string nextModeLabel, float etaSeconds)
        {
            if (bossAttackModeText != null)
            {
                bossAttackModeText.text = LocalizationService.IsChinese
                    ? $"当前模式: {LocalizationService.TranslateLiteral(currentModeLabel)}"
                    : $"CURRENT PATTERN: {LocalizationService.TranslateLiteral(currentModeLabel).ToUpperInvariant()}";
                bossAttackModeText.color = new Color(0.76f, 0.92f, 1f, 0.94f);
            }

            if (bossAttackModePanel != null)
            {
                bossAttackModePanel.color = new Color(0.06f, 0.1f, 0.16f, 0.68f);
            }

            if (bossAttackWarningText != null)
            {
                bossAttackWarningText.text = LocalizationService.IsChinese
                    ? $"下一次大招: {LocalizationService.TranslateLiteral(nextModeLabel)}  {etaSeconds:0.0}s"
                    : $"NEXT MAJOR: {LocalizationService.TranslateLiteral(nextModeLabel).ToUpperInvariant()}  {etaSeconds:0.0}s";
                bossAttackWarningText.color = etaSeconds <= 1.2f ? new Color(1f, 0.8f, 0.52f, 0.98f) : new Color(1f, 0.88f, 0.62f, 0.88f);
            }

            if (bossAttackWarningPanel != null)
            {
                bossAttackWarningPanel.color = etaSeconds <= 1.2f
                    ? new Color(0.18f, 0.08f, 0.04f, 0.82f)
                    : new Color(0.12f, 0.08f, 0.04f, 0.68f);
            }
        }

        private void HandleBossPhaseTelemetryChanged(string phaseLabel, string moduleLabel, bool transitionLocked)
        {
            if (bossWindowText != null)
            {
                bossWindowText.text = LocalizationService.IsChinese
                    ? $"{LocalizationService.TranslateLiteral(phaseLabel)}  |  {LocalizationService.TranslateLiteral(moduleLabel)}"
                    : $"{LocalizationService.TranslateLiteral(phaseLabel).ToUpperInvariant()}  |  {LocalizationService.TranslateLiteral(moduleLabel).ToUpperInvariant()}";
                bossWindowText.color = transitionLocked
                    ? new Color(1f, 0.8f, 0.48f, 0.98f)
                    : new Color(0.84f, 0.96f, 1f, 0.94f);
            }

            if (bossWindowPanel != null)
            {
                bossWindowPanel.color = transitionLocked
                    ? new Color(0.18f, 0.08f, 0.04f, 0.84f)
                    : new Color(0.04f, 0.08f, 0.16f, 0.68f);
            }
        }

        private void UpdateBossFillVisual(float ratio)
        {
            if (bossHpSlider.fillRect == null)
            {
                return;
            }

            Image fill = bossHpSlider.fillRect.GetComponent<Image>();
            if (fill == null)
            {
                return;
            }

            if (ratio <= 0.4f)
            {
                fill.color = new Color(1f, 0.16f, 0.16f, 1f);
            }
            else if (ratio <= 0.7f)
            {
                fill.color = new Color(1f, 0.56f, 0.2f, 1f);
            }
            else
            {
                fill.color = new Color(1f, 0.34f, 0.18f, 1f);
            }

            for (int i = 0; i < bossSegmentMarkers.Count; i++)
            {
                float threshold = i == 0 ? 0.7f : 0.4f;
                bossSegmentMarkers[i].color = ratio <= threshold ? new Color(1f, 0.28f, 0.18f, 0.9f) : new Color(1f, 0.92f, 0.68f, 0.42f);
            }
        }

        private void HandleCombatAnnouncement(string message)
        {
            if (announcementText == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            string localizedMessage = LocalizationService.TranslateLiteral(message);
            announcementText.text = localizedMessage;
            if (missionText != null)
            {
                bool highPriority = localizedMessage.Contains("WARNING") || localizedMessage.Contains("警告") || localizedMessage.Contains("BOSS") || localizedMessage.Contains("Boss");
                if (!highPriority)
                {
                    missionText.text = LocalizationService.IsChinese ? $"战术目标: {localizedMessage}" : $"TACTICAL: {localizedMessage}";
                }
            }
            if (announcementPanel != null)
            {
                announcementPanel.color = localizedMessage.Contains("WARNING") || localizedMessage.Contains("警告") ? new Color(0.26f, 0.1f, 0.02f, 0.84f) : new Color(0.18f, 0.1f, 0.03f, 0.78f);
            }

            if (announcementRoutine != null)
            {
                StopCoroutine(announcementRoutine);
            }

            announcementRoutine = StartCoroutine(ShowAnnouncement());
            if (localizedMessage.Contains("WARNING") || localizedMessage.Contains("警告") || localizedMessage.Contains("BOSS") || localizedMessage.Contains("Boss"))
            {
                if (warningRoutine != null)
                {
                    StopCoroutine(warningRoutine);
                }
                warningRoutine = StartCoroutine(FlashWarningOverlay(localizedMessage.Contains("WARNING") || localizedMessage.Contains("警告") ? 0.34f : 0.2f));
            }
        }

        private void HandleLanguageChanged(GameLanguage _)
        {
            HandleScoreChanged(displayedScore);
            HandlePlayerHpChanged(displayedHp, displayedMaxHp);
            if (comboText != null && !string.IsNullOrEmpty(comboText.text))
            {
                comboText.text = LocalizationService.TranslateLiteral(comboText.text);
            }
            if (skillText != null)
            {
                skillText.text = LocalizationService.TranslateLiteral(skillText.text);
            }
            if (pickupText != null && !string.IsNullOrEmpty(pickupText.text))
            {
                pickupText.text = LocalizationService.TranslateLiteral(pickupText.text);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(state == GameState.Paused);
            }
        }

        private IEnumerator AnimateNumericText(Text text, int from, int to, string prefix, Color pulseColor, float duration)
        {
            float elapsed = 0f;
            Vector3 originalScale = text.transform.localScale;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                int value = Mathf.RoundToInt(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t)));
                text.text = prefix + value;
                text.transform.localScale = originalScale * Mathf.Lerp(1.16f, 1f, t);
                text.color = Color.Lerp(pulseColor, new Color(0.45f, 0.95f, 1f, 1f), t);
                yield return null;
            }

            text.text = prefix + to;
            text.transform.localScale = originalScale;
            text.color = new Color(0.45f, 0.95f, 1f, 1f);
        }

        private IEnumerator FadePickupText()
        {
            float duration = 0.85f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (pickupText != null)
                {
                    pickupText.color = new Color(1f, 0.94f, 0.72f, Mathf.Lerp(1f, 0f, t));
                }
                if (pickupPanel != null)
                {
                    pickupPanel.color = new Color(0.12f, 0.08f, 0.04f, Mathf.Lerp(0.7f, 0f, t));
                }
                yield return null;
            }
        }

        private void UpdateBossStateText(float ratio)
        {
            if (bossStateText == null)
            {
                return;
            }

            string label;
            if (ratio <= 0.4f)
            {
                label = LocalizationService.IsChinese ? "Boss 状态: 终局压制" : "BOSS STATUS: FINAL ASSAULT";
            }
            else if (ratio <= 0.7f)
            {
                label = LocalizationService.IsChinese ? "Boss 状态: 二阶段突破" : "BOSS STATUS: PHASE TWO";
            }
            else
            {
                label = LocalizationService.IsChinese ? "Boss 状态: 交战中" : "BOSS STATUS: ENGAGED";
            }

            bossStateText.text = label;
            bossStateText.color = new Color(1f, 0.88f, 0.62f, 0.95f);
            if (bossStatePanel != null)
            {
                bossStatePanel.color = new Color(0.12f, 0.08f, 0.04f, 0.72f);
            }
        }

        private static Sprite GetBuffSprite(PickupBuffType type)
        {
            return type switch
            {
                PickupBuffType.FireRate => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash),
                PickupBuffType.Damage => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Nose),
                PickupBuffType.ProjectileSpeed => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Bullet),
                PickupBuffType.Magnet => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Wing),
                PickupBuffType.Guard => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Engine),
                _ => GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Cockpit)
            };
        }

        private static Color GetBuffColor(PickupBuffType type)
        {
            return type switch
            {
                PickupBuffType.FireRate => new Color(0.74f, 0.94f, 1f, 0.96f),
                PickupBuffType.Damage => new Color(1f, 0.72f, 0.4f, 0.96f),
                PickupBuffType.ProjectileSpeed => new Color(0.68f, 0.9f, 1f, 0.96f),
                PickupBuffType.Magnet => new Color(0.86f, 0.9f, 1f, 0.96f),
                PickupBuffType.Guard => new Color(0.48f, 0.92f, 1f, 0.96f),
                _ => new Color(0.7f, 0.86f, 1f, 0.96f)
            };
        }

        private IEnumerator PulseText(Text text, Color color, float scaleMultiplier, float duration)
        {
            Color originalColor = text.color;
            Vector3 originalScale = text.transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                text.transform.localScale = originalScale * Mathf.Lerp(scaleMultiplier, 1f, eased);
                text.color = Color.Lerp(color, originalColor, eased);
                yield return null;
            }

            text.transform.localScale = originalScale;
            text.color = originalColor;
        }

        private IEnumerator ShowAnnouncement()
        {
            announcementText.enabled = true;
            announcementText.color = new Color(1f, 0.92f, 0.65f, 1f);
            yield return new WaitForSeconds(1.5f);
            announcementText.text = string.Empty;
            if (announcementPanel != null)
            {
                announcementPanel.color = new Color(announcementPanel.color.r, announcementPanel.color.g, announcementPanel.color.b, 0f);
            }
        }

        private IEnumerator FlashDamageOverlay(float peakAlpha)
        {
            float duration = 0.18f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                damageOverlay.color = new Color(1f, 0.08f, 0.08f, Mathf.Lerp(peakAlpha, 0f, t));
                if (hpText != null)
                {
                    hpText.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, t);
                }
                yield return null;
            }

            damageOverlay.color = new Color(1f, 0.08f, 0.08f, 0f);
            if (hpText != null)
            {
                hpText.transform.localScale = Vector3.one;
            }
        }

        private IEnumerator FlashWarningOverlay(float peakAlpha)
        {
            if (warningOverlay == null)
            {
                yield break;
            }

            for (int i = 0; i < 2; i++)
            {
                warningOverlay.color = new Color(1f, 0.36f, 0.08f, peakAlpha);
                yield return new WaitForSeconds(0.09f);
                warningOverlay.color = new Color(1f, 0.36f, 0.08f, 0f);
                yield return new WaitForSeconds(0.08f);
            }
        }

        private IEnumerator PulseEdgeWarnings(float duration, bool cyan)
        {
            Image[] edges = { edgeWarningTop, edgeWarningBottom, edgeWarningLeft, edgeWarningRight };
            Color peak = cyan ? new Color(0.52f, 0.92f, 1f, 0.6f) : new Color(1f, 0.44f, 0.12f, 0.6f);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float pulse = 0.22f + Mathf.PingPong(elapsed * 8f, 0.38f);
                for (int i = 0; i < edges.Length; i++)
                {
                    if (edges[i] != null)
                    {
                        edges[i].color = new Color(peak.r, peak.g, peak.b, pulse);
                    }
                }

                yield return null;
            }

            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i] != null)
                {
                    edges[i].color = new Color(peak.r, peak.g, peak.b, 0f);
                }
            }
        }

        private IEnumerator PulseBossBarAlert()
        {
            if (bossPanel == null)
            {
                yield break;
            }

            Color baseColor = bossPanel.color;
            for (int i = 0; i < 2; i++)
            {
                bossPanel.color = new Color(0.34f, 0.08f, 0.04f, 0.92f);
                yield return new WaitForSeconds(0.1f);
                bossPanel.color = baseColor;
                yield return new WaitForSeconds(0.08f);
            }
        }

        private IEnumerator FlashLaserBurn(float intensity)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Vector2 scanlineOrigin = scanlineOverlay != null ? scanlineOverlay.rectTransform.anchoredPosition : Vector2.zero;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float fade = 1f - t;
                float burnAlpha = Mathf.Lerp(0.58f, 0f, t) * intensity;
                if (laserBurnOverlay != null)
                {
                    laserBurnOverlay.color = new Color(1f, 0.32f + 0.5f * fade, 0.16f, burnAlpha);
                }

                if (warningOverlay != null)
                {
                    float hot = Mathf.Lerp(0.42f, 0f, t) * intensity;
                    warningOverlay.color = new Color(1f, 0.22f + 0.42f * fade, 0.12f, hot);
                }

                if (scanlineOverlay != null)
                {
                    float jitter = Mathf.Lerp(12f, 0f, Mathf.Clamp01(t * 1.7f)) * intensity;
                    scanlineOverlay.rectTransform.anchoredPosition = scanlineOrigin + new Vector2(Random.Range(-jitter, jitter), Random.Range(-jitter * 0.6f, jitter * 0.6f));
                    scanlineOverlay.color = new Color(0.82f, 0.94f, 1f, 0.055f + fade * 0.09f * intensity);
                }

                yield return null;
            }

            if (laserBurnOverlay != null)
            {
                laserBurnOverlay.color = new Color(1f, 0.82f, 0.46f, 0f);
            }

            if (warningOverlay != null)
            {
                warningOverlay.color = new Color(1f, 0.36f, 0.08f, 0f);
            }

            if (scanlineOverlay != null)
            {
                scanlineOverlay.rectTransform.anchoredPosition = scanlineOrigin;
                scanlineOverlay.color = new Color(0.7f, 0.92f, 1f, 0.055f);
            }
        }

        private void EnsureHudAudio()
        {
            if (hudAudio == null)
            {
                hudAudio = GetComponent<AudioSource>();
                if (hudAudio == null)
                {
                    hudAudio = gameObject.AddComponent<AudioSource>();
                }

                hudAudio.playOnAwake = false;
                hudAudio.spatialBlend = 0f;
            }

            if (laserBurnClip == null)
            {
                laserBurnClip = BuildHudTone("boss-laser-burn", 190f, 0.48f, 0.14f);
            }
        }

        private static AudioClip BuildHudTone(string clipName, float frequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float env = Mathf.Sin(t * Mathf.PI) * volume;
                float hum = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate);
                float crackle = Mathf.Sin(2f * Mathf.PI * frequency * 2.6f * i / sampleRate) * 0.22f;
                samples[i] = (hum + crackle) * env;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private IEnumerator AnimateBossLock(float viewportX, float duration)
        {
            if (bossLockFrame == null || lockCrosshairParts.Count < 4)
            {
                yield break;
            }

            RectTransform frameRect = bossLockFrame.rectTransform;
            float clampedX = Mathf.Lerp(-420f, 420f, Mathf.Clamp01(viewportX));
            frameRect.anchoredPosition = new Vector2(clampedX, -36f);
            Vector2[] startOffsets =
            {
                new Vector2(0f, 120f),
                new Vector2(0f, -120f),
                new Vector2(-120f, 0f),
                new Vector2(120f, 0f)
            };
            Vector2[] endOffsets =
            {
                new Vector2(0f, 42f),
                new Vector2(0f, -42f),
                new Vector2(-42f, 0f),
                new Vector2(42f, 0f)
            };

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pulse = 0.22f + Mathf.PingPong(elapsed * 10f, 0.34f);
                bossLockFrame.color = new Color(0.58f, 0.96f, 1f, pulse);
                frameRect.sizeDelta = Vector2.Lerp(new Vector2(112f, 176f), new Vector2(86f, 138f), t);
                for (int i = 0; i < lockCrosshairParts.Count; i++)
                {
                    Image part = lockCrosshairParts[i];
                    if (part == null)
                    {
                        continue;
                    }

                    RectTransform partRect = part.rectTransform;
                    partRect.anchoredPosition = new Vector2(clampedX, 84f) + Vector2.Lerp(startOffsets[i], endOffsets[i], t);
                    part.color = new Color(0.7f, 0.98f, 1f, pulse);
                }

                yield return null;
            }

            bossLockFrame.color = new Color(0.58f, 0.96f, 1f, 0f);
            for (int i = 0; i < lockCrosshairParts.Count; i++)
            {
                if (lockCrosshairParts[i] != null)
                {
                    lockCrosshairParts[i].color = new Color(0.7f, 0.98f, 1f, 0f);
                }
            }
        }

        private void AddPanelCorners(Transform panelTransform, Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                string name = "Corner_" + i;
                if (panelTransform.Find(name) != null)
                {
                    continue;
                }

                GameObject corner = new GameObject(name);
                corner.transform.SetParent(panelTransform, false);
                RectTransform rect = corner.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(i == 0 || i == 2 ? 0f : 1f, i < 2 ? 1f : 0f);
                rect.anchorMax = rect.anchorMin;
                rect.pivot = rect.anchorMin;
                rect.sizeDelta = new Vector2(18f, 18f);
                rect.anchoredPosition = Vector2.zero;
                float rotation = i == 0 ? 0f : i == 1 ? 90f : i == 2 ? -90f : 180f;
                rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
                Image img = corner.AddComponent<Image>();
                img.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Wing);
                img.color = new Color(Mathf.Clamp01(color.r + 0.1f), Mathf.Clamp01(color.g + 0.12f), Mathf.Clamp01(color.b + 0.12f), Mathf.Max(0.24f, color.a));
            }
        }

        private Sprite BuildScanlineSprite()
        {
            Texture2D texture = new Texture2D(4, 16, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            Color[] pixels = new Color[64];
            for (int y = 0; y < 16; y++)
            {
                Color line = y % 4 == 0 ? new Color(1f, 1f, 1f, 0.22f) : new Color(1f, 1f, 1f, 0f);
                for (int x = 0; x < 4; x++)
                {
                    pixels[(y * 4) + x] = line;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, 4f, 16f), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}

