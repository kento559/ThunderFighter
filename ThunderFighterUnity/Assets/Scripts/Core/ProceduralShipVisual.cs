using UnityEngine;

namespace ThunderFighter.Core
{
    public enum ProceduralShipStyle
    {
        Player = 0,
        Enemy = 1,
        Boss = 2
    }

    public class ProceduralShipVisual : MonoBehaviour
    {
        [SerializeField] private ProceduralShipStyle style = ProceduralShipStyle.Player;
        [SerializeField] private int sortingOrder = 100;

        private const string RootName = "_ProceduralShipVisual";
        private const string MainMuzzleAnchorName = "_Anchor_MainMuzzle";
        private const string SupportMuzzleLeftAnchorName = "_Anchor_SupportMuzzleLeft";
        private const string SupportMuzzleRightAnchorName = "_Anchor_SupportMuzzleRight";

        private Transform visualRoot;
        private Transform thrusterLeft;
        private Transform thrusterRight;
        private Transform mainMuzzleAnchor;
        private Transform supportMuzzleLeftAnchor;
        private Transform supportMuzzleRightAnchor;
        private SpriteRenderer primaryRenderer;
        private ThrusterFlameAnimator[] flameAnimators;
        private float boostAmount;
        private RuntimeArtSpriteId currentSpriteId;
        private RuntimeArtSpriteId currentDamagedSpriteId;
        private RuntimeArtSpriteId[] currentFragmentIds;

        public ProceduralShipStyle Style => style;
        public RuntimeArtSpriteId CurrentSpriteId => currentSpriteId;
        public RuntimeArtSpriteId CurrentDamagedSpriteId => currentDamagedSpriteId;
        public RuntimeArtSpriteId[] CurrentFragmentIds => currentFragmentIds;

        public SpriteRenderer GetPrimaryRenderer()
        {
            return primaryRenderer;
        }

        public void SetVisualTilt(float zAngle)
        {
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localRotation = Quaternion.Euler(0f, 0f, zAngle);
        }

        public void SetThrustBoost(float normalizedBoost)
        {
            boostAmount = Mathf.Clamp01(normalizedBoost);
            if (flameAnimators == null)
            {
                return;
            }

            for (int i = 0; i < flameAnimators.Length; i++)
            {
                if (flameAnimators[i] != null)
                {
                    flameAnimators[i].SetBoost(boostAmount);
                }
            }
        }

        public void SetThrusterPalette(Color glowColor, Color coreColor)
        {
            if (flameAnimators == null)
            {
                return;
            }

            for (int i = 0; i < flameAnimators.Length; i++)
            {
                if (flameAnimators[i] != null)
                {
                    flameAnimators[i].SetPalette(glowColor, coreColor);
                }
            }
        }

        public void SetArtVariant(RuntimeArtSpriteId spriteId, RuntimeArtSpriteId damagedSpriteId, params RuntimeArtSpriteId[] fragmentIds)
        {
            currentSpriteId = spriteId;
            currentDamagedSpriteId = damagedSpriteId;
            currentFragmentIds = fragmentIds;

            if (primaryRenderer != null)
            {
                Sprite sprite = RuntimeArtLibrary.Get(spriteId);
                if (sprite != null)
                {
                    primaryRenderer.sprite = sprite;
                }
            }
        }

        public static ProceduralShipVisual Ensure(GameObject owner, ProceduralShipStyle targetStyle, int order)
        {
            ProceduralShipVisual visual = owner.GetComponent<ProceduralShipVisual>();
            if (visual == null)
            {
                visual = owner.AddComponent<ProceduralShipVisual>();
            }

            visual.style = targetStyle;
            visual.sortingOrder = order;
            visual.Rebuild();
            return visual;
        }

        public Transform GetMainMuzzleAnchor()
        {
            return mainMuzzleAnchor;
        }

        public Transform GetSupportLeftMuzzleAnchor()
        {
            return supportMuzzleLeftAnchor;
        }

