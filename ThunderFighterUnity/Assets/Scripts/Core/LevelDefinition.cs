using ThunderFighter.Spawning;
using UnityEngine;

namespace ThunderFighter.Core
{
    public enum LevelTheme
    {
        Orbital = 0,
        AsteroidBelt = 1,
        DeepSpace = 2
    }

    [CreateAssetMenu(menuName = "ThunderFighter/Campaign/Level Definition", fileName = "LevelDefinition")]
    public class LevelDefinition : ScriptableObject
    {
        [field: SerializeField] public int ChapterIndex { get; private set; } = 1;
        [field: SerializeField] public string SceneName { get; private set; } = "Level_01";
        [field: SerializeField] public string ChapterTitle { get; private set; } = "Chapter 1";
        [field: SerializeField] public string Subtitle { get; private set; } = "Orbital Intercept";
        [field: SerializeField] public string ObjectiveText { get; private set; } = "Intercept hostile craft and destroy the command ship.";
        [field: SerializeField] public string StartAnnouncement { get; private set; } = "ORBITAL DEFENSE SCRAMBLE";
        [field: SerializeField] public string MidAnnouncement { get; private set; } = "HOSTILE PRESSURE RISING";
        [field: SerializeField] public string BossAnnouncement { get; private set; } = "WARNING: COMMAND SHIP APPROACH";
        [field: SerializeField] public LevelTheme Theme { get; private set; } = LevelTheme.Orbital;
        [field: SerializeField] public WaveConfig WaveConfig { get; private set; }
        [field: SerializeField] public GameObject BossPrefab { get; private set; }
        [field: SerializeField] public int RecommendedPower { get; private set; } = 100;
        [field: SerializeField] public string DifficultyLabel { get; private set; } = "NORMAL";
        [field: SerializeField] public Color AccentColor { get; private set; } = new Color(0.4f, 0.84f, 1f, 1f);
    }
}
