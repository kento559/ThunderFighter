namespace ThunderFighter.Core
{
    public enum UpgradeType
    {
        Firepower = 0,
        Armor = 1,
        Reactor = 2
    }

    public sealed class CampaignRunResult
    {
        public string ChapterTitle;
        public string Subtitle;
        public int Score;
        public bool Victory;
        public string Rating;
        public int EarnedTechPoints;
        public int TotalTechPoints;
        public bool UnlockedNextChapter;
        public string UnlockedChapterTitle;
    }

    public static class CampaignRuntime
    {
        public static LevelDefinition CurrentLevel { get; set; }
        public static CampaignRunResult LastResult { get; set; }
        public static PlayerRuntimeLoadout CurrentLoadout { get; set; }
    }
}