        public Transform GetSupportRightMuzzleAnchor()
        {
            return supportMuzzleRightAnchor;
        }

        private void Awake()
        {
            Rebuild();
        }

        private void LateUpdate()
        {
            if (visualRoot == null)
            {
                Rebuild();
                return;
            }

            AnimateThrusters();
        }

        private void AnimateThrusters()
        {
            float pulse = 0.82f + Mathf.Sin(Time.time * 18f) * 0.18f;
            ApplyThrusterPulse(thrusterLeft, pulse);
            ApplyThrusterPulse(thrusterRight, pulse);
        }

        private static void ApplyThrusterPulse(Transform target, float pulse)
        {
            if (target == null)
            {
                return;
            }

            Vector3 baseScale = target.localScale;
            target.localScale = new Vector3(baseScale.x, Mathf.Max(0.05f, baseScale.y >= 0f ? pulse * 0.24f : -pulse * 0.24f), baseScale.z);
        }

        private void Rebuild()
        {
            Transform existing = transform.Find(RootName);
            if (existing != null)
            {
                visualRoot = existing;
            }
            else
            {
                GameObject go = new GameObject(RootName);
                go.transform.SetParent(transform, false);
                visualRoot = go.transform;
            }

            for (int i = visualRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(visualRoot.GetChild(i).gameObject);
            }

            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = GetRootScale(style);
            thrusterLeft = null;
            thrusterRight = null;
            mainMuzzleAnchor = null;
            supportMuzzleLeftAnchor = null;
            supportMuzzleRightAnchor = null;
            primaryRenderer = null;
            flameAnimators = null;
            currentFragmentIds = null;

            switch (style)
            {
                case ProceduralShipStyle.Player:
                    currentSpriteId = RuntimeArtSpriteId.PlayerShip;
                    currentDamagedSpriteId = RuntimeArtSpriteId.PlayerShipDamaged;
                    currentFragmentIds = new[] { RuntimeArtSpriteId.PlayerFragmentA, RuntimeArtSpriteId.PlayerFragmentB };
                    BuildPlayer();
                    break;
                case ProceduralShipStyle.Enemy:
                    currentSpriteId = RuntimeArtSpriteId.EnemyShip;
                    currentDamagedSpriteId = RuntimeArtSpriteId.EnemyShipDamaged;
                    currentFragmentIds = new[] { RuntimeArtSpriteId.EnemyFragmentA, RuntimeArtSpriteId.EnemyFragmentB };
                    BuildEnemy();
                    break;
                case ProceduralShipStyle.Boss:
                    currentSpriteId = RuntimeArtSpriteId.BossShip;
                    currentDamagedSpriteId = RuntimeArtSpriteId.BossShipDamaged;
                    currentFragmentIds = new[] { RuntimeArtSpriteId.BossFragmentA, RuntimeArtSpriteId.BossFragmentB };
                    BuildBoss();
                    break;
            }
        }

        private static Vector3 GetRootScale(ProceduralShipStyle targetStyle)
        {
            switch (targetStyle)
            {
                case ProceduralShipStyle.Player:
                    return new Vector3(1.88f, 1.88f, 1f);
                case ProceduralShipStyle.Enemy:
                    return new Vector3(1.82f, 1.82f, 1f);
                case ProceduralShipStyle.Boss:
                    return new Vector3(2.34f, 2.34f, 1f);
                default:
                    return Vector3.one;
            }
        }

