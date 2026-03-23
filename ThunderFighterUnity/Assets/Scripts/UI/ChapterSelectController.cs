using System.Collections.Generic;
using ThunderFighter.Config;
using ThunderFighter.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThunderFighter.UI
{
    public class ChapterSelectController : MonoBehaviour
    {
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private Text headerText;
        [SerializeField] private Text techPointsText;
        [SerializeField] private Text powerText;
        [SerializeField] private Button backButton;
        [SerializeField] private Button firepowerUpgradeButton;
        [SerializeField] private Button armorUpgradeButton;
        [SerializeField] private Button reactorUpgradeButton;
        [SerializeField] private Text firepowerUpgradeText;
        [SerializeField] private Text armorUpgradeText;
        [SerializeField] private Text reactorUpgradeText;

        private readonly List<GameObject> generatedCards = new List<GameObject>();
        private Text opsText;
        private Text shipSelectText;
        private Text shipDetailText;
        private Button shipPrevButton;
        private Button shipNextButton;

        private void Awake()
        {
            backButton?.onClick.AddListener(BackToMenu);
            firepowerUpgradeButton?.onClick.AddListener(() => TryUpgrade(UpgradeType.Firepower));
            armorUpgradeButton?.onClick.AddListener(() => TryUpgrade(UpgradeType.Armor));
            reactorUpgradeButton?.onClick.AddListener(() => TryUpgrade(UpgradeType.Reactor));
        }

        private void OnEnable()
        {
            LocalizationService.OnLanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            LocalizationService.OnLanguageChanged -= HandleLanguageChanged;
        }

        private void OnDestroy()
        {
            backButton?.onClick.RemoveListener(BackToMenu);
            shipPrevButton?.onClick.RemoveListener(SelectPreviousShip);
            shipNextButton?.onClick.RemoveListener(SelectNextShip);
        }

        private void Start()
        {
            EnsureRuntimeUi();
            RefreshAll();
        }

        private void HandleLanguageChanged(GameLanguage _)
        {
            RefreshAll();
        }

        private void EnsureRuntimeUi()
        {
            Canvas canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            TerminalUiFactory.EnsureCanvasRuntimeSettings(canvas);

            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            bool compact = aspect < 1.72f;
            bool ultraCompact = aspect < 1.5f;

            TerminalUiFactory.CreateStretchPanel(canvas.transform, "DeploymentBackdrop", new Color(0.01f, 0.04f, 0.08f, 0.2f));
            TerminalUiFactory.CreateGlow(canvas.transform, "DeployGlow_01", new Vector2(0.12f, 0.82f), new Vector2(compact ? 320f : 440f, compact ? 320f : 440f), new Color(0.2f, 0.66f, 1f, 0.1f));
            TerminalUiFactory.CreateGlow(canvas.transform, "DeployGlow_02", new Vector2(0.9f, 0.2f), new Vector2(compact ? 360f : 520f, compact ? 360f : 520f), new Color(1f, 0.58f, 0.2f, 0.08f));

            Vector2 leftPanelAnchor = ultraCompact ? new Vector2(0.17f, 0.5f) : (compact ? new Vector2(0.17f, 0.5f) : new Vector2(0.18f, 0.5f));
            Vector2 leftPanelSize = ultraCompact ? new Vector2(250f, 700f) : (compact ? new Vector2(320f, 720f) : new Vector2(350f, 760f));
            Vector2 centerPanelAnchor = ultraCompact ? new Vector2(0.66f, 0.5f) : (compact ? new Vector2(0.62f, 0.5f) : new Vector2(0.61f, 0.5f));
            Vector2 centerPanelSize = ultraCompact ? new Vector2(700f, 700f) : (compact ? new Vector2(900f, 720f) : new Vector2(1120f, 760f));

            Image leftPanel = TerminalUiFactory.CreatePanel(canvas.transform, "UpgradePanel", leftPanelAnchor, leftPanelSize, new Color(0.03f, 0.08f, 0.14f, 0.84f));
            Image centerPanel = TerminalUiFactory.CreatePanel(canvas.transform, "ChapterPanel", centerPanelAnchor, centerPanelSize, new Color(0.04f, 0.08f, 0.16f, 0.78f));
            TerminalUiFactory.AddHorizontalDivider(leftPanel.transform, "DividerTop", new Vector2(0.5f, 0.83f), new Vector2(ultraCompact ? 180f : (compact ? 230f : 270f), 4f), new Color(0.46f, 0.86f, 1f, 0.48f));
            TerminalUiFactory.AddHorizontalDivider(centerPanel.transform, "DividerTop", new Vector2(0.5f, 0.88f), new Vector2(ultraCompact ? 560f : (compact ? 720f : 940f), 4f), new Color(0.46f, 0.86f, 1f, 0.48f));

            headerText = EnsureText(headerText, centerPanel.transform, "Header", ultraCompact ? 28 : (compact ? 34 : 40), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.92f), new Vector2(ultraCompact ? 520f : (compact ? 620f : 760f), 54f), Color.white);
            techPointsText = EnsureText(techPointsText, leftPanel.transform, "TechPoints", ultraCompact ? 18 : 22, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.77f), new Vector2(ultraCompact ? 180f : (compact ? 240f : 280f), 36f), new Color(0.78f, 0.94f, 1f, 0.98f));
            powerText = EnsureText(powerText, leftPanel.transform, "PowerRating", ultraCompact ? 18 : 22, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.72f), new Vector2(ultraCompact ? 180f : (compact ? 240f : 280f), 36f), new Color(1f, 0.86f, 0.56f, 0.98f));
            if (leftPanel.transform.Find("ShipPortrait") == null)
            {
                TerminalUiFactory.CreateSprite(
                    leftPanel.transform,
                    "ShipPortrait",
                    RuntimeArtLibrary.Get(RuntimeArtLibrary.GetPlayerShipSpriteId(CampaignProgressService.GetSelectedShipId()))
                        ?? GeneratedSpriteLibrary.GetShipPresentationSprite(CampaignProgressService.GetSelectedShipId(), true),
                    new Vector2(0.5f, 0.89f),
                    ultraCompact ? new Vector2(110f, 74f) : new Vector2(142f, 96f),
                    Color.white);
            }
            shipSelectText = EnsureText(shipSelectText, leftPanel.transform, "ShipSelectText", ultraCompact ? 16 : 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.65f), new Vector2(ultraCompact ? 180f : 260f, 30f), Color.white);
            shipDetailText = EnsureText(shipDetailText, leftPanel.transform, "ShipDetailText", ultraCompact ? 13 : 14, TextAnchor.UpperCenter, new Vector2(0.5f, 0.60f), new Vector2(ultraCompact ? 180f : 250f, 60f), new Color(0.82f, 0.94f, 1f, 0.96f));
            shipPrevButton = EnsureLabeledButton(shipPrevButton, leftPanel.transform, "ShipPrevButton", "<", new Vector2(0.18f, 0.65f), new Vector2(34f, 30f), new Color(0.16f, 0.24f, 0.36f, 0.92f), SelectPreviousShip);
            shipNextButton = EnsureLabeledButton(shipNextButton, leftPanel.transform, "ShipNextButton", ">", new Vector2(0.82f, 0.65f), new Vector2(34f, 30f), new Color(0.12f, 0.42f, 0.78f, 0.92f), SelectNextShip);
            opsText = EnsureText(opsText, leftPanel.transform, "OpsText", ultraCompact ? 15 : (compact ? 17 : 18), TextAnchor.UpperLeft, new Vector2(0.5f, 0.54f), new Vector2(ultraCompact ? 180f : (compact ? 240f : 280f), ultraCompact ? 160f : 180f), new Color(0.82f, 0.94f, 1f, 0.96f));

            if (cardsRoot == null)
            {
                Image cardHost = TerminalUiFactory.CreatePanel(centerPanel.transform, "CardsRootPanel", new Vector2(0.5f, 0.46f), ultraCompact ? new Vector2(640f, 560f) : (compact ? new Vector2(820f, 560f) : new Vector2(1000f, 560f)), new Color(0.03f, 0.06f, 0.12f, 0.72f));
                cardsRoot = cardHost.rectTransform;
            }
            else
            {
                cardsRoot.SetParent(centerPanel.transform, false);
                RectTransform cardsRect = cardsRoot as RectTransform;
                if (cardsRect != null)
                {
                    cardsRect.anchorMin = new Vector2(0.5f, 0.46f);
                    cardsRect.anchorMax = new Vector2(0.5f, 0.46f);
                    cardsRect.pivot = new Vector2(0.5f, 0.5f);
                    cardsRect.sizeDelta = ultraCompact ? new Vector2(640f, 560f) : (compact ? new Vector2(820f, 560f) : new Vector2(1000f, 560f));
                }
            }

            firepowerUpgradeButton = EnsureButton(firepowerUpgradeButton, leftPanel.transform, "FirepowerButton", new Vector2(0.5f, 0.26f), new Vector2(ultraCompact ? 180f : (compact ? 240f : 270f), 50f), new Color(0.1f, 0.48f, 0.88f, 0.94f), () => TryUpgrade(UpgradeType.Firepower));
            armorUpgradeButton = EnsureButton(armorUpgradeButton, leftPanel.transform, "ArmorButton", new Vector2(0.5f, 0.18f), new Vector2(ultraCompact ? 180f : (compact ? 240f : 270f), 50f), new Color(0.12f, 0.4f, 0.82f, 0.94f), () => TryUpgrade(UpgradeType.Armor));
            reactorUpgradeButton = EnsureButton(reactorUpgradeButton, leftPanel.transform, "ReactorButton", new Vector2(0.5f, 0.10f), new Vector2(ultraCompact ? 180f : (compact ? 240f : 270f), 50f), new Color(0.14f, 0.34f, 0.74f, 0.94f), () => TryUpgrade(UpgradeType.Reactor));
            firepowerUpgradeText = firepowerUpgradeButton.GetComponentInChildren<Text>();
            armorUpgradeText = armorUpgradeButton.GetComponentInChildren<Text>();
            reactorUpgradeText = reactorUpgradeButton.GetComponentInChildren<Text>();
            backButton = EnsureButton(backButton, leftPanel.transform, "BackButton", new Vector2(0.5f, 0.04f), new Vector2(ultraCompact ? 180f : (compact ? 220f : 250f), 50f), new Color(0.18f, 0.22f, 0.32f, 0.92f), BackToMenu);
        }

        private void RefreshAll()
        {
            headerText.text = LocalizationService.Text("COMBAT DEPLOYMENT GRID", "作战部署网格");
            techPointsText.text = LocalizationService.IsChinese ? $"技术点  {CampaignProgressService.TechPoints}" : $"TECH POINTS  {CampaignProgressService.TechPoints}";
            powerText.text = LocalizationService.IsChinese ? $"战力评级  {CampaignProgressService.GetPowerRating()}" : $"POWER RATING  {CampaignProgressService.GetPowerRating()}";
            opsText.text = LocalizationService.IsChinese
                ? "升级舱\n\n投入技术点，强化火力、机体耐久和反应堆充能效率。\n\n部署提示\n" +
                  $"- 当前已解锁章节：{CampaignProgressService.HighestUnlockedChapter}/3\n" +
                  $"- 已通关章节数：{(CampaignProgressService.IsCompleted(1) ? 1 : 0) + (CampaignProgressService.IsCompleted(2) ? 1 : 0) + (CampaignProgressService.IsCompleted(3) ? 1 : 0)}\n" +
                  "- 高阶战区会出现更高密度精英敌人与复杂 Boss 机制"
                : "UPGRADE BAY\n\nInvest tech points into firepower, hull endurance, and reactor recharge efficiency.\n\nDEPLOYMENT NOTES\n" +
                  $"- Highest unlocked chapter: {CampaignProgressService.HighestUnlockedChapter}/3\n" +
                  $"- Chapters cleared: {(CampaignProgressService.IsCompleted(1) ? 1 : 0) + (CampaignProgressService.IsCompleted(2) ? 1 : 0) + (CampaignProgressService.IsCompleted(3) ? 1 : 0)}\n" +
                  "- Advanced sectors feature heavier elite density and more complex bosses";

            RefreshUpgradeText(firepowerUpgradeText, UpgradeType.Firepower, LocalizationService.Text("FIREPOWER", "火力"));
            RefreshUpgradeText(armorUpgradeText, UpgradeType.Armor, LocalizationService.Text("ARMOR", "装甲"));
            RefreshUpgradeText(reactorUpgradeText, UpgradeType.Reactor, LocalizationService.Text("REACTOR", "反应堆"));
            RefreshShipSelection();
            SetButtonLabel(backButton, LocalizationService.Text("RETURN TO HANGAR", "返回机库"));
            BuildCards();
        }

        private void RefreshShipSelection()
        {
            ShipDefinition ship = ShipCatalog.GetSelected();
            if (ship == null)
            {
                return;
            }

            if (shipSelectText != null)
            {
                shipSelectText.text = LocalizationService.IsChinese ? $"机型  {ship.GetDisplayName(true)}" : $"FRAME  {ship.GetDisplayName(false).ToUpperInvariant()}";
                shipSelectText.color = ship.AccentColor;
            }

            if (shipDetailText != null)
            {
                shipDetailText.text = LocalizationService.IsChinese
                    ? $"{ship.GetSubtitle(true)}\n主技能：{ship.GetSkillOneName(true)}\n强化技：{ship.GetSkillTwoName(true)}"
                    : $"{ship.GetSubtitle(false)}\nSkill 1: {ship.GetSkillOneName(false)}\nSkill 2: {ship.GetSkillTwoName(false)}";
            }

            Transform portrait = shipSelectText != null ? shipSelectText.transform.parent.Find("ShipPortrait") : null;
            if (portrait != null)
            {
                Image image = portrait.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = RuntimeArtLibrary.Get(RuntimeArtLibrary.GetPlayerShipSpriteId(ship.ShipId))
                        ?? GeneratedSpriteLibrary.GetShipPresentationSprite(ship.ShipId, true)
                        ?? image.sprite;
                    image.color = Color.white;
                }
            }
        }

        private void RefreshUpgradeText(Text text, UpgradeType type, string title)
        {
            if (text == null)
            {
                return;
            }

            int level = CampaignProgressService.GetUpgradeLevel(type);
            string suffix = level >= 5
                ? LocalizationService.Text("MAX", "已满级")
                : (LocalizationService.IsChinese ? $"Lv.{level}  需求 {CampaignProgressService.GetUpgradeCost(type)}" : $"Lv.{level}  COST {CampaignProgressService.GetUpgradeCost(type)}");
            text.text = $"{title}  {suffix}";
        }

        private void BuildCards()
        {
            for (int i = 0; i < generatedCards.Count; i++)
            {
                if (generatedCards[i] != null)
                {
                    Destroy(generatedCards[i]);
                }
            }
            generatedCards.Clear();

            IReadOnlyList<LevelDefinition> levels = CampaignCatalog.GetLevels();
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            bool ultraCompact = aspect < 1.5f;
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDefinition level = levels[i];
                if (level == null)
                {
                    continue;
                }

                bool unlocked = CampaignProgressService.IsChapterUnlocked(level.ChapterIndex);
                Color accent = level.AccentColor;
                Vector2 cardAnchor = ResolveCardAnchor(i, levels.Count, ultraCompact);
                Vector2 cardSize = ultraCompact ? new Vector2(540f, 150f) : new Vector2(280f, 470f);
                Image card = TerminalUiFactory.CreatePanel(cardsRoot, $"ChapterCard_{level.ChapterIndex}", cardAnchor, cardSize, unlocked ? new Color(accent.r * 0.22f, accent.g * 0.16f + 0.04f, accent.b * 0.16f + 0.06f, 0.92f) : new Color(0.06f, 0.08f, 0.11f, 0.88f));
                Transform root = card.transform;
                generatedCards.Add(card.gameObject);

                Vector2 previewAnchor = ultraCompact ? new Vector2(0.17f, 0.5f) : new Vector2(0.5f, 0.82f);
                Vector2 previewSize = ultraCompact ? new Vector2(128f, 82f) : new Vector2(150f, 92f);
                TerminalUiFactory.CreateGlow(root, "PreviewGlow", previewAnchor, ultraCompact ? new Vector2(142f, 90f) : new Vector2(160f, 100f), new Color(accent.r, accent.g, accent.b, unlocked ? 0.18f : 0.06f));
                Sprite previewSprite = ResolvePreviewSprite(level);
                if (previewSprite != null)
                {
                    TerminalUiFactory.CreateSprite(root, "Preview", previewSprite, previewAnchor, previewSize, unlocked ? Color.white : new Color(0.36f, 0.42f, 0.48f, 0.72f));
                }

                Vector2 titleAnchor = ultraCompact ? new Vector2(0.52f, 0.74f) : new Vector2(0.5f, 0.67f);
                Vector2 subtitleAnchor = ultraCompact ? new Vector2(0.52f, 0.61f) : new Vector2(0.5f, 0.6f);
                Vector2 difficultyAnchor = ultraCompact ? new Vector2(0.48f, 0.42f) : new Vector2(0.5f, 0.53f);
                Vector2 powerAnchor = ultraCompact ? new Vector2(0.78f, 0.42f) : new Vector2(0.5f, 0.47f);
                Vector2 objectiveAnchor = ultraCompact ? new Vector2(0.55f, 0.20f) : new Vector2(0.5f, 0.3f);
                Vector2 scoreAnchor = ultraCompact ? new Vector2(0.42f, 0.08f) : new Vector2(0.5f, 0.15f);
                Vector2 stateAnchor = ultraCompact ? new Vector2(0.72f, 0.08f) : new Vector2(0.5f, 0.10f);
                Vector2 buttonAnchor = ultraCompact ? new Vector2(0.88f, 0.5f) : new Vector2(0.5f, 0.045f);

                TerminalUiFactory.CreateText(root, "Title", LocalizationService.TranslateLiteral(level.ChapterTitle), ultraCompact ? 22 : 24, ultraCompact ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter, titleAnchor, ultraCompact ? new Vector2(270f, 32f) : new Vector2(230f, 36f), Color.white);
                TerminalUiFactory.CreateText(root, "Subtitle", LocalizationService.TranslateLiteral(level.Subtitle), ultraCompact ? 15 : 17, ultraCompact ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter, subtitleAnchor, ultraCompact ? new Vector2(280f, 28f) : new Vector2(240f, 34f), new Color(0.82f, 0.94f, 1f, 0.96f));
                TerminalUiFactory.CreateText(root, "Difficulty", LocalizationService.IsChinese ? $"威胁  {LocalizationService.TranslateLiteral(level.DifficultyLabel)}" : $"THREAT  {level.DifficultyLabel}", ultraCompact ? 15 : 16, TextAnchor.MiddleCenter, difficultyAnchor, ultraCompact ? new Vector2(150f, 24f) : new Vector2(220f, 26f), new Color(accent.r, accent.g, accent.b, 1f));
                TerminalUiFactory.CreateText(root, "Power", LocalizationService.IsChinese ? $"推荐战力  {level.RecommendedPower}" : $"RECOMMENDED  {level.RecommendedPower}", ultraCompact ? 15 : 16, TextAnchor.MiddleCenter, powerAnchor, ultraCompact ? new Vector2(166f, 24f) : new Vector2(220f, 26f), new Color(1f, 0.86f, 0.58f, 0.94f));

                Text objective = TerminalUiFactory.CreateText(root, "Objective", LocalizationService.TranslateLiteral(level.ObjectiveText), ultraCompact ? 14 : 15, ultraCompact ? TextAnchor.UpperLeft : TextAnchor.UpperLeft, objectiveAnchor, ultraCompact ? new Vector2(310f, 56f) : new Vector2(230f, 118f), new Color(0.82f, 0.94f, 1f, 0.94f));
                objective.horizontalOverflow = HorizontalWrapMode.Wrap;
                objective.verticalOverflow = VerticalWrapMode.Overflow;
                TerminalUiFactory.AddHorizontalDivider(root, "DividerMid", ultraCompact ? new Vector2(0.56f, 0.33f) : new Vector2(0.5f, 0.39f), ultraCompact ? new Vector2(320f, 3f) : new Vector2(220f, 3f), new Color(accent.r, accent.g, accent.b, 0.42f));
                TerminalUiFactory.CreateText(root, "Score", LocalizationService.IsChinese ? $"最高分  {CampaignProgressService.GetHighScore(level.ChapterIndex)}" : $"HIGH SCORE  {CampaignProgressService.GetHighScore(level.ChapterIndex)}", ultraCompact ? 14 : 16, TextAnchor.MiddleCenter, scoreAnchor, ultraCompact ? new Vector2(190f, 24f) : new Vector2(220f, 26f), new Color(0.92f, 0.96f, 1f, 0.94f));
                string stateText = unlocked ? (CampaignProgressService.IsCompleted(level.ChapterIndex) ? LocalizationService.Text("CLEARED", "已通关") : LocalizationService.Text("READY", "就绪")) : LocalizationService.Text("LOCKED", "未解锁");
                TerminalUiFactory.CreateText(root, "State", stateText, ultraCompact ? 15 : 17, TextAnchor.MiddleCenter, stateAnchor, ultraCompact ? new Vector2(120f, 24f) : new Vector2(180f, 26f), unlocked ? accent : new Color(0.76f, 0.56f, 0.36f, 0.88f));

                Button button = TerminalUiFactory.CreateButton(root, "DeployButton", unlocked ? LocalizationService.Text("DEPLOY", "部署") : LocalizationService.Text("SEALED", "封锁"), buttonAnchor, ultraCompact ? new Vector2(86f, 84f) : new Vector2(180f, 42f), unlocked ? new Color(accent.r * 0.7f + 0.08f, accent.g * 0.7f + 0.12f, accent.b * 0.7f + 0.18f, 0.94f) : new Color(0.18f, 0.18f, 0.22f, 0.72f), Color.white);
                button.interactable = unlocked;
                if (unlocked)
                {
                    LevelDefinition capturedLevel = level;
                    button.onClick.AddListener(() => StartLevel(capturedLevel));
                }
            }
        }

        private static Vector2 ResolveCardAnchor(int index, int totalCount, bool ultraCompact)
        {
            if (!ultraCompact)
            {
                float startX = totalCount == 3 ? 0.18f : 0.16f;
                float step = totalCount <= 1 ? 0f : 0.32f;
                return new Vector2(startX + index * step, 0.5f);
            }

            float[] yAnchors = totalCount switch
            {
                1 => new[] { 0.5f },
                2 => new[] { 0.68f, 0.32f },
                _ => new[] { 0.80f, 0.50f, 0.20f }
            };

            float y = index < yAnchors.Length ? yAnchors[index] : Mathf.Lerp(0.8f, 0.2f, totalCount <= 1 ? 0f : (float)index / (totalCount - 1));
            return new Vector2(0.5f, y);
        }

        private static Sprite ResolvePreviewSprite(LevelDefinition level)
        {
            switch (level.Theme)
            {
                case LevelTheme.Orbital:
                    return RuntimeArtLibrary.Get(RuntimeArtSpriteId.PlayerShip);
                case LevelTheme.AsteroidBelt:
                    return RuntimeArtLibrary.Get(RuntimeArtSpriteId.EnemyShipElite) ?? RuntimeArtLibrary.Get(RuntimeArtSpriteId.EnemyShip);
                case LevelTheme.DeepSpace:
                    return RuntimeArtLibrary.Get(RuntimeArtSpriteId.BossShipPhase2) ?? RuntimeArtLibrary.Get(RuntimeArtSpriteId.BossShip);
                default:
                    return RuntimeArtLibrary.Get(RuntimeArtSpriteId.PlayerShip);
            }
        }

        private void TryUpgrade(UpgradeType type)
        {
            if (CampaignProgressService.TryUpgrade(type))
            {
                RefreshAll();
            }
        }

        private void StartLevel(LevelDefinition level)
        {
            GameFlowController flow = FindFirstObjectByType<GameFlowController>();
            if (flow != null)
            {
                flow.StartLevel(level);
                return;
            }

            CampaignRuntime.CurrentLevel = level;
            SceneManager.LoadScene(level.SceneName);
        }

        private void BackToMenu()
        {
            GameFlowController flow = FindFirstObjectByType<GameFlowController>();
            if (flow != null)
            {
                flow.ReturnToMenu();
                return;
            }

            SceneManager.LoadScene("MainMenu");
        }

        private void SelectPreviousShip()
        {
            CycleShipSelection(-1);
        }

        private void SelectNextShip()
        {
            CycleShipSelection(1);
        }

        private void CycleShipSelection(int direction)
        {
            IReadOnlyList<ShipDefinition> ships = ShipCatalog.GetAll();
            if (ships == null || ships.Count == 0)
            {
                return;
            }

            ShipId current = CampaignProgressService.GetSelectedShipId();
            int index = 0;
            for (int i = 0; i < ships.Count; i++)
            {
                if (ships[i] != null && ships[i].ShipId == current)
                {
                    index = i;
                    break;
                }
            }

            index = (index + direction + ships.Count) % ships.Count;
            CampaignProgressService.SetSelectedShipId(ships[index].ShipId);
            RefreshAll();
        }

        private static Text EnsureText(Text existing, Transform parent, string name, int fontSize, TextAnchor alignment, Vector2 anchor, Vector2 size, Color color)
        {
            Text text = existing;
            if (text == null)
            {
                text = TerminalUiFactory.CreateText(parent, name, string.Empty, fontSize, alignment, anchor, size, color);
            }
            else
            {
                text.transform.SetParent(parent, false);
                RectTransform rect = text.rectTransform;
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                text.font = TerminalUiFactory.GetUiFont();
                text.fontSize = fontSize;
                text.fontStyle = FontStyle.Bold;
                text.alignment = alignment;
                text.color = color;
            }
            return text;
        }

        private static Button EnsureButton(Button existing, Transform parent, string name, Vector2 anchor, Vector2 size, Color fill, UnityEngine.Events.UnityAction onClick)
        {
            Button button = existing;
            if (button == null)
            {
                button = TerminalUiFactory.CreateButton(parent, name, string.Empty, anchor, size, fill, Color.white);
                button.onClick.AddListener(onClick);
            }
            else
            {
                button.transform.SetParent(parent, false);
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = fill;
                }
            }
            return button;
        }

        private static Button EnsureLabeledButton(Button existing, Transform parent, string name, string label, Vector2 anchor, Vector2 size, Color fill, UnityEngine.Events.UnityAction onClick)
        {
            Button button = EnsureButton(existing, parent, name, anchor, size, fill, onClick);
            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
            }

            return button;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
                text.font = TerminalUiFactory.GetUiFont();
                text.fontStyle = FontStyle.Bold;
            }
        }
    }
}




