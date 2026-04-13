using UnityEngine;

namespace ThunderFighter.Core
{
    public class CameraShakeController : MonoBehaviour
    {
        private Camera targetCamera;
        private Vector3 basePosition;
        private float shakeTime;
        private float shakeDuration;
        private float shakeMagnitude;

        public static CameraShakeController Ensure(Camera camera)
        {
            if (camera == null)
            {
                return null;
            }

            CameraShakeController controller = camera.GetComponent<CameraShakeController>();
            if (controller == null)
            {
                controller = camera.gameObject.AddComponent<CameraShakeController>();
            }

            return controller;
        }

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            basePosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            if (shakeTime > 0f)
            {
                shakeTime -= Time.deltaTime;
                float progress = 1f - Mathf.Clamp01(shakeTime / shakeDuration);
                float damper = 1f - progress;
                Vector2 offset = Random.insideUnitCircle * (shakeMagnitude * damper);
                transform.localPosition = new Vector3(basePosition.x + offset.x, basePosition.y + offset.y, basePosition.z);
                return;
            }

            transform.localPosition = basePosition;
        }

        public void Shake(float magnitude, float duration)
        {
            shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
            shakeDuration = Mathf.Max(duration, 0.05f);
            shakeTime = shakeDuration;
        }
    }
}