        private void BuildPlayer()
        {
            if (TryBuildTexturedShip(currentSpriteId, new Vector2(0f, -0.03f), new Vector2(0.36f, 0.36f), sortingOrder + 2))
            {
                thrusterLeft = MakePart("ThrusterLeft", new Vector2(-0.2f, -1.1f), new Vector2(0.16f, 0.46f), new Color(0.92f, 0.82f, 0.28f, 0.95f), sortingOrder - 2);
                thrusterRight = MakePart("ThrusterRight", new Vector2(0.2f, -1.1f), new Vector2(0.16f, 0.46f), new Color(0.92f, 0.82f, 0.28f, 0.95f), sortingOrder - 2);
                BuildThrusterFlames();
                mainMuzzleAnchor = CreateAnchor(MainMuzzleAnchorName, new Vector2(0f, 1.2f));
                supportMuzzleLeftAnchor = CreateAnchor(SupportMuzzleLeftAnchorName, new Vector2(-0.56f, 0.1f));
                supportMuzzleRightAnchor = CreateAnchor(SupportMuzzleRightAnchorName, new Vector2(0.56f, 0.1f));
                return;
            }

            MakePart("CoreSpine", new Vector2(0f, 0.06f), new Vector2(0.22f, 1.5f), new Color(0.26f, 0.98f, 1f, 0.99f), sortingOrder + 2);
            thrusterLeft = MakePart("ThrusterLeft", new Vector2(-0.18f, -1.28f), new Vector2(0.14f, 0.46f), new Color(1f, 0.7f, 0.16f, 0.97f), sortingOrder - 2);
            thrusterRight = MakePart("ThrusterRight", new Vector2(0.18f, -1.28f), new Vector2(0.14f, 0.46f), new Color(1f, 0.7f, 0.16f, 0.97f), sortingOrder - 2);
            BuildThrusterFlames();
            mainMuzzleAnchor = CreateAnchor(MainMuzzleAnchorName, new Vector2(0f, 1.28f));
            supportMuzzleLeftAnchor = CreateAnchor(SupportMuzzleLeftAnchorName, new Vector2(-0.58f, 0.12f));
            supportMuzzleRightAnchor = CreateAnchor(SupportMuzzleRightAnchorName, new Vector2(0.58f, 0.12f));
        }

        private void BuildEnemy()
        {
            if (TryBuildTexturedShip(currentSpriteId, new Vector2(0f, 0f), new Vector2(0.32f, 0.32f), sortingOrder + 1))
            {
                thrusterLeft = MakePart("ThrusterLeft", new Vector2(-0.2f, 0.76f), new Vector2(0.14f, -0.28f), new Color(1f, 0.58f, 0.12f, 0.9f), sortingOrder - 2);
                thrusterRight = MakePart("ThrusterRight", new Vector2(0.2f, 0.76f), new Vector2(0.14f, -0.28f), new Color(1f, 0.58f, 0.12f, 0.9f), sortingOrder - 2);
                BuildThrusterFlames();
                return;
            }

            MakePart("Spine", new Vector2(0f, 0.04f), new Vector2(0.32f, 1.2f), new Color(1f, 0.22f, 0.22f, 0.98f), sortingOrder + 1);
            thrusterLeft = MakePart("ThrusterLeft", new Vector2(-0.2f, 0.86f), new Vector2(0.12f, -0.28f), new Color(1f, 0.58f, 0.12f, 0.9f), sortingOrder - 2);
            thrusterRight = MakePart("ThrusterRight", new Vector2(0.2f, 0.86f), new Vector2(0.12f, -0.28f), new Color(1f, 0.58f, 0.12f, 0.9f), sortingOrder - 2);
            BuildThrusterFlames();
        }

