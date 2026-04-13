using UnityEngine;
using UnityEngine.SceneManagement;
using ThunderFighter.Core;

namespace ThunderFighter.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;
        private static AudioManager instance;
        private static AudioClip menuBgm;
        private static AudioClip battleBgm;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            if (bgmSource == null)
            {
                bgmSource = GetComponent<AudioSource>();
                if (bgmSource == null)
                {
                    bgmSource = gameObject.AddComponent<AudioSource>();
                }
            }

            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
            EnsureClips();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void Update()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = 0.24f * GameSettingsService.MasterVolume * GameSettingsService.MusicVolume;
            }
        }

        public void PlayMenuBgm()
        {
            Play(menuBgm);
        }

        public void PlayBattleBgm()
        {
            Play(battleBgm);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu" || scene.name == "ChapterSelect" || scene.name == "Result")
            {
                PlayMenuBgm();
            }
            else
            {
                PlayBattleBgm();
            }
        }

        private void Play(AudioClip clip)
        {
            if (bgmSource == null || clip == null)
            {
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.volume = 0.24f * GameSettingsService.MasterVolume * GameSettingsService.MusicVolume;
            bgmSource.Play();
        }

        private static void EnsureClips()
        {
            if (menuBgm == null)
            {
                menuBgm = BuildLoopClip("menu-bgm", 160f, 240f, 6f, 0.05f);
            }

            if (battleBgm == null)
            {
                battleBgm = BuildLoopClip("battle-bgm", 110f, 360f, 6f, 0.06f);
            }
        }

        private static AudioClip BuildLoopClip(string clipName, float lowFrequency, float highFrequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float pulse = 0.6f + Mathf.Sin(t * Mathf.PI * 2f * 0.5f) * 0.4f;
                float low = Mathf.Sin(2f * Mathf.PI * lowFrequency * t) * 0.65f;
                float high = Mathf.Sin(2f * Mathf.PI * highFrequency * t) * 0.35f;
                data[i] = (low + high) * pulse * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
