using ThunderFighter.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThunderFighter.UI
{
    public class ResultController : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text unlockText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button nextButton;

        private Text breakdownText;
        private Text summaryText;

        private void Awake()
        {
            retryButton?.onClick.AddListener(Retry);
            menuButton?.onClick.AddListener(BackToMenu);
            nextButton?.onClick.AddListener(NextChapter);
        }

        private void OnDestroy()
        {
            retryButton?.onClick.RemoveListener(Retry);
            menuButton?.onClick.RemoveListener(BackToMenu);
            nextButton?.onClick.RemoveListener(NextChapter);
        }

        private void Start()
        {
            EnsureRuntimeUi();
            Populate();
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

            TerminalUiFactory.CreateGlow(canvas.transform, "ResultGlowLeft", new Vector2(0.16f, 0.7f), new Vector2(compact ? 300f : 420f, compact ? 300f : 420f), new Color(0.16f, 0.62f, 1f, 0.12f));
            TerminalUiFactory.CreateGlow(canvas.transform, "ResultGlowRight", new Vector2(0.84f, 0.28f), new Vector2(compact ? 300f : 420f, compact ? 300f : 420f), new Color(1f, 0.48f, 0.24f, 0.08f));

            Vector2 mainPanelAnchor = ultraCompact ? new Vector2(0.35f, 0.52f) : (compact ? new Vector2(0.34f, 0.52f) : new Vector2(0.36f, 0.52f));
            Vector2 mainPanelSize = ultraCompact ? new Vector2(520f, 620f) : (compact ? new Vector2(620f, 660f) : new Vector2(720f, 680f));
            Vector2 sidePanelAnchor = ultraCompact ? new Vector2(0.79f, 0.52f) : (compact ? new Vector2(0.78f, 0.52f) : new Vector2(0.8f, 0.52f));
            Vector2 sidePanelSize = ultraCompact ? new Vector2(210f, 620f) : (compact ? new Vector2(280f, 660f) : new Vector2(340f, 680f));

            Image mainPanel = TerminalUiFactory.CreatePanel(canvas.transform, "ResultMainPanel", mainPanelAnchor, mainPanelSize, new Color(0.03f, 0.08f, 0.14f, 0.84f));
            Image sidePanel = TerminalUiFactory.CreatePanel(canvas.transform, "ResultSidePanel", sidePanelAnchor, sidePanelSize, new Color(0.05f, 0.08f, 0.16f, 0.78f));
            TerminalUiFactory.AddHorizontalDivider(mainPanel.transform, "DividerTop", new Vector2(0.5f, 0.8f), new Vector2(ultraCompact ? 400f : (compact ? 500f : 580f), 4f), new Color(0.46f, 0.86f, 1f, 0.48f));

            titleText = EnsureText(titleText, mainPanel.transform, "Title", ultraCompact ? 34 : (compact ? 42 : 50), TextAnchor.MiddleLeft, new Vector2(0.12f, 0.84f), new Vector2(ultraCompact ? 340f : (compact ? 420f : 500f), 70f), Color.white);
            scoreText = EnsureText(scoreText, mainPanel.transform, "Score", ultraCompact ? 20 : (compact ? 24 : 28), TextAnchor.MiddleLeft, new Vector2(0.12f, 0.72f), new Vector2(ultraCompact ? 380f : (compact ? 470f : 540f), 48f), new Color(0.86f, 0.94f, 1f, 0.96f));
            detailText = EnsureText(detailText, mainPanel.transform, "Detail", ultraCompact ? 16 : (compact ? 19 : 22), TextAnchor.UpperLeft, new Vector2(0.5f, 0.5f), new Vector2(ultraCompact ? 420f : (compact ? 520f : 590f), ultraCompact ? 120f : 130f), new Color(0.78f, 0.92f, 1f, 0.96f));
            unlockText = EnsureText(unlockText, mainPanel.transform, "Unlock", ultraCompact ? 15 : (compact ? 18 : 20), TextAnchor.UpperLeft, new Vector2(0.5f, 0.28f), new Vector2(ultraCompact ? 420f : (compact ? 520f : 590f), 60f), new Color(1f, 0.84f, 0.54f, 0.98f));
            breakdownText = EnsureText(breakdownText, sidePanel.transform, "Breakdown", ultraCompact ? 15 : (compact ? 18 : 20), TextAnchor.UpperLeft, new Vector2(0.5f, 0.66f), new Vector2(ultraCompact ? 160f : (compact ? 220f : 260f), 220f), new Color(0.84f, 0.94f, 1f, 0.96f));
            summaryText = EnsureText(summaryText, sidePanel.transform, "Summary", ultraCompact ? 14 : (compact ? 17 : 18), TextAnchor.UpperLeft, new Vector2(0.5f, 0.3f), new Vector2(ultraCompact ? 160f : (compact ? 220f : 260f), 190f), new Color(0.92f, 0.84f, 0.62f, 0.96f));

            retryButton = EnsureButton(retryButton, mainPanel.transform, "RetryButton", new Vector2(0.22f, 0.12f), ultraCompact ? new Vector2(135f, 54f) : (compact ? new Vector2(180f, 56f) : new Vector2(210f, 58f)), new Color(0.08f, 0.42f, 0.82f, 0.95f), Retry);
            nextButton = EnsureButton(nextButton, mainPanel.transform, "NextButton", new Vector2(0.50f, 0.12f), ultraCompact ? new Vector2(145f, 54f) : (compact ? new Vector2(190f, 56f) : new Vector2(230f, 58f)), new Color(0.1f, 0.52f, 0.92f, 0.95f), NextChapter);
            menuButton = EnsureButton(menuButton, mainPanel.transform, "MenuButton", new Vector2(0.78f, 0.12f), ultraCompact ? new Vector2(150f, 54f) : (compact ? new Vector2(200f, 56f) : new Vector2(240f, 58f)), new Color(0.18f, 0.22f, 0.32f, 0.92f), BackToMenu);
        }

        private void Populate()
        {
            CampaignRunResult result = CampaignRuntime.LastResult;
            if (result == null)
            {
                result = new CampaignRunResult
                {
                    ChapterTitle = "Simulation",
                    Subtitle = string.Empty,
                    Score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0,
                    Victory = false,
                    Rating = "D",
                    EarnedTechPoints = 0,
                    TotalTechPoints = CampaignProgressService.TechPoints
                };
            }

            titleText.text = LocalizationService.IsChinese
                ? $"{LocalizationService.TranslateLiteral(result.ChapterTitle)}{(result.Victory ? " \u901a\u5173" : " \u5931\u8d25")}"
                : $"{result.ChapterTitle} {(result.Victory ? "CLEAR" : "FAILED")}";
            titleText.color = result.Victory ? new Color(0.72f, 0.98f, 1f, 1f) : new Color(1f, 0.5f, 0.42f, 1f);
            scoreText.text = LocalizationService.IsChinese ? $"\u8bc4\u7ea7  {result.Rating}    |    \u5206\u6570  {result.Score}" : $"RATING  {result.Rating}    |    SCORE  {result.Score}";
            detailText.text = LocalizationService.IsChinese
                ? $"{LocalizationService.TranslateLiteral(result.Subtitle)}\n\u6280\u672f\u70b9 +{result.EarnedTechPoints}    |    \u603b\u8ba1 {result.TotalTechPoints}"
                : $"{result.Subtitle}\nTECH POINTS +{result.EarnedTechPoints}    |    TOTAL {result.TotalTechPoints}";
            unlockText.text = result.UnlockedNextChapter
                ? (LocalizationService.IsChinese ? $"\u65b0\u7ae0\u8282\u5df2\u89e3\u9501  {LocalizationService.TranslateLiteral(result.UnlockedChapterTitle)}" : $"NEW CHAPTER UNLOCKED  {result.UnlockedChapterTitle}")
                : (result.Victory ? LocalizationService.Text("UPLINK COMPLETE  RETURN TO DEPLOYMENT", "\u94fe\u8def\u540c\u6b65\u5b8c\u6210  \u8fd4\u56de\u90e8\u7f72") : LocalizationService.Text("RECALIBRATE LOADOUT AND RETRY", "\u91cd\u65b0\u6821\u51c6\u914d\u7f6e\u540e\u518d\u6b21\u51fa\u51fb"));

            breakdownText.text = LocalizationService.IsChinese
                ? "\u4efb\u52a1\u7ed3\u7b97\n\n" +
                  $"\u7ae0\u8282          {LocalizationService.TranslateLiteral(result.ChapterTitle)}\n" +
                  $"\u7ed3\u679c          {(result.Victory ? "\u6210\u529f" : "\u5931\u8d25")}\n" +
                  $"\u8bc4\u7ea7          {result.Rating}\n" +
                  $"\u6280\u672f\u70b9\u6536\u76ca    +{result.EarnedTechPoints}\n" +
                  $"\u540e\u7eed\u6743\u9650      {(result.UnlockedNextChapter ? LocalizationService.TranslateLiteral(result.UnlockedChapterTitle) : "\u65e0\u65b0\u589e\u89e3\u9501")}"
                : "MISSION BREAKDOWN\n\n" +
                  $"CHAPTER        {result.ChapterTitle}\n" +
                  $"OUTCOME        {(result.Victory ? "SUCCESS" : "FAILURE")}\n" +
                  $"RATING         {result.Rating}\n" +
                  $"TECH GAIN      +{result.EarnedTechPoints}\n" +
                  $"NEXT ACCESS    {(result.UnlockedNextChapter ? result.UnlockedChapterTitle : "NO NEW ACCESS")}";

            summaryText.text = LocalizationService.IsChinese
                ? "\u6218\u540e\u7b80\u62a5\n\n" +
                  (result.Victory ? "\u654c\u65b9\u6307\u6325\u94fe\u5df2\u7ecf\u74e6\u89e3\uff0c\u5efa\u8bae\u6574\u5907\u540e\u7ee7\u7eed\u63a8\u8fdb\u4e0b\u4e00\u6218\u533a\u3002" : "\u672c\u6b21\u4f5c\u6218\u66b4\u9732\u4e86\u7a81\u7834\u8def\u7ebf\u7684\u5f31\u70b9\uff0c\u5efa\u8bae\u5347\u7ea7\u914d\u7f6e\u540e\u91cd\u65b0\u51fa\u51fb\u3002") +
                  $"\n\n\u5f53\u524d\u6218\u529b\u8bc4\u7ea7  {CampaignProgressService.GetPowerRating()}"
                : "POST-MISSION NOTES\n\n" +
                  (result.Victory ? "Enemy command layer destabilized. Recover, reinforce, and redeploy to the next sector." : "Combat telemetry flagged weak windows in route execution. Upgrade loadout and re-enter the theater.") +
                  $"\n\nCURRENT POWER RATING  {CampaignProgressService.GetPowerRating()}";

            SetButtonLabel(retryButton, LocalizationService.Text("RETRY SORTIE", "\u91cd\u65b0\u51fa\u51fb"));
            SetButtonLabel(nextButton, LocalizationService.Text("NEXT CHAPTER", "\u4e0b\u4e00\u7ae0\u8282"));
            SetButtonLabel(menuButton, LocalizationService.Text("RETURN TO HANGAR", "\u8fd4\u56de\u673a\u5e93"));
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(result.Victory);
            }
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

        private void Retry()
        {
            GameFlowController flow = FindFirstObjectByType<GameFlowController>();
            if (flow != null)
            {
                flow.RestartLevel();
                return;
            }

            SceneManager.LoadScene(CampaignRuntime.CurrentLevel != null ? CampaignRuntime.CurrentLevel.SceneName : "Level_01");
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

        private void NextChapter()
        {
            GameFlowController flow = FindFirstObjectByType<GameFlowController>();
            if (flow != null)
            {
                flow.LoadNextChapterOrReturn();
                return;
            }

            SceneManager.LoadScene("ChapterSelect");
        }
    }
}
