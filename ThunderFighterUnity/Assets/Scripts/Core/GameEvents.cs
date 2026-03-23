using System;

namespace ThunderFighter.Core
{
    public static class GameEvents
    {
        public static event Action<int> OnScoreChanged;
        public static event Action<int, int> OnPlayerHpChanged;
        public static event Action<int, int> OnPlayerDamaged;
        public static event Action<int, float> OnComboChanged;
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<float> OnBossHpChanged;
        public static event Action<string> OnCombatAnnouncement;
        public static event Action<float, string> OnSkillEnergyChanged;
        public static event Action<float, bool> OnThreatEdgePulse;
        public static event Action<float, float> OnBossLockOnWarning;
        public static event Action<float> OnBossLaserHit;
        public static event Action<string, int> OnLoadoutChanged;
        public static event Action<string, PickupBuffType[]> OnBuffStatusChanged;
        public static event Action<string> OnPickupCollected;
        public static event Action OnPlayerDied;
        public static event Action OnBossDied;

        public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);
        public static void RaisePlayerHpChanged(int currentHp, int maxHp) => OnPlayerHpChanged?.Invoke(currentHp, maxHp);
        public static void RaisePlayerDamaged(int currentHp, int maxHp) => OnPlayerDamaged?.Invoke(currentHp, maxHp);
        public static void RaiseComboChanged(int comboCount, float multiplier) => OnComboChanged?.Invoke(comboCount, multiplier);
        public static void RaiseGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
        public static void RaiseBossHpChanged(float ratio) => OnBossHpChanged?.Invoke(ratio);
        public static void RaiseCombatAnnouncement(string message) => OnCombatAnnouncement?.Invoke(message);
        public static void RaiseSkillEnergyChanged(float normalized, string label) => OnSkillEnergyChanged?.Invoke(normalized, label);
        public static void RaiseThreatEdgePulse(float duration, bool cyan) => OnThreatEdgePulse?.Invoke(duration, cyan);
        public static void RaiseBossLockOnWarning(float viewportX, float duration) => OnBossLockOnWarning?.Invoke(viewportX, duration);
        public static void RaiseBossLaserHit(float intensity) => OnBossLaserHit?.Invoke(intensity);
        public static void RaiseLoadoutChanged(string shipName, int weaponLevel) => OnLoadoutChanged?.Invoke(shipName, weaponLevel);
        public static void RaiseBuffStatusChanged(string summary, PickupBuffType[] activeTypes) => OnBuffStatusChanged?.Invoke(summary, activeTypes);
        public static void RaisePickupCollected(string label) => OnPickupCollected?.Invoke(label);
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaiseBossDied() => OnBossDied?.Invoke();
    }
}
