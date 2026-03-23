using ThunderFighter.Spawning;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderFighter.Core
{
    public class SceneRuntimeGuard : MonoBehaviour
    {
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            Fix2DCamera();
            EnsureAudioListener();
            EnsureSceneVisuals(SceneManager.GetActiveScene().name);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Fix2DCamera();
            EnsureAudioListener();
            EnsureSceneVisuals(scene.name);
        }

        private static void Fix2DCamera()
        {
            Camera cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                return;
            }

            Vector3 pos = cam.transform.position;
            cam.transform.position = new Vector3(pos.x, pos.y, -10f);
            cam.orthographic = true;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;
            cam.cullingMask = ~0;
        }

        private static void EnsureSceneVisuals(string sceneName)
        {
            if (CampaignCatalog.GetBySceneName(sceneName) == null)
            {
                return;
            }

            Camera cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            LevelBackgroundController.Ensure(cam);
            CameraShakeController.Ensure(cam);
        }

        private static void EnsureAudioListener()
        {
            if (Object.FindFirstObjectByType<AudioListener>() != null)
            {
                return;
            }

            Camera cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            if (cam != null && cam.GetComponent<AudioListener>() == null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }
        }
    }
}
