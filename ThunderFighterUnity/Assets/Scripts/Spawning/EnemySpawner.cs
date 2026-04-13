using System.Collections;
using System.Collections.Generic;
using ThunderFighter.Config;
using ThunderFighter.Core;
using ThunderFighter.Enemy;
using UnityEngine;

namespace ThunderFighter.Spawning
{
    public class EnemySpawner : MonoBehaviour
    {
        private class SpawnedEnemyTracker : MonoBehaviour
        {
            public System.Action OnRemoved;
            private bool notified;

            private void OnDestroy()
            {
                if (notified)
                {
                    return;
                }

                notified = true;
                OnRemoved?.Invoke();
            }
        }

        [SerializeField] private WaveConfig waveConfig;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField] private string overrideStartAnnouncement;
        [SerializeField] private string overrideMidAnnouncement;
        [SerializeField] private string overrideBossAnnouncement;

        [Header("Fallback (when wave config missing)")]
        [SerializeField] private GameObject fallbackEnemyPrefab;
        [SerializeField] private float fallbackSpawnInterval = 1.2f;

        private static EnemySpawner activeSpawner;
        private bool started;
        private int activeWaveIndex;
        private int activeWaveEnemyTotal;
        private int activeWaveEnemyRemaining;
        private int totalPhaseCount = 1;
        private readonly List<GameObject> chapterEnemyPrefabs = new List<GameObject>();

        private void Awake()
        {
            activeSpawner = this;
        }

        private void Start()
        {
            TryAdoptLevelDefinition();
            CacheWavePrefabs();
            TryStartSpawning();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleStateChanged;
            if (activeSpawner == this)
            {
                activeSpawner = null;
            }
        }

