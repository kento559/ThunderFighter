using ThunderFighter.Audio;
using ThunderFighter.Combat;
using UnityEngine;

namespace ThunderFighter.Core
{
    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureCoreSystems()
        {
            GameSettingsService.Initialize();

            if (Object.FindFirstObjectByType<GameFlowController>() == null)
            {
                GameObject go = new GameObject("[Core] GameFlowController");
                go.AddComponent<GameFlowController>();
            }

            if (Object.FindFirstObjectByType<ScoreManager>() == null)
            {
                GameObject go = new GameObject("[Core] ScoreManager");
                go.AddComponent<ScoreManager>();
            }

            if (Object.FindFirstObjectByType<ProjectilePool>() == null)
            {
                GameObject go = new GameObject("[Core] ProjectilePool");
                go.AddComponent<ProjectilePool>();
            }

            if (Object.FindFirstObjectByType<SceneRuntimeGuard>() == null)
            {
                GameObject go = new GameObject("[Core] SceneRuntimeGuard");
                go.AddComponent<SceneRuntimeGuard>();
                Object.DontDestroyOnLoad(go);
            }

            if (Object.FindFirstObjectByType<AudioManager>() == null)
            {
                GameObject go = new GameObject("[Core] AudioManager");
                go.AddComponent<AudioManager>();
            }
        }
    }
}
