using ThunderFighter.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderFighter.Core
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string chapterSelectScene = "ChapterSelect";
        [SerializeField] private string resultScene = "Result";

        public GameState CurrentState { get; private set; } = GameState.Boot;

        private static GameFlowController instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDied += HandlePlayerDied;
            GameEvents.OnBossDied += HandleBossDied;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied -= HandlePlayerDied;
            GameEvents.OnBossDied -= HandleBossDied;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            ApplyStateForScene(SceneManager.GetActiveScene().name);
        }

        public void StartGame()
        {
            OpenChapterSelect();
        }

        public void OpenChapterSelect()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(chapterSelectScene);
            SetState(GameState.MainMenu);
        }

        public void StartLevel(LevelDefinition level)
        {
            if (level == null)
            {
                return;
            }

            CampaignRuntime.CurrentLevel = level;
            CampaignRuntime.CurrentLoadout = new PlayerRuntimeLoadout(CampaignProgressService.GetSelectedShipId());
            ScoreManager.Instance?.ResetScore();
            Time.timeScale = 1f;
            SceneManager.LoadScene(level.SceneName);
            SetState(GameState.Playing);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
            SetState(GameState.MainMenu);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            ScoreManager.Instance?.ResetScore();
            LevelDefinition level = CampaignRuntime.CurrentLevel ?? CampaignCatalog.GetBySceneName(SceneManager.GetActiveScene().name) ?? CampaignCatalog.GetByChapterIndex(1);
            if (level == null)
            {
                SceneManager.LoadScene("Level_01");
                SetState(GameState.Playing);
                return;
            }

            StartLevel(level);
        }

        public void LoadNextChapterOrReturn()
        {
            if (CampaignRuntime.CurrentLevel == null)
            {
                OpenChapterSelect();
                return;
            }

            LevelDefinition next = CampaignCatalog.GetByChapterIndex(CampaignRuntime.CurrentLevel.ChapterIndex + 1);
            if (next != null && CampaignProgressService.IsChapterUnlocked(next.ChapterIndex))
            {
                StartLevel(next);
                return;
            }

            OpenChapterSelect();
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetState(GameState.Paused);
                return;
            }

            if (CurrentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(GameState.Playing);
            }
        }

        private void HandlePlayerDied()
        {
            FinishRun(false);
        }

        private void HandleBossDied()
        {
            FinishRun(true);
        }

        private void FinishRun(bool victory)
        {
            Time.timeScale = 1f;
            LevelDefinition level = CampaignRuntime.CurrentLevel ?? CampaignCatalog.GetBySceneName(SceneManager.GetActiveScene().name);
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
            CampaignProgressService.RecordRun(level, score, victory);
            CampaignRuntime.CurrentLoadout = null;
            SetState(victory ? GameState.Victory : GameState.GameOver);
            SceneManager.LoadScene(resultScene);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LevelDefinition level = CampaignCatalog.GetBySceneName(scene.name);
            if (level != null)
            {
                CampaignRuntime.CurrentLevel = level;
            }

            ApplyStateForScene(scene.name);
            AudioManager audio = FindFirstObjectByType<AudioManager>();
            if (audio != null)
            {
                if (scene.name == mainMenuScene || scene.name == chapterSelectScene || scene.name == resultScene)
                {
                    audio.PlayMenuBgm();
                }
                else if (level != null)
                {
                    audio.PlayBattleBgm();
                }
            }
        }

        private void ApplyStateForScene(string sceneName)
        {
            if (sceneName == mainMenuScene || sceneName == chapterSelectScene)
            {
                SetState(GameState.MainMenu);
                return;
            }

            if (sceneName == resultScene)
            {
                if (CurrentState != GameState.GameOver && CurrentState != GameState.Victory)
                {
                    SetState(GameState.GameOver);
                }
                return;
            }

            if (CampaignCatalog.GetBySceneName(sceneName) != null)
            {
                SetState(GameState.Playing);
                return;
            }

            SetState(GameState.MainMenu);
        }

        private void SetState(GameState state)
        {
            CurrentState = state;
            GameEvents.RaiseGameStateChanged(CurrentState);
        }
    }
}
