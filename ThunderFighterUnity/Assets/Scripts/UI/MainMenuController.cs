using ThunderFighter.Config;
using ThunderFighter.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThunderFighter.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text statusText;

        private Text operationsText;
        private Text chapterInfoText;
        private Text upgradeSummaryText;
        private Text shipSelectText;
        private Text shipDetailText;
        private Button languageButton;
        private Button settingsButton;
        private Button shipPrevButton;
        private Button shipNextButton;
        private GameObject settingsPanel;
        private Text settingsHeaderText;
        private Text controlsText;
        private Text masterVolumeText;
        private Text musicVolumeText;
        private Text sfxVolumeText;
        private Text fullscreenText;
        private Text resolutionText;
        private Button settingsCloseButton;
        private Button masterMinusButton;
        private Button masterPlusButton;
        private Button musicMinusButton;
        private Button musicPlusButton;
        private Button sfxMinusButton;
        private Button sfxPlusButton;
        private Button fullscreenToggleButton;
        private Button resolutionPrevButton;
        private Button resolutionNextButton;

        private void Awake()
        {
            startButton?.onClick.AddListener(OnStartClicked);
            quitButton?.onClick.AddListener(OnQuitClicked);
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
            startButton?.onClick.RemoveListener(OnStartClicked);
            quitButton?.onClick.RemoveListener(OnQuitClicked);
            languageButton?.onClick.RemoveListener(ToggleLanguage);
            settingsButton?.onClick.RemoveListener(ToggleSettingsPanel);
            settingsCloseButton?.onClick.RemoveListener(HideSettingsPanel);
            shipPrevButton?.onClick.RemoveListener(SelectPreviousShip);
            shipNextButton?.onClick.RemoveListener(SelectNextShip);
            masterMinusButton?.onClick.RemoveListener(DecreaseMasterVolume);
            masterPlusButton?.onClick.RemoveListener(IncreaseMasterVolume);
            musicMinusButton?.onClick.RemoveListener(DecreaseMusicVolume);
            musicPlusButton?.onClick.RemoveListener(IncreaseMusicVolume);
            sfxMinusButton?.onClick.RemoveListener(DecreaseSfxVolume);
            sfxPlusButton?.onClick.RemoveListener(IncreaseSfxVolume);
            fullscreenToggleButton?.onClick.RemoveListener(ToggleFullscreen);
            resolutionPrevButton?.onClick.RemoveListener(PreviousResolution);
            resolutionNextButton?.onClick.RemoveListener(NextResolution);
        }

        private void Start()
        {
            EnsureRuntimeUi();
            RefreshText();
            RefreshSettingsPanel();
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

            TerminalUiFactory.CreateStretchPanel(canvas.transform, "BackdropTint", new Color(0.01f, 0.03f, 0.07f, 0.18f));
            TerminalUiFactory.CreateGlow(canvas.transform, "Glow_Main_01", new Vector2(0.14f, 0.78f), new Vector2(compact ? 340f : 460f, compact ? 340f : 460f), new Color(0.16f, 0.62f, 1f, 0.12f));
            TerminalUiFactory.CreateGlow(canvas.transform, "Glow_Main_02", new Vector2(0.84f, 0.22f), new Vector2(compact ? 360f : 520f, compact ? 360f : 520f), new Color(0.08f, 0.88f, 1f, 0.08f));

            Vector2 leftPanelAnchor = ultraCompact ? new Vector2(0.29f, 0.5f) : (compact ? new Vector2(0.28f, 0.52f) : new Vector2(0.25f, 0.5f));
            Vector2 leftPanelSize = ultraCompact ? new Vector2(450f, 620f) : (compact ? new Vector2(560f, 660f) : new Vector2(660f, 680f));
            Vector2 rightPanelAnchor = ultraCompact ? new Vector2(0.77f, 0.5f) : (compact ? new Vector2(0.76f, 0.52f) : new Vector2(0.76f, 0.5f));
            Vector2 rightPanelSize = ultraCompact ? new Vector2(250f, 620f) : (compact ? new Vector2(340f, 660f) : new Vector2(430f, 680f));

            Image leftPanel = TerminalUiFactory.CreatePanel(canvas.transform, "OperationsPanel", leftPanelAnchor, leftPanelSize, new Color(0.03f, 0.08f, 0.14f, 0.84f));
            Image rightPanel = TerminalUiFactory.CreatePanel(canvas.transform, "TacticalPanel", rightPanelAnchor, rightPanelSize, new Color(0.04f, 0.08f, 0.16f, 0.78f));
            TerminalUiFactory.AddHorizontalDivider(leftPanel.transform, "DividerTop", new Vector2(0.5f, 0.79f), new Vector2(ultraCompact ? 360f : (compact ? 450f : 560f), 4f), new Color(0.46f, 0.86f, 1f, 0.48f));
            TerminalUiFactory.AddHorizontalDivider(rightPanel.transform, "DividerTop", new Vector2(0.5f, 0.79f), new Vector2(ultraCompact ? 180f : (compact ? 250f : 320f), 4f), new Color(0.46f, 0.86f, 1f, 0.48f));

            subtitleText = EnsureText(subtitleText, leftPanel.transform, "Subtitle", ultraCompact ? 20 : 22, TextAnchor.MiddleLeft, new Vector2(ultraCompact ? 0.16f : 0.14f, 0.83f), new Vector2(ultraCompact ? 330f : (compact ? 410f : 500f), 46f), new Color(0.68f, 0.92f, 1f, 1f));
            statusText = EnsureText(statusText, leftPanel.transform, "Status", ultraCompact ? 16 : (compact ? 18 : 20), TextAnchor.UpperLeft, new Vector2(0.5f, 0.72f), new Vector2(ultraCompact ? 370f : (compact ? 470f : 540f), ultraCompact ? 64f : 72f), new Color(0.9f, 0.96f, 1f, 0.96f));
            operationsText = EnsureText(operationsText, leftPanel.transform, "OperationsText", ultraCompact ? 16 : (compact ? 18 : 20), TextAnchor.UpperLeft, new Vector2(0.5f, ultraCompact ? 0.46f : 0.47f), new Vector2(ultraCompact ? 370f : (compact ? 470f : 540f), ultraCompact ? 220f : 240f), new Color(0.78f, 0.92f, 1f, 0.96f));
            chapterInfoText = EnsureText(chapterInfoText, rightPanel.transform, "ChapterInfo", ultraCompact ? 15 : (compact ? 17 : 18), TextAnchor.UpperLeft, new Vector2(0.5f, 0.5f), new Vector2(ultraCompact ? 190f : (compact ? 250f : 320f), ultraCompact ? 220f : 250f), new Color(0.78f, 0.92f, 1f, 0.96f));
            upgradeSummaryText = EnsureText(upgradeSummaryText, rightPanel.transform, "UpgradeSummary", ultraCompact ? 15 : (compact ? 17 : 18), TextAnchor.UpperLeft, new Vector2(0.5f, ultraCompact ? 0.19f : 0.21f), new Vector2(ultraCompact ? 190f : (compact ? 250f : 320f), ultraCompact ? 120f : 140f), new Color(1f, 0.86f, 0.56f, 0.96f));

            startButton = EnsureButton(startButton, leftPanel.transform, "StartButton", LocalizationService.Text("ENTER CAMPAIGN GRID", "\u8fdb\u5165\u6218\u5f79\u90e8\u7f72"), new Vector2(ultraCompact ? 0.29f : 0.31f, 0.12f), new Vector2(ultraCompact ? 160f : (compact ? 220f : 250f), ultraCompact ? 56f : 62f), new Color(0.08f, 0.44f, 0.88f, 0.95f), OnStartClicked);
            settingsButton = EnsureButton(settingsButton, leftPanel.transform, "SettingsButton", LocalizationService.Text("SETTINGS", "\u8bbe\u7f6e"), new Vector2(ultraCompact ? 0.56f : 0.58f, 0.12f), new Vector2(ultraCompact ? 120f : (compact ? 160f : 180f), ultraCompact ? 56f : 62f), new Color(0.1f, 0.34f, 0.56f, 0.94f), ToggleSettingsPanel);
            quitButton = EnsureButton(quitButton, leftPanel.transform, "QuitButton", LocalizationService.Text("POWER DOWN", "\u9000\u51fa\u7cfb\u7edf"), new Vector2(ultraCompact ? 0.81f : 0.81f, 0.12f), new Vector2(ultraCompact ? 110f : (compact ? 150f : 160f), ultraCompact ? 56f : 62f), new Color(0.18f, 0.22f, 0.32f, 0.92f), OnQuitClicked);
            languageButton = EnsureButton(languageButton, leftPanel.transform, "LanguageButton", LocalizationService.LanguageButtonLabel(), new Vector2(0.84f, ultraCompact ? 0.89f : 0.92f), new Vector2(ultraCompact ? 118f : (compact ? 150f : 176f), 40f), new Color(0.12f, 0.24f, 0.38f, 0.92f), ToggleLanguage);

            Sprite shipSprite = RuntimeArtLibrary.Get(RuntimeArtLibrary.GetPlayerShipSpriteId(CampaignProgressService.GetSelectedShipId()))
                ?? GeneratedSpriteLibrary.GetShipPresentationSprite(CampaignProgressService.GetSelectedShipId(), true)
                ?? RuntimeArtLibrary.Get(RuntimeArtSpriteId.PlayerShip);
            if (shipSprite != null && rightPanel.transform.Find("HangarShipPreview") == null)
            {
                TerminalUiFactory.CreateSprite(rightPanel.transform, "HangarShipPreview", shipSprite, new Vector2(0.5f, 0.82f), compact ? new Vector2(180f, 126f) : new Vector2(230f, 160f), new Color(1f, 1f, 1f, 0.95f));
            }

            shipSelectText = EnsureText(shipSelectText, rightPanel.transform, "ShipSelectText", ultraCompact ? 16 : 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.69f), new Vector2(ultraCompact ? 180f : 260f, 34f), new Color(0.9f, 0.98f, 1f, 0.98f));
            shipDetailText = EnsureText(shipDetailText, rightPanel.transform, "ShipDetailText", ultraCompact ? 13 : 15, TextAnchor.UpperCenter, new Vector2(0.5f, 0.61f), new Vector2(ultraCompact ? 190f : 280f, 72f), new Color(0.78f, 0.92f, 1f, 0.94f));
            shipPrevButton = EnsureButton(shipPrevButton, rightPanel.transform, "ShipPrevButton", "<", new Vector2(0.18f, 0.69f), new Vector2(38f, 34f), new Color(0.16f, 0.24f, 0.36f, 0.92f), SelectPreviousShip);
            shipNextButton = EnsureButton(shipNextButton, rightPanel.transform, "ShipNextButton", ">", new Vector2(0.82f, 0.69f), new Vector2(38f, 34f), new Color(0.12f, 0.42f, 0.78f, 0.92f), SelectNextShip);

            EnsureChip(rightPanel.transform, 0.2f, 1, new Color(0.34f, 0.86f, 1f, 0.94f));
            EnsureChip(rightPanel.transform, 0.5f, 2, new Color(1f, 0.7f, 0.3f, 0.94f));
            EnsureChip(rightPanel.transform, 0.8f, 3, new Color(0.62f, 0.84f, 1f, 0.94f));

            EnsureSettingsPanel(canvas.transform, ultraCompact, compact);
        }

        private void EnsureSettingsPanel(Transform canvas, bool ultraCompact, bool compact)
        {
            if (settingsPanel != null)
            {
                return;
            }

            Image overlay = TerminalUiFactory.CreateStretchPanel(canvas, "SettingsOverlay", new Color(0.01f, 0.03f, 0.08f, 0.64f));
            settingsPanel = overlay.gameObject;
            settingsPanel.SetActive(false);

            Image panel = TerminalUiFactory.CreatePanel(settingsPanel.transform, "SettingsPanel", new Vector2(0.5f, 0.5f), ultraCompact ? new Vector2(620f, 560f) : (compact ? new Vector2(760f, 620f) : new Vector2(860f, 660f)), new Color(0.03f, 0.08f, 0.14f, 0.94f));
            TerminalUiFactory.AddHorizontalDivider(panel.transform, "DividerTop", new Vector2(0.5f, 0.9f), new Vector2(ultraCompact ? 440f : 560f, 4f), new Color(0.46f, 0.86f, 1f, 0.48f));
            settingsHeaderText = TerminalUiFactory.CreateText(panel.transform, "Header", string.Empty, ultraCompact ? 26 : 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.93f), new Vector2(460f, 40f), Color.white);

            masterVolumeText = TerminalUiFactory.CreateText(panel.transform, "MasterText", string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(0.24f, 0.76f), new Vector2(260f, 32f), new Color(0.82f, 0.94f, 1f, 0.98f));
            musicVolumeText = TerminalUiFactory.CreateText(panel.transform, "MusicText", string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(0.24f, 0.66f), new Vector2(260f, 32f), new Color(0.82f, 0.94f, 1f, 0.98f));
            sfxVolumeText = TerminalUiFactory.CreateText(panel.transform, "SfxText", string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(0.24f, 0.56f), new Vector2(260f, 32f), new Color(0.82f, 0.94f, 1f, 0.98f));
            fullscreenText = TerminalUiFactory.CreateText(panel.transform, "FullscreenText", string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(0.24f, 0.44f), new Vector2(260f, 32f), new Color(0.82f, 0.94f, 1f, 0.98f));
            resolutionText = TerminalUiFactory.CreateText(panel.transform, "ResolutionText", string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(0.24f, 0.34f), new Vector2(260f, 32f), new Color(0.82f, 0.94f, 1f, 0.98f));
            controlsText = TerminalUiFactory.CreateText(panel.transform, "ControlsText", string.Empty, ultraCompact ? 15 : 17, TextAnchor.UpperLeft, new Vector2(0.68f, 0.55f), new Vector2(260f, 320f), new Color(1f, 0.88f, 0.62f, 0.96f));

            masterMinusButton = TerminalUiFactory.CreateButton(panel.transform, "MasterMinus", "-", new Vector2(0.47f, 0.76f), new Vector2(42f, 34f), new Color(0.16f, 0.24f, 0.36f, 0.92f), Color.white);
            masterPlusButton = TerminalUiFactory.CreateButton(panel.transform, "MasterPlus", "+", new Vector2(0.62f, 0.76f), new Vector2(42f, 34f), new Color(0.12f, 0.42f, 0.78f, 0.92f), Color.white);
            musicMinusButton = TerminalUiFactory.CreateButton(panel.transform, "MusicMinus", "-", new Vector2(0.47f, 0.66f), new Vector2(42f, 34f), new Color(0.16f, 0.24f, 0.36f, 0.92f), Color.white);
            musicPlusButton = TerminalUiFactory.CreateButton(panel.transform, "MusicPlus", "+", new Vector2(0.62f, 0.66f), new Vector2(42f, 34f), new Color(0.12f, 0.42f, 0.78f, 0.92f), Color.white);
            sfxMinusButton = TerminalUiFactory.CreateButton(panel.transform, "SfxMinus", "-", new Vector2(0.47f, 0.56f), new Vector2(42f, 34f), new Color(0.16f, 0.24f, 0.36f, 0.92f), Color.white);
            sfxPlusButton = TerminalUiFactory.CreateButton(panel.transform, "SfxPlus", "+", new Vector2(0.62f, 0.56f), new Vector2(42f, 34f), new Color(0.12f, 0.42f, 0.78f, 0.92f), Color.white);
            fullscreenToggleButton = TerminalUiFactory.CreateButton(panel.transform, "FullscreenButton", string.Empty, new Vector2(0.54f, 0.44f), new Vector2(170f, 38f), new Color(0.12f, 0.42f, 0.78f, 0.92f), Color.white);
            resolutionPrevButton = TerminalUiFactory.CreateButton(panel.transform, "ResolutionPrev", "<", new Vector2(0.47f, 0.34f), new Vector2(42f, 34f), new Color(0.16f, 0.24f, 0.36f, 0.92f), Color.white);
            resolutionNextButton = TerminalUiFactory.CreateButton(panel.transform, "ResolutionNext", ">", new Vector2(0.62f, 0.34f), new Vector2(42f, 34f), new Color(0.12f, 0.42f, 0.78f, 0.92f), Color.white);
            settingsCloseButton = TerminalUiFactory.CreateButton(panel.transform, "SettingsClose", string.Empty, new Vector2(0.5f, 0.09f), new Vector2(220f, 48f), new Color(0.18f, 0.22f, 0.32f, 0.92f), Color.white);

            masterMinusButton.onClick.AddListener(DecreaseMasterVolume);
            masterPlusButton.onClick.AddListener(IncreaseMasterVolume);
            musicMinusButton.onClick.AddListener(DecreaseMusicVolume);
            musicPlusButton.onClick.AddListener(IncreaseMusicVolume);
            sfxMinusButton.onClick.AddListener(DecreaseSfxVolume);
            sfxPlusButton.onClick.AddListener(IncreaseSfxVolume);
            fullscreenToggleButton.onClick.AddListener(ToggleFullscreen);
            resolutionPrevButton.onClick.AddListener(PreviousResolution);
            resolutionNextButton.onClick.AddListener(NextResolution);
            settingsCloseButton.onClick.AddListener(HideSettingsPanel);
        }

        private void RefreshText()
        {
            subtitleText.text = LocalizationService.Text("TACTICAL HANGAR  |  FLEET DEPLOYMENT TERMINAL", "\u6218\u672f\u673a\u5e93  |  \u8230\u961f\u90e8\u7f72\u7ec8\u7aef");
            statusText.text = LocalizationService.IsChinese
                ? $"\u5df2\u89e3\u9501\u7ae0\u8282  {CampaignProgressService.HighestUnlockedChapter}/3\n\u6218\u529b\u8bc4\u7ea7  {CampaignProgressService.GetPowerRating()}    \u6280\u672f\u70b9  {CampaignProgressService.TechPoints}"
                : $"UNLOCKED CHAPTERS  {CampaignProgressService.HighestUnlockedChapter}/3\nPOWER RATING  {CampaignProgressService.GetPowerRating()}    TECH  {CampaignProgressService.TechPoints}";

            operationsText.text = LocalizationService.IsChinese
                ? "\u4f5c\u6218\u7b80\u62a5\n\n1. \u8fdb\u5165\u90e8\u7f72\u7f51\u683c\uff0c\u9009\u62e9\u5f53\u524d\u6218\u5f79\u7ae0\u8282\u3002\n2. \u4f7f\u7528\u6218\u6597\u6536\u76ca\u5347\u7ea7\u706b\u529b\u3001\u88c5\u7532\u4e0e\u53cd\u5e94\u5806\u3002\n3. \u51fb\u7834\u654c\u65b9\u6307\u6325\u5c42\uff0c\u9010\u6b65\u89e3\u9501\u540e\u7eed\u6218\u533a\u3002\n\n\u5b9e\u65f6\u6218\u51b5\n" +
                  $"- \u7b2c\u4e00\u7ae0\uff1a{(CampaignProgressService.IsCompleted(1) ? "\u8f68\u9053\u5df2\u8083\u6e05" : "\u7b49\u5f85\u9996\u6b21\u901a\u5173")}\n" +
                  $"- \u7b2c\u4e8c\u7ae0\uff1a{(CampaignProgressService.IsChapterUnlocked(2) ? "\u9668\u77f3\u5e26\u6218\u533a\u5f00\u653e" : "\u4ecd\u5904\u4e8e\u5c01\u9501\u72b6\u6001")}\n" +
                  $"- \u7b2c\u4e09\u7ae0\uff1a{(CampaignProgressService.IsChapterUnlocked(3) ? "\u65d7\u8230\u6218\u533a\u5df2\u4e0a\u7ebf" : "\u5c1a\u672a\u89e3\u5bc6")}"
                : "OPERATIONS BRIEF\n\n1. Enter the deployment grid and select a campaign chapter.\n2. Spend combat gains on firepower, armor, and reactor upgrades.\n3. Break hostile command layers and unlock the next theater.\n\nLIVE FEED\n" +
                  $"- Chapter 1: {(CampaignProgressService.IsCompleted(1) ? "Orbit secured" : "Awaiting first clear")}\n" +
                  $"- Chapter 2: {(CampaignProgressService.IsChapterUnlocked(2) ? "Asteroid breach available" : "Lockout active")}\n" +
                  $"- Chapter 3: {(CampaignProgressService.IsChapterUnlocked(3) ? "Flagship sector online" : "Encrypted")}";

            chapterInfoText.text = LocalizationService.IsChinese
                ? "\u6218\u533a\u603b\u89c8\n\n" +
                  $"CH-01  \u8f68\u9053\u62e6\u622a        \u6700\u9ad8\u5206 {CampaignProgressService.GetHighScore(1)}\n" +
                  $"CH-02  \u9668\u77f3\u5e26\u7a81\u5165      \u6700\u9ad8\u5206 {CampaignProgressService.GetHighScore(2)}\n" +
                  $"CH-03  \u6df1\u7a7a\u65d7\u8230\u6218      \u6700\u9ad8\u5206 {CampaignProgressService.GetHighScore(3)}\n\n" +
                  "\u4efb\u52a1\u7279\u5f81\n- \u591a\u9636\u6bb5 Boss \u6218\n- \u7ae0\u8282\u9010\u6b65\u89e3\u9501\n- \u6301\u4e45\u673a\u4f53\u5f3a\u5316"
                : "TACTICAL GRID\n\n" +
                  $"CH-01  ORBITAL INTERCEPT    HIGH SCORE {CampaignProgressService.GetHighScore(1)}\n" +
                  $"CH-02  ASTEROID BREACH      HIGH SCORE {CampaignProgressService.GetHighScore(2)}\n" +
                  $"CH-03  DEEP SPACE FLAGSHIP  HIGH SCORE {CampaignProgressService.GetHighScore(3)}\n\n" +
                  "MISSION PROFILE\n- Multi-stage boss engagements\n- Progressive unlock campaign\n- Persistent ship upgrades";

            upgradeSummaryText.text = LocalizationService.IsChinese
                ? "\u5f3a\u5316\u6458\u8981\n\n" +
                  $"\u706b\u529b      Lv.{CampaignProgressService.GetUpgradeLevel(UpgradeType.Firepower)}\n" +
                  $"\u88c5\u7532      Lv.{CampaignProgressService.GetUpgradeLevel(UpgradeType.Armor)}\n" +
                  $"\u53cd\u5e94\u5806    Lv.{CampaignProgressService.GetUpgradeLevel(UpgradeType.Reactor)}"
                : "UPGRADE SUMMARY\n\n" +
                  $"FIREPOWER  Lv.{CampaignProgressService.GetUpgradeLevel(UpgradeType.Firepower)}\n" +
                  $"ARMOR      Lv.{CampaignProgressService.GetUpgradeLevel(UpgradeType.Armor)}\n" +
                  $"REACTOR    Lv.{CampaignProgressService.GetUpgradeLevel(UpgradeType.Reactor)}";

            SetButtonLabel(startButton, LocalizationService.Text("ENTER CAMPAIGN GRID", "\u8fdb\u5165\u6218\u5f79\u90e8\u7f72"));
            SetButtonLabel(settingsButton, LocalizationService.Text("SETTINGS", "\u8bbe\u7f6e"));
            SetButtonLabel(quitButton, LocalizationService.Text("POWER DOWN", "\u9000\u51fa\u7cfb\u7edf"));
            SetButtonLabel(languageButton, LocalizationService.LanguageButtonLabel());
            RefreshShipSelection();
            RefreshSettingsPanel();
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
                shipSelectText.text = LocalizationService.IsChinese
                    ? $"当前机型  {ship.GetDisplayName(true)}"
                    : $"ACTIVE FRAME  {ship.GetDisplayName(false).ToUpperInvariant()}";
                shipSelectText.color = ship.AccentColor;
            }

            if (shipDetailText != null)
            {
                shipDetailText.text = LocalizationService.IsChinese
                    ? $"{ship.GetSubtitle(true)}\n主技能：{ship.GetSkillOneName(true)}\n强化技：{ship.GetSkillTwoName(true)}"
                    : $"{ship.GetSubtitle(false)}\nSkill 1: {ship.GetSkillOneName(false)}\nSkill 2: {ship.GetSkillTwoName(false)}";
            }

            Transform preview = chapterInfoText != null ? chapterInfoText.transform.parent.Find("HangarShipPreview") : null;
            if (preview != null)
            {
                Image image = preview.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = RuntimeArtLibrary.Get(RuntimeArtLibrary.GetPlayerShipSpriteId(ship.ShipId))
                        ?? GeneratedSpriteLibrary.GetShipPresentationSprite(ship.ShipId, true)
                        ?? image.sprite;
                    image.color = Color.white;
                }
            }
        }

        private void RefreshSettingsPanel()
        {
            if (settingsPanel == null)
            {
                return;
            }

            settingsHeaderText.text = LocalizationService.Text("TACTICAL SETTINGS", "\u6218\u672f\u8bbe\u7f6e");
            masterVolumeText.text = FormatSettingsLine(LocalizationService.Text("MASTER VOLUME", "\u603b\u97f3\u91cf"), GameSettingsService.GetPercentLabel(GameSettingsService.MasterVolume));
            musicVolumeText.text = FormatSettingsLine(LocalizationService.Text("MUSIC VOLUME", "\u97f3\u4e50\u97f3\u91cf"), GameSettingsService.GetPercentLabel(GameSettingsService.MusicVolume));
            sfxVolumeText.text = FormatSettingsLine(LocalizationService.Text("SFX VOLUME", "\u97f3\u6548\u97f3\u91cf"), GameSettingsService.GetPercentLabel(GameSettingsService.SfxVolume));
            fullscreenText.text = FormatSettingsLine(LocalizationService.Text("DISPLAY MODE", "\u663e\u793a\u6a21\u5f0f"), LocalizationService.Text(GameSettingsService.Fullscreen ? "Fullscreen" : "Windowed", GameSettingsService.Fullscreen ? "\u5168\u5c4f" : "\u7a97\u53e3"));
            resolutionText.text = FormatSettingsLine(LocalizationService.Text("RESOLUTION", "\u5206\u8fa8\u7387"), GameSettingsService.GetResolutionLabel());
            controlsText.text = LocalizationService.IsChinese
                ? "\u952e\u4f4d\u8bf4\u660e\n\nWASD / \u65b9\u5411\u952e  \u79fb\u52a8\nJ / \u9f20\u6807\u5de6\u952e  \u5f00\u706b\nSpace  \u77ed\u51b2\u523a / \u95ea\u907f\nK  \u7b49\u79bb\u5b50\u65b0\u661f\nL  \u8fc7\u8f7d\u7206\u53d1\nEsc  \u6682\u505c\n\n\u63d0\u793a\n- \u97f3\u91cf\u6539\u52a8\u4f1a\u7acb\u5373\u751f\u6548\n- \u5206\u8fa8\u7387\u4e0e\u5168\u5c4f\u8bbe\u7f6e\u4f1a\u5373\u65f6\u5e94\u7528\n- \u8bed\u8a00\u5207\u6362\u4f1a\u540c\u6b65\u5230\u6240\u6709\u7ec8\u7aef\u754c\u9762"
                : "CONTROLS\n\nWASD / Arrows  Move\nJ / Left Mouse  Fire\nSpace  Dash / Evade\nK  Plasma Nova\nL  Overdrive\nEsc  Pause\n\nNOTES\n- Audio changes apply immediately\n- Resolution and fullscreen switch in real time\n- Language updates the full terminal UI";

            SetButtonLabel(fullscreenToggleButton, LocalizationService.Text(GameSettingsService.Fullscreen ? "SWITCH TO WINDOWED" : "SWITCH TO FULLSCREEN", GameSettingsService.Fullscreen ? "\u5207\u6362\u4e3a\u7a97\u53e3" : "\u5207\u6362\u4e3a\u5168\u5c4f"));
            SetButtonLabel(settingsCloseButton, LocalizationService.Text("CLOSE SETTINGS", "\u5173\u95ed\u8bbe\u7f6e"));
        }

        private static string FormatSettingsLine(string label, string value)
        {
            return label + "  " + value;
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

        private static Button EnsureButton(Button existing, Transform parent, string name, string label, Vector2 anchor, Vector2 size, Color fill, UnityEngine.Events.UnityAction onClick)
        {
            Button button = existing;
            if (button == null)
            {
                button = TerminalUiFactory.CreateButton(parent, name, label, anchor, size, fill, Color.white);
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
                SetButtonLabel(button, label);
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
                text.alignment = TextAnchor.MiddleCenter;
            }
        }

        private static void EnsureChip(Transform parent, float anchorX, int chapter, Color color)
        {
            string name = "ChapterChip_" + chapter;
            if (parent.Find(name) != null)
            {
                return;
            }

            Image panel = TerminalUiFactory.CreatePanel(parent, name, new Vector2(anchorX, 0.94f), new Vector2(92f, 42f), new Color(color.r * 0.22f, color.g * 0.18f, color.b * 0.18f, 0.72f));
            TerminalUiFactory.CreateText(panel.transform, "Label", $"CH-{chapter:00}", 16, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(72f, 24f), color);
        }

        private void ToggleLanguage()
        {
            LocalizationService.ToggleLanguage();
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
            var ships = ShipCatalog.GetAll();
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
            RefreshText();
        }

        private void HandleLanguageChanged(GameLanguage _)
        {
            RefreshText();
        }

        private void ToggleSettingsPanel()
        {
            if (settingsPanel == null)
            {
                return;
            }

            settingsPanel.SetActive(!settingsPanel.activeSelf);
            if (settingsPanel.activeSelf)
            {
                RefreshSettingsPanel();
            }
        }

        private void HideSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void DecreaseMasterVolume() => ChangeMasterVolume(-0.1f);
        private void IncreaseMasterVolume() => ChangeMasterVolume(0.1f);
        private void DecreaseMusicVolume() => ChangeMusicVolume(-0.1f);
        private void IncreaseMusicVolume() => ChangeMusicVolume(0.1f);
        private void DecreaseSfxVolume() => ChangeSfxVolume(-0.1f);
        private void IncreaseSfxVolume() => ChangeSfxVolume(0.1f);
        private void PreviousResolution() => ChangeResolution(-1);
        private void NextResolution() => ChangeResolution(1);

        private void ChangeMasterVolume(float delta)
        {
            GameSettingsService.SetMasterVolume(GameSettingsService.MasterVolume + delta);
            RefreshSettingsPanel();
        }

        private void ChangeMusicVolume(float delta)
        {
            GameSettingsService.SetMusicVolume(GameSettingsService.MusicVolume + delta);
            RefreshSettingsPanel();
        }

        private void ChangeSfxVolume(float delta)
        {
            GameSettingsService.SetSfxVolume(GameSettingsService.SfxVolume + delta);
            RefreshSettingsPanel();
        }

        private void ToggleFullscreen()
        {
            GameSettingsService.SetFullscreen(!GameSettingsService.Fullscreen);
            RefreshSettingsPanel();
        }

        private void ChangeResolution(int direction)
        {
            GameSettingsService.CycleResolution(direction);
            RefreshSettingsPanel();
        }

        private void OnStartClicked()
        {
            HideSettingsPanel();
            GameFlowController flow = FindFirstObjectByType<GameFlowController>();
            if (flow != null)
            {
                flow.OpenChapterSelect();
                return;
            }

            SceneManager.LoadScene("ChapterSelect");
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}