        private void BuildBoss()
        {
            if (TryBuildTexturedShip(currentSpriteId, new Vector2(0f, 0.04f), new Vector2(0.34f, 0.34f), sortingOrder + 1))
            {
                thrusterLeft = MakePart("ThrusterLeft", new Vector2(-0.28f, 0.98f), new Vector2(0.2f, -0.38f), new Color(1f, 0.55f, 0.12f, 0.92f), sortingOrder - 2);
                thrusterRight = MakePart("ThrusterRight", new Vector2(0.28f, 0.98f), new Vector2(0.2f, -0.38f), new Color(1f, 0.55f, 0.12f, 0.92f), sortingOrder - 2);
                BuildThrusterFlames();
                return;
            }

            MakePart("Spine", new Vector2(0f, 0.04f), new Vector2(0.46f, 1.5f), new Color(1f, 0.86f, 0.24f, 0.98f), sortingOrder + 1);
            thrusterLeft = MakePart("ThrusterLeft", new Vector2(-0.28f, 1f), new Vector2(0.16f, -0.34f), new Color(1f, 0.55f, 0.12f, 0.92f), sortingOrder - 2);
            thrusterRight = MakePart("ThrusterRight", new Vector2(0.28f, 1f), new Vector2(0.16f, -0.34f), new Color(1f, 0.55f, 0.12f, 0.92f), sortingOrder - 2);
            BuildThrusterFlames();
        }

        private Transform MakePart(string partName, Vector2 localPos, Vector2 localScale, Color color, int order, float rotationZ = 0f)
        {
            GameObject go = new GameObject(partName);
            go.transform.SetParent(visualRoot, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            go.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);
            go.layer = 0;

            SpriteRenderer renderer = VisualDebugSprite.Ensure(go, color, order, 1f, ResolveSpriteKind(partName));
            renderer.sortingOrder = order;
            renderer.color = color;
            return go.transform;
        }

        private bool TryBuildTexturedShip(RuntimeArtSpriteId spriteId, Vector2 localPosition, Vector2 localScale, int order)
        {
            Sprite sprite = RuntimeArtLibrary.Get(spriteId);
            if (sprite == null)
            {
                return false;
            }

            GameObject go = new GameObject(spriteId.ToString());
            go.transform.SetParent(visualRoot, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);
            go.layer = 0;

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            renderer.color = Color.white;
            primaryRenderer = renderer;
            return true;
        }

        private static GeneratedSpriteKind ResolveSpriteKind(string partName)
        {
            if (partName.Contains("Nose"))
            {
                return GeneratedSpriteKind.Nose;
            }

            if (partName.Contains("Cockpit") || partName.Contains("Bridge") || partName.Contains("Core"))
            {
                return GeneratedSpriteKind.Cockpit;
            }

            if (partName.Contains("Wing") || partName.Contains("Blade") || partName.Contains("Canard"))
            {
                return GeneratedSpriteKind.Wing;
            }

            if (partName.Contains("Engine"))
            {
                return GeneratedSpriteKind.Engine;
            }

            if (partName.Contains("Thruster"))
            {
                return GeneratedSpriteKind.Thruster;
            }

            return GeneratedSpriteKind.Hull;
        }

        private Transform CreateAnchor(string anchorName, Vector2 localPosition)
        {
            GameObject go = new GameObject(anchorName);
            go.transform.SetParent(visualRoot, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.layer = 0;
            return go.transform;
        }

        private void BuildThrusterFlames()
        {
            flameAnimators = new ThrusterFlameAnimator[2];
            flameAnimators[0] = AttachFlame(thrusterLeft, sortingOrder - 3);
            flameAnimators[1] = AttachFlame(thrusterRight, sortingOrder - 3);
            SetThrustBoost(boostAmount);
        }

        private ThrusterFlameAnimator AttachFlame(Transform thruster, int order)
        {
            if (thruster == null)
            {
                return null;
            }

            ThrusterFlameAnimator animator = thruster.gameObject.AddComponent<ThrusterFlameAnimator>();
            float directionSign = thruster.localScale.y >= 0f ? -1f : 1f;
            Color glow = style == ProceduralShipStyle.Player ? new Color(1f, 0.58f, 0.18f, 0.55f) : new Color(1f, 0.46f, 0.14f, 0.5f);
            Color core = style == ProceduralShipStyle.Player ? new Color(1f, 0.9f, 0.72f, 0.95f) : new Color(1f, 0.78f, 0.54f, 0.95f);
            animator.Setup(directionSign, order, glow, core);
            return animator;
        }
    }
}
