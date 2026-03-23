using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThunderFighter.Core
{
    public static class CampaignCatalog
    {
        private static List<LevelDefinition> cachedLevels;

        public static IReadOnlyList<LevelDefinition> GetLevels()
        {
            if (cachedLevels == null)
            {
                cachedLevels = Resources.LoadAll<LevelDefinition>("Campaign")
                    .OrderBy(level => level != null ? level.ChapterIndex : int.MaxValue)
                    .ToList();
            }

            return cachedLevels;
        }

        public static LevelDefinition GetBySceneName(string sceneName)
        {
            return GetLevels().FirstOrDefault(level => level != null && level.SceneName == sceneName);
        }

        public static LevelDefinition GetByChapterIndex(int chapterIndex)
        {
            return GetLevels().FirstOrDefault(level => level != null && level.ChapterIndex == chapterIndex);
        }

        public static int GetMaxChapterIndex()
        {
            return GetLevels().Count == 0 ? 1 : GetLevels().Max(level => level.ChapterIndex);
        }

        public static void ResetCache()
        {
            cachedLevels = null;
        }
    }
}
