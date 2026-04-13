using System.Collections.Generic;
using ThunderFighter.Boss;
using ThunderFighter.Combat;
using ThunderFighter.Config;
using ThunderFighter.Core;
using ThunderFighter.Enemy;
using ThunderFighter.InputSystem;
using ThunderFighter.Player;
using ThunderFighter.Spawning;
using ThunderFighter.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThunderFighter.EditorSetup
{
    public static class ProjectAutoSetup
    {
        private const string Root = "Assets/Generated";
        private const string PrefabDir = Root + "/Prefabs";
        private const string ConfigDir = Root + "/Config";
        private const string SceneDir = "Assets/Scenes";

        [MenuItem("ThunderFighter/Auto Setup Project")]
        public static void RunFromMenu()
        {
            Run();
        }

        public static void Run()
        {
            EnsureFolders();

            var assets = CreateAssets();
            var prefabs = CreatePrefabs(assets);

            CreateMainMenuScene();
            CreateLevelScene(assets, prefabs);
            CreateResultScene();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ThunderFighter] Auto setup completed.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Generated");
            EnsureFolder(Root, "Prefabs");
            EnsureFolder(Root, "Config");
            EnsureFolder("Assets", "Scenes");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string full = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private sealed class AssetRefs
        {
            public PlayerConfig PlayerConfig;
            public EnemyConfig EnemyConfigBasic;
            public EnemyConfig EnemyConfigElite;
            public WeaponConfig PlayerWeaponConfig;
            public WeaponConfig EnemyWeaponConfig;
            public WaveConfig WaveConfig;
            public BossPhaseConfig[] BossPhases;
        }

        private sealed class PrefabRefs
        {
            public Projectile PlayerProjectile;
            public Projectile EnemyProjectile;
            public Projectile BossProjectile;
            public GameObject EnemyBasic;
            public GameObject EnemyElite;
            public GameObject Boss;
            public GameObject Player;
        }

        private static AssetRefs CreateAssets()
        {
            var refs = new AssetRefs
            {
                PlayerConfig = CreateAsset<PlayerConfig>(ConfigDir + "/PlayerConfig.asset"),
                EnemyConfigBasic = CreateAsset<EnemyConfig>(ConfigDir + "/EnemyConfig_Basic.asset"),
                EnemyConfigElite = CreateAsset<EnemyConfig>(ConfigDir + "/EnemyConfig_Elite.asset"),
                PlayerWeaponConfig = CreateAsset<WeaponConfig>(ConfigDir + "/WeaponConfig_Player.asset"),
                EnemyWeaponConfig = CreateAsset<WeaponConfig>(ConfigDir + "/WeaponConfig_Enemy.asset"),
                WaveConfig = CreateAsset<WaveConfig>(ConfigDir + "/WaveConfig_Main.asset"),
                BossPhases = new[]
                {
                    CreateAsset<BossPhaseConfig>(ConfigDir + "/BossPhase_01.asset"),
                    CreateAsset<BossPhaseConfig>(ConfigDir + "/BossPhase_02.asset"),
                    CreateAsset<BossPhaseConfig>(ConfigDir + "/BossPhase_03.asset")
                }
            };

            SetSerializedFloat(refs.EnemyConfigBasic, "<MoveSpeed>k__BackingField", 2.5f);
            SetSerializedInt(refs.EnemyConfigBasic, "<MaxHp>k__BackingField", 3);
            SetSerializedInt(refs.EnemyConfigBasic, "<ScoreValue>k__BackingField", 100);
            SetSerializedBool(refs.EnemyConfigBasic, "<CanShoot>k__BackingField", true);
            SetSerializedFloat(refs.EnemyConfigBasic, "<ShootInterval>k__BackingField", 1.4f);

            SetSerializedFloat(refs.EnemyConfigElite, "<MoveSpeed>k__BackingField", 3.4f);
            SetSerializedInt(refs.EnemyConfigElite, "<MaxHp>k__BackingField", 8);
            SetSerializedInt(refs.EnemyConfigElite, "<ScoreValue>k__BackingField", 350);
            SetSerializedBool(refs.EnemyConfigElite, "<CanShoot>k__BackingField", true);
            SetSerializedFloat(refs.EnemyConfigElite, "<ShootInterval>k__BackingField", 1.0f);

            SetSerializedFloat(refs.PlayerWeaponConfig, "<FireInterval>k__BackingField", 0.12f);
            SetSerializedInt(refs.PlayerWeaponConfig, "<ProjectileDamage>k__BackingField", 1);
            SetSerializedFloat(refs.PlayerWeaponConfig, "<ProjectileSpeed>k__BackingField", 14f);

            SetSerializedFloat(refs.EnemyWeaponConfig, "<FireInterval>k__BackingField", 0.75f);
            SetSerializedInt(refs.EnemyWeaponConfig, "<ProjectileDamage>k__BackingField", 1);
            SetSerializedFloat(refs.EnemyWeaponConfig, "<ProjectileSpeed>k__BackingField", 7.5f);

            SetSerializedFloat(refs.BossPhases[0], "<TriggerHpRatio>k__BackingField", 1.0f);
            SetSerializedFloat(refs.BossPhases[0], "<MoveSpeed>k__BackingField", 1.5f);
            SetSerializedFloat(refs.BossPhases[0], "<FireInterval>k__BackingField", 0.7f);
            SetSerializedInt(refs.BossPhases[0], "<BulletsPerShot>k__BackingField", 3);
            SetSerializedFloat(refs.BossPhases[0], "<SpreadAngle>k__BackingField", 18f);

            SetSerializedFloat(refs.BossPhases[1], "<TriggerHpRatio>k__BackingField", 0.7f);
            SetSerializedFloat(refs.BossPhases[1], "<MoveSpeed>k__BackingField", 2.2f);
            SetSerializedFloat(refs.BossPhases[1], "<FireInterval>k__BackingField", 0.5f);
            SetSerializedInt(refs.BossPhases[1], "<BulletsPerShot>k__BackingField", 5);
            SetSerializedFloat(refs.BossPhases[1], "<SpreadAngle>k__BackingField", 28f);

            SetSerializedFloat(refs.BossPhases[2], "<TriggerHpRatio>k__BackingField", 0.4f);
            SetSerializedFloat(refs.BossPhases[2], "<MoveSpeed>k__BackingField", 2.8f);
            SetSerializedFloat(refs.BossPhases[2], "<FireInterval>k__BackingField", 0.34f);
            SetSerializedInt(refs.BossPhases[2], "<BulletsPerShot>k__BackingField", 7);
            SetSerializedFloat(refs.BossPhases[2], "<SpreadAngle>k__BackingField", 34f);

            return refs;
        }

        private static PrefabRefs CreatePrefabs(AssetRefs assets)
        {
            var refs = new PrefabRefs
            {
                PlayerProjectile = CreateProjectilePrefab(PrefabDir + "/Projectile_Player.prefab", Color.cyan, 0.22f),
                EnemyProjectile = CreateProjectilePrefab(PrefabDir + "/Projectile_Enemy.prefab", new Color(1f, 0.3f, 0.3f, 1f), 0.24f),
                BossProjectile = CreateProjectilePrefab(PrefabDir + "/Projectile_Boss.prefab", new Color(1f, 0.6f, 0.2f, 1f), 0.32f)
            };

            refs.Player = CreatePlayerPrefab(assets, refs.PlayerProjectile);
            refs.EnemyBasic = CreateEnemyPrefab("Enemy_Basic", new Color(1f, 0.35f, 0.35f, 1f), assets.EnemyConfigBasic, assets.EnemyWeaponConfig, refs.EnemyProjectile, 0.75f);
            refs.EnemyElite = CreateEnemyPrefab("Enemy_Elite", new Color(1f, 0.6f, 0.2f, 1f), assets.EnemyConfigElite, assets.EnemyWeaponConfig, refs.EnemyProjectile, 1.1f);
            refs.Boss = CreateBossPrefab(assets.BossPhases, refs.BossProjectile);

            BuildWaveConfig(assets.WaveConfig, refs.EnemyBasic, refs.EnemyElite);
            return refs;
        }

        private static Projectile CreateProjectilePrefab(string path, Color color, float scale)
        {
            var root = new GameObject("Projectile");
            root.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = root.AddComponent<SpriteRenderer>();
            sr.color = color;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            root.AddComponent<Projectile>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path).GetComponent<Projectile>();
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreatePlayerPrefab(AssetRefs assets, Projectile projectilePrefab)
        {
            var root = new GameObject("Player");
            root.transform.position = new Vector3(0f, -3.7f, 0f);

            var sr = root.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.4f, 0.9f, 1f, 1f);

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            var health = root.AddComponent<HealthComponent>();
            SetSerializedEnum(health, "faction", (int)Faction.Player);
            SetSerializedInt(health, "maxHp", 8);
            SetSerializedBool(health, "notifyPlayerHpEvents", true);
            SetSerializedBool(health, "notifyDeathAsPlayer", true);

            var input = root.AddComponent<KeyboardMouseInputProvider>();

            var firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(root.transform);
            firePoint.localPosition = new Vector3(0f, 0.7f, 0f);

            var weapon = root.AddComponent<WeaponController>();
            SetSerializedObject(weapon, "weaponConfig", assets.PlayerWeaponConfig);
            SetSerializedObject(weapon, "projectilePrefab", projectilePrefab);
            SetSerializedEnum(weapon, "ownerFaction", (int)Faction.Player);
            SetSerializedArrayObject(weapon, "firePoints", new Object[] { firePoint });

            var player = root.AddComponent<PlayerController>();
            SetSerializedObject(player, "playerConfig", assets.PlayerConfig);
            SetSerializedObject(player, "inputProvider", input);
            SetSerializedObject(player, "weapon", weapon);

            string path = PrefabDir + "/Player.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateEnemyPrefab(string name, Color color, EnemyConfig config, WeaponConfig weaponConfig, Projectile projectile, float scale)
        {
            var root = new GameObject(name);
            root.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = root.AddComponent<SpriteRenderer>();
            sr.color = color;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            var health = root.AddComponent<HealthComponent>();
            SetSerializedEnum(health, "faction", (int)Faction.Enemy);
            SetSerializedInt(health, "maxHp", config == null ? 5 : ReadSerializedInt(config, "<MaxHp>k__BackingField"));

            var firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(root.transform);
            firePoint.localPosition = new Vector3(0f, -0.8f, 0f);
            firePoint.localRotation = Quaternion.Euler(0f, 0f, 180f);

            var weapon = root.AddComponent<WeaponController>();
            SetSerializedObject(weapon, "weaponConfig", weaponConfig);
            SetSerializedObject(weapon, "projectilePrefab", projectile);
            SetSerializedEnum(weapon, "ownerFaction", (int)Faction.Enemy);
            SetSerializedArrayObject(weapon, "firePoints", new Object[] { firePoint });

            var enemy = root.AddComponent<EnemyController>();
            SetSerializedObject(enemy, "config", config);
            SetSerializedObject(enemy, "weapon", weapon);

            var ram = root.AddComponent<RamDamage>();
            SetSerializedInt(ram, "collisionDamage", 1);
            SetSerializedEnum(ram, "faction", (int)Faction.Enemy);

            string path = PrefabDir + "/" + name + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateBossPrefab(BossPhaseConfig[] phases, Projectile projectilePrefab)
        {
            var root = new GameObject("Boss");
            root.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

            var sr = root.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.85f, 0.2f, 1f);

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            var health = root.AddComponent<HealthComponent>();
            SetSerializedEnum(health, "faction", (int)Faction.Enemy);
            SetSerializedInt(health, "maxHp", 220);
            SetSerializedBool(health, "notifyBossHpEvents", true);
            SetSerializedBool(health, "notifyDeathAsBoss", true);

            var firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(root.transform);
            firePoint.localPosition = new Vector3(0f, -0.8f, 0f);
            firePoint.localRotation = Quaternion.Euler(0f, 0f, 180f);

            var boss = root.AddComponent<BossController>();
            SetSerializedArrayObject(boss, "phases", phases);
            SetSerializedObject(boss, "projectilePrefab", projectilePrefab);
            SetSerializedObject(boss, "firePoint", firePoint);

            string path = PrefabDir + "/Boss.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BuildWaveConfig(WaveConfig waveConfig, GameObject enemyBasic, GameObject enemyElite)
        {
            var so = new SerializedObject(waveConfig);
            var list = so.FindProperty("<Waves>k__BackingField");
            list.ClearArray();

            AddWave(list, enemyBasic, 12, 0.6f, 0f);
            AddWave(list, enemyBasic, 14, 0.5f, 2f);
            AddWave(list, enemyElite, 8, 0.8f, 2.2f);
            AddWave(list, enemyBasic, 16, 0.45f, 1.8f);

            so.FindProperty("<BossSpawnDelayAfterLastWave>k__BackingField").floatValue = 3f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(waveConfig);
        }

        private static void AddWave(SerializedProperty list, GameObject prefab, int count, float interval, float delay)
        {
            int idx = list.arraySize;
            list.InsertArrayElementAtIndex(idx);
            var wave = list.GetArrayElementAtIndex(idx);
            wave.FindPropertyRelative("enemyPrefab").objectReferenceValue = prefab;
            wave.FindPropertyRelative("count").intValue = count;
            wave.FindPropertyRelative("spawnInterval").floatValue = interval;
            wave.FindPropertyRelative("startDelay").floatValue = delay;
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            CreateCamera(new Color(0.02f, 0.03f, 0.08f));
            var canvas = CreateCanvas();
            CreateEventSystem();

            var panel = CreateUIObject("Panel", canvas.transform).AddComponent<Image>();
            var panelRt = panel.rectTransform;
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.color = new Color(0f, 0f, 0f, 0.35f);

            var title = CreateText("Title", canvas.transform, "THUNDER FIGHTER", 58, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.78f), new Vector2(700, 100));

            var startBtn = CreateButton("StartButton", canvas.transform, "START");
            SetRect(startBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.52f), new Vector2(300, 72));

            var quitBtn = CreateButton("QuitButton", canvas.transform, "QUIT");
            SetRect(quitBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.4f), new Vector2(300, 72));

            var controllerObj = new GameObject("MainMenuUI");
            var controller = controllerObj.AddComponent<MainMenuController>();
            SetSerializedObject(controller, "startButton", startBtn.GetComponent<Button>());
            SetSerializedObject(controller, "quitButton", quitBtn.GetComponent<Button>());

            EditorSceneManager.SaveScene(scene, SceneDir + "/MainMenu.unity");
        }

        private static void CreateLevelScene(AssetRefs assets, PrefabRefs prefabs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level_01";

            CreateCamera(new Color(0.03f, 0.04f, 0.09f));

            var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bg.name = "BG_Starfield";
            bg.transform.position = new Vector3(0f, 0f, 10f);
            bg.transform.localScale = new Vector3(20f, 12f, 1f);
            var bgRenderer = bg.GetComponent<Renderer>();
            bgRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.05f, 0.08f, 0.14f, 1f) };
            Object.DestroyImmediate(bg.GetComponent<Collider>());
            var scroll = bg.AddComponent<ScrollingBackground>();
            SetSerializedObject(scroll, "targetRenderer", bgRenderer);

            PrefabUtility.InstantiatePrefab(prefabs.Player);

            var spawnerObj = new GameObject("Spawner");
            var spawner = spawnerObj.AddComponent<EnemySpawner>();
            SetSerializedObject(spawner, "waveConfig", assets.WaveConfig);
            SetSerializedObject(spawner, "bossPrefab", prefabs.Boss);

            Transform[] points = new Transform[5];
            float[] xs = { -6.5f, -3.2f, 0f, 3.2f, 6.5f };
            for (int i = 0; i < points.Length; i++)
            {
                var p = new GameObject("SpawnPoint_" + (i + 1)).transform;
                p.position = new Vector3(xs[i], 6f, 0f);
                p.SetParent(spawnerObj.transform);
                points[i] = p;
            }

            var bossPoint = new GameObject("BossSpawnPoint").transform;
            bossPoint.position = new Vector3(0f, 4.1f, 0f);
            bossPoint.SetParent(spawnerObj.transform);

            SetSerializedArrayObject(spawner, "spawnPoints", points);
            SetSerializedObject(spawner, "bossSpawnPoint", bossPoint);

            SetupProjectilePool(prefabs);
            CreateHUD();

            EditorSceneManager.SaveScene(scene, SceneDir + "/Level_01.unity");
        }

        private static void SetupProjectilePool(PrefabRefs prefabs)
        {
            var pool = Object.FindFirstObjectByType<ProjectilePool>();
            if (pool == null)
            {
                var go = new GameObject("[Core] ProjectilePool");
                pool = go.AddComponent<ProjectilePool>();
            }

            var so = new SerializedObject(pool);
            var entries = so.FindProperty("entries");
            entries.arraySize = 3;

            SetPoolEntry(entries.GetArrayElementAtIndex(0), prefabs.PlayerProjectile, 120, true);
            SetPoolEntry(entries.GetArrayElementAtIndex(1), prefabs.EnemyProjectile, 140, true);
            SetPoolEntry(entries.GetArrayElementAtIndex(2), prefabs.BossProjectile, 220, true);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pool);
        }

        private static void SetPoolEntry(SerializedProperty entry, Projectile prefab, int prewarm, bool expandable)
        {
            entry.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            entry.FindPropertyRelative("prewarmCount").intValue = prewarm;
            entry.FindPropertyRelative("expandable").boolValue = expandable;
        }

        private static void CreateResultScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Result";

            CreateCamera(new Color(0.04f, 0.03f, 0.05f));
            var canvas = CreateCanvas();
            CreateEventSystem();

            var title = CreateText("Title", canvas.transform, "MISSION COMPLETE", 56, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.75f), new Vector2(900f, 100f));

            var score = CreateText("Score", canvas.transform, "Final Score: 0", 36, TextAnchor.MiddleCenter);
            SetRect(score.rectTransform, new Vector2(0.5f, 0.6f), new Vector2(900f, 80f));

            var retryBtn = CreateButton("RetryButton", canvas.transform, "RETRY");
            SetRect(retryBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.44f), new Vector2(280, 70));

            var menuBtn = CreateButton("MenuButton", canvas.transform, "MAIN MENU");
            SetRect(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.33f), new Vector2(280, 70));

            var resultObj = new GameObject("ResultUI");
            var result = resultObj.AddComponent<ResultController>();
            SetSerializedObject(result, "titleText", title);
            SetSerializedObject(result, "scoreText", score);
            SetSerializedObject(result, "retryButton", retryBtn.GetComponent<Button>());
            SetSerializedObject(result, "menuButton", menuBtn.GetComponent<Button>());

            EditorSceneManager.SaveScene(scene, SceneDir + "/Result.unity");
        }

        private static void CreateHUD()
        {
            var canvas = CreateCanvas();

            var score = CreateText("ScoreText", canvas.transform, "Score: 0", 28, TextAnchor.MiddleLeft);
            score.color = Color.cyan;
            SetRect(score.rectTransform, new Vector2(0.11f, 0.95f), new Vector2(320, 48));

            var hp = CreateText("HpText", canvas.transform, "HP: 8/8", 28, TextAnchor.MiddleLeft);
            hp.color = new Color(0.6f, 1f, 0.85f, 1f);
            SetRect(hp.rectTransform, new Vector2(0.1f, 0.89f), new Vector2(320, 48));

            var sliderObj = CreateUIObject("BossHpSlider", canvas.transform);
            var sliderRt = sliderObj.GetComponent<RectTransform>();
            SetRect(sliderRt, new Vector2(0.5f, 0.95f), new Vector2(620, 24));
            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.direction = Slider.Direction.LeftToRight;

            var bg = CreateUIObject("Background", sliderObj.transform).AddComponent<Image>();
            bg.color = new Color(0.15f, 0.12f, 0.12f, 0.9f);
            Stretch(bg.rectTransform);

            var fillArea = CreateUIObject("Fill Area", sliderObj.transform);
            Stretch(fillArea.GetComponent<RectTransform>());
            var fill = CreateUIObject("Fill", fillArea.transform).AddComponent<Image>();
            fill.color = new Color(1f, 0.34f, 0.2f, 1f);
            Stretch(fill.rectTransform);

            slider.targetGraphic = fill;
            slider.fillRect = fill.rectTransform;

            var pausePanel = CreateUIObject("PausePanel", canvas.transform).AddComponent<Image>();
            pausePanel.color = new Color(0f, 0f, 0f, 0.42f);
            Stretch(pausePanel.rectTransform);
            pausePanel.gameObject.SetActive(false);

            var pauseText = CreateText("PauseText", pausePanel.transform, "PAUSED", 64, TextAnchor.MiddleCenter);
            SetRect(pauseText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(500, 120));

            var hudObj = new GameObject("HUDController");
            var hud = hudObj.AddComponent<HUDController>();
            SetSerializedObject(hud, "scoreText", score);
            SetSerializedObject(hud, "hpText", hp);
            SetSerializedObject(hud, "bossHpSlider", slider);
            SetSerializedObject(hud, "pausePanel", pausePanel.gameObject);
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneDir + "/MainMenu.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Level_01.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Result.unity", true)
            };
        }

        private static Camera CreateCamera(Color background)
        {
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            var camera = cameraObj.AddComponent<Camera>();
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            return camera;
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor align)
        {
            var go = CreateUIObject(name, parent);
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.alignment = align;
            txt.color = Color.white;
            return txt;
        }

        private static GameObject CreateButton(string name, Transform parent, string label)
        {
            var btnObj = CreateUIObject(name, parent);
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.9f, 0.95f);
            btnObj.AddComponent<Button>();

            var labelTxt = CreateText("Label", btnObj.transform, label, 30, TextAnchor.MiddleCenter);
            Stretch(labelTxt.rectTransform);

            return btnObj;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rt, Vector2 anchor, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetSerializedObject(Object target, string propertyName, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedArrayObject(Object target, string propertyName, Object[] values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(propertyName);
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedInt(Object target, string propertyName, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedFloat(Object target, string propertyName, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedBool(Object target, string propertyName, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedEnum(Object target, string propertyName, int enumValue)
        {
            var so = new SerializedObject(target);
            so.FindProperty(propertyName).enumValueIndex = enumValue;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static int ReadSerializedInt(Object target, string propertyName)
        {
            var so = new SerializedObject(target);
            return so.FindProperty(propertyName).intValue;
        }
    }
}

