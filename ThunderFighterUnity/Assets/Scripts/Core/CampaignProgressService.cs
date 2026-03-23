using UnityEngine;

namespace ThunderFighter.Core
{
    public static class CampaignProgressService
    {
        private const string HighestUnlockedChapterKey = "campaign.highestUnlockedChapter";
        private const string TechPointsKey = "campaign.techPoints";
        private const string SelectedShipKey = "campaign.selectedShip";
        private const string LevelPrefix = "campaign.level.";
        private const string UpgradePrefix = "campaign.upgrade.";
        private const int MaxUpgradeLevel = 5;

        public static int HighestUnlockedChapter => Mathf.Max(1, PlayerPrefs.GetInt(HighestUnlockedChapterKey, 1));
        public static int TechPoints => Mathf.Max(0, PlayerPrefs.GetInt(TechPointsKey, 0));
        public static ShipId GetSelectedShipId()
        {
            return (ShipId)Mathf.Clamp(PlayerPrefs.GetInt(SelectedShipKey, (int)ShipId.Balanced), 0, 2);
        }

        public static void SetSelectedShipId(ShipId shipId)
        {
            PlayerPrefs.SetInt(SelectedShipKey, (int)shipId);
            PlayerPrefs.Save();
        }

        public static bool IsChapterUnlocked(int chapterIndex)
        {
            return chapterIndex <= HighestUnlockedChapter;
        }

        public static int GetHighScore(int chapterIndex)
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(LevelPrefix + chapterIndex + ".highScore", 0));
        }

        public static bool IsCompleted(int chapterIndex)
        {
            return PlayerPrefs.GetInt(LevelPrefix + chapterIndex + ".completed", 0) == 1;
        }

        public static int GetUpgradeLevel(UpgradeType type)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(UpgradePrefix + type, 0), 0, MaxUpgradeLevel);
        }

        public static int GetUpgradeCost(UpgradeType type)
        {
            int level = GetUpgradeLevel(type);
            return 3 + (level * 2);
        }

        public static bool CanUpgrade(UpgradeType type)
        {
            int level = GetUpgradeLevel(type);
            return level < MaxUpgradeLevel && TechPoints >= GetUpgradeCost(type);
        }

        public static bool TryUpgrade(UpgradeType type)
        {
            if (!CanUpgrade(type))
            {
                return false;
            }

            int cost = GetUpgradeCost(type);
            PlayerPrefs.SetInt(TechPointsKey, TechPoints - cost);
            PlayerPrefs.SetInt(UpgradePrefix + type, GetUpgradeLevel(type) + 1);
            PlayerPrefs.Save();
            return true;
        }

        public static int GetPlayerMaxHp()
        {
            return 100 + (GetUpgradeLevel(UpgradeType.Armor) * 20);
        }

        public static int GetFirepowerBonus()
        {
            return GetUpgradeLevel(UpgradeType.Firepower);
        }

        public static float GetFireRateMultiplier()
        {
            return Mathf.Clamp(1f - GetUpgradeLevel(UpgradeType.Firepower) * 0.08f, 0.68f, 1f);
        }

        public static float GetSkillRegenMultiplier()
        {
            return 1f + GetUpgradeLevel(UpgradeType.Reactor) * 0.18f;
        }

        public static int GetPowerRating()
        {
            return 100 + GetUpgradeLevel(UpgradeType.Firepower) * 18 + GetUpgradeLevel(UpgradeType.Armor) * 20 + GetUpgradeLevel(UpgradeType.Reactor) * 16;
        }

        public static CampaignRunResult RecordRun(LevelDefinition level, int score, bool victory)
        {
            int chapterIndex = level != null ? level.ChapterIndex : 1;
            int highScore = Mathf.Max(GetHighScore(chapterIndex), score);
            PlayerPrefs.SetInt(LevelPrefix + chapterIndex + ".highScore", highScore);

            int earnedTechPoints = Mathf.Max(victory ? 3 : 1, score / (victory ? 1200 : 2000));
            PlayerPrefs.SetInt(TechPointsKey, TechPoints + earnedTechPoints);

            bool unlocked = false;
            string unlockedChapterTitle = string.Empty;
            if (victory)
            {
                PlayerPrefs.SetInt(LevelPrefix + chapterIndex + ".completed", 1);
                int nextChapter = chapterIndex + 1;
                if (nextChapter > HighestUnlockedChapter && nextChapter <= CampaignCatalog.GetMaxChapterIndex())
                {
                    PlayerPrefs.SetInt(HighestUnlockedChapterKey, nextChapter);
                    unlocked = true;
                    LevelDefinition unlockedLevel = CampaignCatalog.GetByChapterIndex(nextChapter);
                    unlockedChapterTitle = unlockedLevel != null ? unlockedLevel.ChapterTitle : $"Chapter {nextChapter}";
                }
            }

            PlayerPrefs.Save();

            CampaignRunResult result = new CampaignRunResult
            {
                ChapterTitle = level != null ? level.ChapterTitle : $"Chapter {chapterIndex}",
                Subtitle = level != null ? level.Subtitle : string.Empty,
                Score = score,
                Victory = victory,
                Rating = CalculateRating(score, victory),
                EarnedTechPoints = earnedTechPoints,
                TotalTechPoints = TechPoints,
                UnlockedNextChapter = unlocked,
                UnlockedChapterTitle = unlockedChapterTitle
            };
            CampaignRuntime.LastResult = result;
            return result;
        }

        private static string CalculateRating(int score, bool victory)
        {
            if (!victory)
            {
                return score >= 2500 ? "C" : "D";
            }

            if (score >= 9000)
            {
                return "S";
            }

            if (score >= 6500)
            {
                return "A";
            }

            if (score >= 4200)
            {
                return "B";
            }

            return "C";
        }
    }
}


