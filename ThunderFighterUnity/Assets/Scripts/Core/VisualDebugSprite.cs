using UnityEngine;

namespace ThunderFighter.Core
{
    public static class VisualDebugSprite
    {
        public static void Ensure(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            EnsureSprite(renderer, GeneratedSpriteKind.Hull);
            if (renderer.color.a <= 0.01f)
            {
                renderer.color = Color.white;
            }
        }

        public static SpriteRenderer Ensure(GameObject owner, Color tint, int sortingOrder = 10, float minScale = 0.5f)
        {
            return Ensure(owner, tint, sortingOrder, minScale, GeneratedSpriteKind.Hull);
        }

        public static SpriteRenderer Ensure(GameObject owner, Color tint, int sortingOrder, float minScale, GeneratedSpriteKind spriteKind)
        {
            if (owner == null)
            {
                return null;
            }

            SpriteRenderer renderer = owner.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = owner.AddComponent<SpriteRenderer>();
            }

            EnsureSprite(renderer, spriteKind);
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;

            Vector3 s = owner.transform.localScale;
            if (Mathf.Abs(s.x) < 0.01f || Mathf.Abs(s.y) < 0.01f)
            {
                owner.transform.localScale = new Vector3(minScale, minScale, 1f);
            }

            return renderer;
        }

        private static void EnsureSprite(SpriteRenderer renderer, GeneratedSpriteKind spriteKind)
        {
            Sprite generatedSprite = GeneratedSpriteLibrary.Get(spriteKind);
            if (renderer.sprite == generatedSprite)
            {
                return;
            }

            renderer.sprite = generatedSprite;
        }
    }
}
