using UnityEngine;

namespace ThunderFighter.Core
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private float comboWindowSeconds = 2f;
        [SerializeField] private float comboMultiplierStep = 0.2f;
        [SerializeField] private float comboMultiplierCap = 3f;

        public int Score { get; private set; }
        public int ComboCount { get; private set; }

        private float comboExpireAt;

        private static ScoreManager instance;
        public static ScoreManager Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            ResetScore();
        }

        private void Update()
        {
            if (ComboCount > 0 && Time.time >= comboExpireAt)
            {
                ComboCount = 0;
                GameEvents.RaiseComboChanged(ComboCount, 1f);
            }
        }

        public void ResetScore()
        {
            Score = 0;
            ComboCount = 0;
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseComboChanged(ComboCount, 1f);
        }

        public void AddKillScore(int baseScore)
        {
            ComboCount++;
            comboExpireAt = Time.time + comboWindowSeconds;

            float multiplier = Mathf.Min(1f + ComboCount * comboMultiplierStep, comboMultiplierCap);
            Score += Mathf.RoundToInt(baseScore * multiplier);
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseComboChanged(ComboCount, multiplier);
        }
    }
}