        public static void SpawnBossSummonWing(Vector3 aroundPosition, int chapterIndex, bool includeSupport)
        {
            if (activeSpawner == null)
            {
                return;
            }

            activeSpawner.SpawnBossSummonWingInternal(aroundPosition, chapterIndex, includeSupport);
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                TryAdoptLevelDefinition();
                CacheWavePrefabs();
                TryStartSpawning();
            }
        }

        private void TryAdoptLevelDefinition()
        {
            LevelDefinition level = CampaignRuntime.CurrentLevel ?? CampaignCatalog.GetBySceneName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            if (level == null)
            {
                return;
            }

            waveConfig = level.WaveConfig != null ? level.WaveConfig : waveConfig;
            bossPrefab = level.BossPrefab != null ? level.BossPrefab : bossPrefab;
            if (!string.IsNullOrWhiteSpace(level.StartAnnouncement))
            {
                overrideStartAnnouncement = level.StartAnnouncement;
            }
            if (!string.IsNullOrWhiteSpace(level.MidAnnouncement))
            {
                overrideMidAnnouncement = level.MidAnnouncement;
            }
            if (!string.IsNullOrWhiteSpace(level.BossAnnouncement))
            {
                overrideBossAnnouncement = level.BossAnnouncement;
            }
        }

        private void CacheWavePrefabs()
        {
            chapterEnemyPrefabs.Clear();
            if (waveConfig == null || waveConfig.Waves == null)
            {
                return;
            }

            for (int i = 0; i < waveConfig.Waves.Count; i++)
            {
                GameObject prefab = waveConfig.Waves[i] != null ? waveConfig.Waves[i].enemyPrefab : null;
                if (prefab != null && !chapterEnemyPrefabs.Contains(prefab))
                {
                    chapterEnemyPrefabs.Add(prefab);
                }
            }
        }

        private void TryStartSpawning()
        {
            if (started)
            {
                return;
            }

            started = true;
            StartCoroutine(RunSpawning());
        }

        private IEnumerator RunSpawning()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                yield break;
            }

            if (waveConfig != null && waveConfig.Waves != null && waveConfig.Waves.Count > 0)
            {
                totalPhaseCount = waveConfig.Waves.Count + 1;
                int midWaveIndex = Mathf.Max(0, waveConfig.Waves.Count / 2);
                int chapterIndex = Mathf.Clamp(CampaignRuntime.CurrentLevel != null ? CampaignRuntime.CurrentLevel.ChapterIndex : 1, 1, 3);
                for (int waveIndex = 0; waveIndex < waveConfig.Waves.Count; waveIndex++)
                {
                    WaveEntry wave = waveConfig.Waves[waveIndex];
                    activeWaveIndex = waveIndex + 1;
                    activeWaveEnemyTotal = Mathf.Max(1, wave.count);
                    if (ShouldSpawnCoordinatedEscort(chapterIndex, waveIndex, wave))
                    {
                        activeWaveEnemyTotal += 2;
                    }
                    activeWaveEnemyRemaining = activeWaveEnemyTotal;
                    GameEvents.RaiseTacticalProgressChanged(
                        LocalizationService.Text("Clear incoming hostile wave", "清除当前敌方波次"),
                        activeWaveIndex,
                        totalPhaseCount,
                        0f,
                        activeWaveEnemyRemaining,
                        activeWaveEnemyTotal);
                    yield return new WaitForSeconds(wave.startDelay);

                    if (waveIndex == 0)
                    {
                        GameEvents.RaiseCombatAnnouncement(string.IsNullOrWhiteSpace(overrideStartAnnouncement) ? GetWaveAnnouncement(wave) : overrideStartAnnouncement);
                    }
                    else if (waveIndex == midWaveIndex && !string.IsNullOrWhiteSpace(overrideMidAnnouncement))
                    {
                        GameEvents.RaiseCombatAnnouncement(overrideMidAnnouncement);
                    }
                    else
                    {
                        GameEvents.RaiseCombatAnnouncement(GetWaveAnnouncement(wave));
                    }

                    for (int i = 0; i < wave.count; i++)
                    {
                        SpawnEnemy(wave.enemyPrefab, true, EnemyBehaviorType.Auto, false, Vector3.zero);
                        yield return new WaitForSeconds(wave.spawnInterval);
                    }

                    if (ShouldSpawnCoordinatedEscort(chapterIndex, waveIndex, wave))
                    {
                        SpawnCoordinatedEscort(chapterIndex, wave.enemyPrefab);
                    }
                }

                GameEvents.RaiseTacticalProgressChanged(
                    LocalizationService.Text("Prepare for boss engagement", "准备进入 Boss 交战"),
                    totalPhaseCount,
                    totalPhaseCount,
                    1f,
                    0,
                    0);
                GameEvents.RaiseCombatAnnouncement(string.IsNullOrWhiteSpace(overrideBossAnnouncement) ? "WARNING: BOSS APPROACH" : overrideBossAnnouncement);
                yield return new WaitForSeconds(waveConfig.BossSpawnDelayAfterLastWave);
                SpawnBoss();
                yield break;
            }

            while (true)
            {
                SpawnEnemy(fallbackEnemyPrefab, false, EnemyBehaviorType.Auto, false, Vector3.zero);
                yield return new WaitForSeconds(Mathf.Max(0.2f, fallbackSpawnInterval));
            }
        }

        private void SpawnEnemy(GameObject enemyPrefab, bool trackWaveProgress, EnemyBehaviorType forcedBehavior, bool forceElite, Vector3 positionOffset)
        {
            if (enemyPrefab == null)
            {
                return;
            }

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 spawnPosition = point.position + positionOffset;
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, point.rotation);
            if (enemy != null)
            {
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller != null && forcedBehavior != EnemyBehaviorType.Auto)
                {
                    controller.ForceRuntimeBehavior(forcedBehavior, forceElite);
                }
            }

            if (!trackWaveProgress || enemy == null)
            {
                return;
            }

            SpawnedEnemyTracker tracker = enemy.GetComponent<SpawnedEnemyTracker>();
            if (tracker == null)
            {
                tracker = enemy.AddComponent<SpawnedEnemyTracker>();
            }

            tracker.OnRemoved += HandleTrackedEnemyRemoved;
            PublishWaveProgress();
        }

        private void SpawnEnemyAtPosition(GameObject enemyPrefab, Vector3 position, EnemyBehaviorType forcedBehavior, bool forceElite)
        {
            if (enemyPrefab == null)
            {
                return;
            }

            GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            if (enemy == null)
            {
                return;
            }

            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null && forcedBehavior != EnemyBehaviorType.Auto)
            {
                controller.ForceRuntimeBehavior(forcedBehavior, forceElite);
            }
        }

        private void SpawnBoss()
        {
            if (bossPrefab == null || bossSpawnPoint == null)
            {
                return;
            }

            Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        }

        private void SpawnBossSummonWingInternal(Vector3 aroundPosition, int chapterIndex, bool includeSupport)
        {
            GameObject prefab = GetPreferredSummonPrefab();
            if (prefab == null)
            {
                return;
            }

            int count = chapterIndex >= 3 ? 3 : 2;
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3((i - (count - 1) * 0.5f) * 1.15f, 0.55f + i * 0.12f, 0f);
                EnemyBehaviorType behavior = chapterIndex == 1 ? EnemyBehaviorType.Strafe : (i % 2 == 0 ? EnemyBehaviorType.Flank : EnemyBehaviorType.Dive);
                bool forceSupport = includeSupport && i == count - 1;
                SpawnEnemyAtPosition(prefab, aroundPosition + offset, forceSupport ? EnemyBehaviorType.Support : behavior, true);
            }

            GameEvents.RaiseCombatAnnouncement(LocalizationService.Text(
                includeSupport ? "Boss support wing inbound" : "Boss interceptors inbound",
                includeSupport ? "Boss 支援编队已进入空域" : "Boss 拦截编队进入战场"));
        }

        private GameObject GetPreferredSummonPrefab()
        {
            if (chapterEnemyPrefabs.Count > 0)
            {
                return chapterEnemyPrefabs[Mathf.Max(0, chapterEnemyPrefabs.Count - 1)];
            }

            return fallbackEnemyPrefab;
        }

        private bool ShouldSpawnCoordinatedEscort(int chapterIndex, int waveIndex, WaveEntry wave)
        {
            if (wave == null || wave.enemyPrefab == null)
            {
                return false;
            }

            if (chapterIndex == 1)
            {
                return false;
            }

            return waveIndex % 2 == 1 || wave.count >= 7;
        }

        private void SpawnCoordinatedEscort(int chapterIndex, GameObject basePrefab)
        {
            if (basePrefab == null)
            {
                return;
            }

            if (chapterIndex == 2)
            {
                SpawnEnemy(basePrefab, true, EnemyBehaviorType.Flank, true, new Vector3(-0.8f, 0.4f, 0f));
                SpawnEnemy(basePrefab, true, EnemyBehaviorType.Support, true, new Vector3(0.9f, 0.85f, 0f));
                GameEvents.RaiseCombatAnnouncement(LocalizationService.Text("Flankers supported by command craft", "包抄机与支援机协同压进"));
                return;
            }

            SpawnEnemy(basePrefab, true, EnemyBehaviorType.Dive, true, new Vector3(-1f, 0.55f, 0f));
            SpawnEnemy(basePrefab, true, EnemyBehaviorType.Support, true, new Vector3(1f, 0.9f, 0f));
            GameEvents.RaiseCombatAnnouncement(LocalizationService.Text("Dive strike escorted by support craft", "俯冲突击与支援机协同来袭"));
        }

        private static string GetWaveAnnouncement(WaveEntry wave)
        {
            if (wave == null || wave.enemyPrefab == null)
            {
                return "ENEMIES INBOUND";
            }

            return wave.count >= 10 ? "HOSTILE SWARM" : "INTERCEPTORS INBOUND";
        }

        private void HandleTrackedEnemyRemoved()
        {
            if (activeWaveEnemyRemaining <= 0)
            {
                return;
            }

            activeWaveEnemyRemaining = Mathf.Max(0, activeWaveEnemyRemaining - 1);
            PublishWaveProgress();
        }

        private void PublishWaveProgress()
        {
            if (activeWaveEnemyTotal <= 0)
            {
                return;
            }

            float progress = 1f - ((float)activeWaveEnemyRemaining / Mathf.Max(1, activeWaveEnemyTotal));
            GameEvents.RaiseTacticalProgressChanged(
                LocalizationService.Text("Clear incoming hostile wave", "清除当前敌方波次"),
                Mathf.Max(1, activeWaveIndex),
                Mathf.Max(1, totalPhaseCount),
                progress,
                activeWaveEnemyRemaining,
                activeWaveEnemyTotal);
        }
    }
}
