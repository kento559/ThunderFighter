using UnityEngine;

namespace ThunderFighter.Core
{
    public static class GameSettingsService
    {
        private const string MasterVolumeKey = "settings.masterVolume";
        private const string MusicVolumeKey = "settings.musicVolume";
        private const string SfxVolumeKey = "settings.sfxVolume";
        private const string FullscreenKey = "settings.fullscreen";
        private const string ResolutionIndexKey = "settings.resolutionIndex";

        private static readonly Vector2Int[] SupportedResolutions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440)
        };

        public static float MasterVolume => PlayerPrefs.GetFloat(MasterVolumeKey, 0.9f);
        public static float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, 0.72f);
        public static float SfxVolume => PlayerPrefs.GetFloat(SfxVolumeKey, 0.84f);
        public static bool Fullscreen => PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        public static int ResolutionIndex => Mathf.Clamp(PlayerPrefs.GetInt(ResolutionIndexKey, 2), 0, SupportedResolutions.Length - 1);
        public static Vector2Int CurrentResolution => SupportedResolutions[ResolutionIndex];

        public static void Initialize()
        {
            AudioListener.volume = MasterVolume;
            ApplyDisplaySettings();
        }

        public static void SetMasterVolume(float value)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            AudioListener.volume = MasterVolume;
        }

        public static void SetMusicVolume(float value)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }

        public static void SetSfxVolume(float value)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }

        public static void SetFullscreen(bool value)
        {
            PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyDisplaySettings();
        }

        public static void CycleResolution(int step)
        {
            int next = ResolutionIndex + step;
            if (next < 0)
            {
                next = SupportedResolutions.Length - 1;
            }
            else if (next >= SupportedResolutions.Length)
            {
                next = 0;
            }

            PlayerPrefs.SetInt(ResolutionIndexKey, next);
            PlayerPrefs.Save();
            ApplyDisplaySettings();
        }

        public static string GetResolutionLabel()
        {
            Vector2Int resolution = CurrentResolution;
            return resolution.x + "x" + resolution.y;
        }

        public static string GetPercentLabel(float value)
        {
            return Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
        }

        public static void ApplyDisplaySettings()
        {
            Vector2Int resolution = CurrentResolution;
            Screen.fullScreen = Fullscreen;
            Screen.SetResolution(resolution.x, resolution.y, Fullscreen);
        }
    }
}
