using UnityEngine;

namespace ThunderFighter.Spawning
{
    public class ScrollingBackground : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Vector2 scrollSpeed = new Vector2(0f, -0.08f);

        private Vector2 offset;

        private void Update()
        {
            if (targetRenderer == null)
            {
                return;
            }

            offset += scrollSpeed * Time.deltaTime;
            targetRenderer.material.mainTextureOffset = offset;
        }
    }
}
