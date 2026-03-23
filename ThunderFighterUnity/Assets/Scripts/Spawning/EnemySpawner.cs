using System.Collections;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Spawning
{
    public class EnemySpawner : MonoBehaviour
    {
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

        private bool started;

        private void Start()
        {
            TryAdoptLevelDefinition();
            TryStartSpawning();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                TryAdoptLevelDefinition();
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
                int midWaveIndex = Mathf.Max(0, waveConfig.Waves.Count / 2);
                for (int waveIndex = 0; waveIndex < waveConfig.Waves.Count; waveIndex++)
                {
                    WaveEntry wave = waveConfig.Waves[waveIndex];
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
                        SpawnEnemy(wave.enemyPrefab);
                        yield return new WaitForSeconds(wave.spawnInterval);
                    }
                }

                GameEvents.RaiseCombatAnnouncement(string.IsNullOrWhiteSpace(overrideBossAnnouncement) ? "WARNING: BOSS APPROACH" : overrideBossAnnouncement);
                yield return new WaitForSeconds(waveConfig.BossSpawnDelayAfterLastWave);
                SpawnBoss();
                yield break;
            }

            while (true)
            {
                SpawnEnemy(fallbackEnemyPrefab);
                yield return new WaitForSeconds(Mathf.Max(0.2f, fallbackSpawnInterval));
            }
        }

        private void SpawnEnemy(GameObject enemyPrefab)
        {
            if (enemyPrefab == null)
            {
                return;
            }

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(enemyPrefab, point.position, point.rotation);
        }

        private void SpawnBoss()
        {
            if (bossPrefab == null || bossSpawnPoint == null)
            {
                return;
            }

            Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        }

        private static string GetWaveAnnouncement(WaveEntry wave)
        {
            if (wave == null || wave.enemyPrefab == null)
            {
                return "ENEMIES INBOUND";
            }

            return wave.count >= 10 ? "HOSTILE SWARM" : "INTERCEPTORS INBOUND";
        }
    }
}
