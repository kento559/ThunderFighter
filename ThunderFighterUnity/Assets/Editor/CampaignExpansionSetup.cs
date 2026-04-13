using ThunderFighter.Boss;
using ThunderFighter.Combat;
using ThunderFighter.Config;
using ThunderFighter.Core;
using ThunderFighter.EditorSetup;
using ThunderFighter.Player;
using ThunderFighter.Spawning;
using ThunderFighter.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThunderFighter.EditorSetup
{
    public static class CampaignExpansionSetup
    {
        private const string SceneDir = "Assets/Scenes";
        private const string CampaignDir = "Assets/Generated/Campaign";
        private const string CampaignConfigDir = CampaignDir + "/Config";
        private const string CampaignPrefabDir = CampaignDir + "/Prefabs";
        private const string CampaignResourceDir = "Assets/Resources/Campaign";

        [MenuItem("ThunderFighter/Build Campaign Expansion")]
        public static void Run()
        {
            ProjectAutoSetup.Run();
            EnsureFolders();
            CampaignCatalog.ResetCache();

            GameObject playerPrefab = Load<GameObject>("Assets/Generated/Prefabs/Player.prefab");
            GameObject enemyBasic = Load<GameObject>("Assets/Generated/Prefabs/Enemy_Basic.prefab");
            GameObject enemyElite = Load<GameObject>("Assets/Generated/Prefabs/Enemy_Elite.prefab");
            Projectile bossProjectile = Load<Projectile>("Assets/Generated/Prefabs/Projectile_Boss.prefab");
            if (playerPrefab == null || enemyBasic == null || enemyElite == null || bossProjectile == null)
            {
                throw new System.Exception("Base prefabs missing. Run base auto setup first.");
            }

            WaveConfig chapter1Wave = CreateWaveConfig("WaveConfig_Chapter1", new[]
            {
                Wave(enemyBasic, 10, 0.72f, 0f),
                Wave(enemyBasic, 12, 0.58f, 2.1f),
                Wave(enemyElite, 5, 0.92f, 2.2f),
                Wave(enemyBasic, 14, 0.46f, 1.8f)
            }, 3f);
            WaveConfig chapter2Wave = CreateWaveConfig("WaveConfig_Chapter2", new[]
            {
                Wave(enemyBasic, 12, 0.62f, 0f),
                Wave(enemyElite, 7, 0.78f, 1.8f),
                Wave(enemyBasic, 16, 0.44f, 1.9f),
                Wave(enemyElite, 8, 0.66f, 1.6f),
                Wave(enemyBasic, 14, 0.42f, 1.4f)
            }, 2.8f);
            WaveConfig chapter3Wave = CreateWaveConfig("WaveConfig_Chapter3", new[]
            {
                Wave(enemyElite, 8, 0.72f, 0f),
                Wave(enemyBasic, 18, 0.42f, 1.4f),
                Wave(enemyElite, 10, 0.58f, 1.5f),
                Wave(enemyBasic, 20, 0.38f, 1.2f),
                Wave(enemyElite, 12, 0.5f, 1.1f)
            }, 2.4f);

            BossPhaseConfig[] boss1 = CreateBossPhaseSet("Chapter1", new[]
            {
                Phase(1f, 1.4f, 0.72f, 3, 18f),
                Phase(0.68f, 1.8f, 0.58f, 5, 24f),
                Phase(0.36f, 2.1f, 0.42f, 6, 28f)
            });
            BossPhaseConfig[] boss2 = CreateBossPhaseSet("Chapter2", new[]
            {
                Phase(1f, 1.8f, 0.62f, 4, 22f),
                Phase(0.7f, 2.3f, 0.46f, 6, 30f),
                Phase(0.4f, 2.8f, 0.34f, 8, 34f)
            });
            BossPhaseConfig[] boss3 = CreateBossPhaseSet("Chapter3", new[]
            {
                Phase(1f, 2f, 0.56f, 5, 24f),
                Phase(0.72f, 2.6f, 0.4f, 7, 32f),
                Phase(0.38f, 3.2f, 0.28f, 9, 38f)
            });

            GameObject boss1Prefab = CreateBossPrefab("Boss_Chapter1", boss1, bossProjectile, 260, new Color(1f, 0.78f, 0.32f, 1f));
            GameObject boss2Prefab = CreateBossPrefab("Boss_Chapter2", boss2, bossProjectile, 320, new Color(1f, 0.44f, 0.26f, 1f));
            GameObject boss3Prefab = CreateBossPrefab("Boss_Chapter3", boss3, bossProjectile, 420, new Color(0.54f, 0.82f, 1f, 1f));

            LevelDefinition level1 = CreateLevelDefinition(1, "Level_01", "CHAPTER 1", "Orbital Intercept", "Intercept hostile craft and clear the orbital corridor.", LevelTheme.Orbital, chapter1Wave, boss1Prefab, 100, "NORMAL", new Color(0.38f, 0.84f, 1f, 1f), "ORBITAL DEFENSE SCRAMBLE", "HOSTILE RING BROKEN", "WARNING: COMMAND SHIP APPROACH");
            LevelDefinition level2 = CreateLevelDefinition(2, "Level_02", "CHAPTER 2", "Asteroid Breach", "Punch through the asteroid belt and eliminate strike wings.", LevelTheme.AsteroidBelt, chapter2Wave, boss2Prefab, 130, "HARD", new Color(1f, 0.66f, 0.3f, 1f), "ASTEROID BELT ENTRY", "MULTI-VECTOR THREATS DETECTED", "WARNING: ASSAULT CARRIER APPROACH");
            LevelDefinition level3 = CreateLevelDefinition(3, "Level_03", "CHAPTER 3", "Deep Space Flagship", "Engage the flagship task force and neutralize the war core.", LevelTheme.DeepSpace, chapter3Wave, boss3Prefab, 165, "EXTREME", new Color(0.54f, 0.84f, 1f, 1f), "DEEP SPACE TRANSMISSION JAMMED", "ENEMY FLEET PRESSURE RISING", "WARNING: FLAGSHIP CORE ONLINE");

            CreateMainMenuScene();
            CreateChapterSelectScene();
            CreateLevelScene(level1, playerPrefab, boss1Prefab, new Color(0.02f, 0.04f, 0.08f));
            CreateLevelScene(level2, playerPrefab, boss2Prefab, new Color(0.06f, 0.03f, 0.02f));
            CreateLevelScene(level3, playerPrefab, boss3Prefab, new Color(0.01f, 0.02f, 0.08f));
            CreateResultScene();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ThunderFighter] Campaign expansion generated.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Generated");
            EnsureFolder("Assets/Generated", "Campaign");
            EnsureFolder(CampaignDir, "Config");
            EnsureFolder(CampaignDir, "Prefabs");
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "Campaign");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.015f, 0.03f, 0.06f));
            Canvas canvas = CreateCanvas();
            CreateEventSystem();
            CreateFullScreenPanel(canvas.transform, new Color(0.02f, 0.06f, 0.12f, 0.96f));
            CreateGlow(canvas.transform, new Vector2(0.72f, 0.8f), new Vector2(420f, 420f), new Color(0.18f, 0.48f, 0.94f, 0.16f));
            CreateGlow(canvas.transform, new Vector2(0.18f, 0.22f), new Vector2(380f, 380f), new Color(0.08f, 0.84f, 1f, 0.12f));

            Text title = CreateText("Title", canvas.transform, "THUNDER FIGHTER", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.76f), new Vector2(820f, 100f));
            Text subtitle = CreateText("Subtitle", canvas.transform, string.Empty, 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.66f), new Vector2(920f, 50f));
            Text status = CreateText("Status", canvas.transform, string.Empty, 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.58f), new Vector2(940f, 46f));

            Button start = CreateButton("StartButton", canvas.transform, "CAMPAIGN DEPLOY", new Vector2(0.5f, 0.42f), new Vector2(340f, 72f));
            Button quit = CreateButton("QuitButton", canvas.transform, "DISENGAGE", new Vector2(0.5f, 0.31f), new Vector2(340f, 72f));

            GameObject controllerObj = new GameObject("MainMenuUI");
            MainMenuController controller = controllerObj.AddComponent<MainMenuController>();
            SetObject(controller, "startButton", start);
            SetObject(controller, "quitButton", quit);
            SetObject(controller, "subtitleText", subtitle);
            SetObject(controller, "statusText", status);

            EditorSceneManager.SaveScene(scene, SceneDir + "/MainMenu.unity");
        }

        private static void CreateChapterSelectScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.01f, 0.03f, 0.08f));
            Canvas canvas = CreateCanvas();
            CreateEventSystem();
            CreateFullScreenPanel(canvas.transform, new Color(0.015f, 0.04f, 0.08f, 0.96f));
            CreateGlow(canvas.transform, new Vector2(0.82f, 0.18f), new Vector2(420f, 420f), new Color(0.16f, 0.48f, 0.92f, 0.14f));
            new GameObject("ChapterSelectUI").AddComponent<ChapterSelectController>();
            EditorSceneManager.SaveScene(scene, SceneDir + "/ChapterSelect.unity");
        }

        private static void CreateLevelScene(LevelDefinition level, GameObject playerPrefab, GameObject bossPrefab, Color clearColor)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(clearColor);
            PrefabUtility.InstantiatePrefab(playerPrefab);

            GameObject spawnerObj = new GameObject("Spawner");
            EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
            SetObject(spawner, "waveConfig", level.WaveConfig);
            SetObject(spawner, "bossPrefab", bossPrefab);
            SetString(spawner, "overrideStartAnnouncement", level.StartAnnouncement);
            SetString(spawner, "overrideMidAnnouncement", level.MidAnnouncement);
            SetString(spawner, "overrideBossAnnouncement", level.BossAnnouncement);

            Transform[] spawnPoints = new Transform[5];
            float[] xs = { -6.5f, -3.2f, 0f, 3.2f, 6.5f };
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform p = new GameObject("SpawnPoint_" + (i + 1)).transform;
                p.position = new Vector3(xs[i], 6f, 0f);
                p.SetParent(spawnerObj.transform);
                spawnPoints[i] = p;
            }
            Transform bossPoint = new GameObject("BossSpawnPoint").transform;
            bossPoint.position = new Vector3(0f, 4.1f, 0f);
            bossPoint.SetParent(spawnerObj.transform);
            SetArray(spawner, "spawnPoints", spawnPoints);
            SetObject(spawner, "bossSpawnPoint", bossPoint);

            CreateHUD();
            EditorSceneManager.SaveScene(scene, SceneDir + "/" + level.SceneName + ".unity");
        }

        private static void CreateResultScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.02f, 0.02f, 0.05f));
            Canvas canvas = CreateCanvas();
            CreateEventSystem();
            CreateFullScreenPanel(canvas.transform, new Color(0.02f, 0.03f, 0.08f, 0.96f));

            Text title = CreateText("Title", canvas.transform, "CHAPTER CLEAR", 56, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.76f), new Vector2(900f, 100f));
            Text score = CreateText("Score", canvas.transform, "RATING B  |  SCORE 0", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.62f), new Vector2(960f, 60f));
            Text detail = CreateText("Detail", canvas.transform, string.Empty, 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(980f, 90f));
            Text unlock = CreateText("Unlock", canvas.transform, string.Empty, 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.38f), new Vector2(980f, 50f));

            Button retry = CreateButton("RetryButton", canvas.transform, "RETRY", new Vector2(0.32f, 0.22f), new Vector2(220f, 62f));
            Button next = CreateButton("NextButton", canvas.transform, "NEXT CHAPTER", new Vector2(0.5f, 0.22f), new Vector2(240f, 62f));
            Button menu = CreateButton("MenuButton", canvas.transform, "RETURN TO HANGAR", new Vector2(0.68f, 0.22f), new Vector2(260f, 62f));

            GameObject resultObj = new GameObject("ResultUI");
            ResultController result = resultObj.AddComponent<ResultController>();
            SetObject(result, "titleText", title);
            SetObject(result, "scoreText", score);
            SetObject(result, "detailText", detail);
            SetObject(result, "unlockText", unlock);
            SetObject(result, "retryButton", retry);
            SetObject(result, "menuButton", menu);
            SetObject(result, "nextButton", next);

            EditorSceneManager.SaveScene(scene, SceneDir + "/Result.unity");
        }

        private static void CreateHUD()
        {
            Canvas canvas = CreateCanvas();
            Text score = CreateText("ScoreText", canvas.transform, "Score: 0", 28, TextAnchor.MiddleLeft, new Vector2(0.11f, 0.95f), new Vector2(320f, 48f));
            Text hp = CreateText("HpText", canvas.transform, "HP: 100/100", 28, TextAnchor.MiddleLeft, new Vector2(0.1f, 0.89f), new Vector2(320f, 48f));
            score.color = Color.cyan;
            hp.color = new Color(0.6f, 1f, 0.85f, 1f);

            GameObject sliderObj = CreateUiObject("BossHpSlider", canvas.transform);
            RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
            SetRect(sliderRt, new Vector2(0.5f, 0.95f), new Vector2(620f, 24f));
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.direction = Slider.Direction.LeftToRight;
            Image bg = CreateUiObject("Background", sliderObj.transform).AddComponent<Image>();
            bg.color = new Color(0.15f, 0.12f, 0.12f, 0.9f);
            Stretch(bg.rectTransform);
            GameObject fillArea = CreateUiObject("Fill Area", sliderObj.transform);
            Stretch(fillArea.GetComponent<RectTransform>());
            Image fill = CreateUiObject("Fill", fillArea.transform).AddComponent<Image>();
            fill.color = new Color(1f, 0.34f, 0.2f, 1f);
            Stretch(fill.rectTransform);
            slider.targetGraphic = fill;
            slider.fillRect = fill.rectTransform;

            Image pausePanel = CreateUiObject("PausePanel", canvas.transform).AddComponent<Image>();
            pausePanel.color = new Color(0f, 0f, 0f, 0.42f);
            Stretch(pausePanel.rectTransform);
            pausePanel.gameObject.SetActive(false);
            CreateText("PauseText", pausePanel.transform, "PAUSED", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(500f, 120f));

            GameObject hudObj = new GameObject("HUDController");
            HUDController hud = hudObj.AddComponent<HUDController>();
            SetObject(hud, "scoreText", score);
            SetObject(hud, "hpText", hp);
            SetObject(hud, "bossHpSlider", slider);
            SetObject(hud, "pausePanel", pausePanel.gameObject);
        }

        private static BossPhaseConfig[] CreateBossPhaseSet(string prefix, PhaseSpec[] specs)
        {
            BossPhaseConfig[] phases = new BossPhaseConfig[specs.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                BossPhaseConfig phase = CreateAsset<BossPhaseConfig>($"{CampaignConfigDir}/{prefix}_Phase_{i + 1:00}.asset");
                SetFloat(phase, "<TriggerHpRatio>k__BackingField", specs[i].Trigger);
                SetFloat(phase, "<MoveSpeed>k__BackingField", specs[i].Move);
                SetFloat(phase, "<FireInterval>k__BackingField", specs[i].FireInterval);
                SetInt(phase, "<BulletsPerShot>k__BackingField", specs[i].Bullets);
                SetFloat(phase, "<SpreadAngle>k__BackingField", specs[i].Spread);
                phases[i] = phase;
            }
            return phases;
        }

        private static WaveConfig CreateWaveConfig(string name, WaveSpec[] specs, float bossDelay)
        {
            WaveConfig config = CreateAsset<WaveConfig>($"{CampaignConfigDir}/{name}.asset");
            SerializedObject so = new SerializedObject(config);
            SerializedProperty list = so.FindProperty("<Waves>k__BackingField");
            list.ClearArray();
            for (int i = 0; i < specs.Length; i++)
            {
                list.InsertArrayElementAtIndex(i);
                SerializedProperty wave = list.GetArrayElementAtIndex(i);
                wave.FindPropertyRelative("enemyPrefab").objectReferenceValue = specs[i].Prefab;
                wave.FindPropertyRelative("count").intValue = specs[i].Count;
                wave.FindPropertyRelative("spawnInterval").floatValue = specs[i].Interval;
                wave.FindPropertyRelative("startDelay").floatValue = specs[i].Delay;
            }
            so.FindProperty("<BossSpawnDelayAfterLastWave>k__BackingField").floatValue = bossDelay;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static LevelDefinition CreateLevelDefinition(int chapterIndex, string sceneName, string chapterTitle, string subtitle, string objective, LevelTheme theme, WaveConfig waveConfig, GameObject bossPrefab, int recommendedPower, string difficulty, Color accent, string startAnnouncement, string midAnnouncement, string bossAnnouncement)
        {
            LevelDefinition level = CreateAsset<LevelDefinition>($"{CampaignResourceDir}/{sceneName}.asset");
            SetInt(level, "<ChapterIndex>k__BackingField", chapterIndex);
            SetString(level, "<SceneName>k__BackingField", sceneName);
            SetString(level, "<ChapterTitle>k__BackingField", chapterTitle);
            SetString(level, "<Subtitle>k__BackingField", subtitle);
            SetString(level, "<ObjectiveText>k__BackingField", objective);
            SetString(level, "<StartAnnouncement>k__BackingField", startAnnouncement);
            SetString(level, "<MidAnnouncement>k__BackingField", midAnnouncement);
            SetString(level, "<BossAnnouncement>k__BackingField", bossAnnouncement);
            SetEnum(level, "<Theme>k__BackingField", (int)theme);
            SetObject(level, "<WaveConfig>k__BackingField", waveConfig);
            SetObject(level, "<BossPrefab>k__BackingField", bossPrefab);
            SetInt(level, "<RecommendedPower>k__BackingField", recommendedPower);
            SetString(level, "<DifficultyLabel>k__BackingField", difficulty);
            SetColor(level, "<AccentColor>k__BackingField", accent);
            return level;
        }

        private static GameObject CreateBossPrefab(string name, BossPhaseConfig[] phases, Projectile projectilePrefab, int maxHp, Color tint)
        {
            string path = CampaignPrefabDir + "/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            GameObject root = new GameObject(name);
            root.transform.localScale = new Vector3(2.3f, 2.3f, 1f);
            SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
            sr.color = tint;
            Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            CircleCollider2D col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;
            HealthComponent health = root.AddComponent<HealthComponent>();
            SetEnum(health, "faction", (int)Faction.Enemy);
            SetInt(health, "maxHp", maxHp);
            SetBool(health, "notifyBossHpEvents", true);
            SetBool(health, "notifyDeathAsBoss", true);
            Transform firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(root.transform);
            firePoint.localPosition = new Vector3(0f, -0.8f, 0f);
            firePoint.localRotation = Quaternion.Euler(0f, 0f, 180f);
            BossController boss = root.AddComponent<BossController>();
            SetArray(boss, "phases", phases);
            SetObject(boss, "projectilePrefab", projectilePrefab);
            SetObject(boss, "firePoint", firePoint);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static PhaseSpec Phase(float trigger, float move, float interval, int bullets, float spread) => new PhaseSpec { Trigger = trigger, Move = move, FireInterval = interval, Bullets = bullets, Spread = spread };
        private static WaveSpec Wave(GameObject prefab, int count, float interval, float delay) => new WaveSpec { Prefab = prefab, Count = count, Interval = interval, Delay = delay };

        private struct WaveSpec { public GameObject Prefab; public int Count; public float Interval; public float Delay; }
        private struct PhaseSpec { public float Trigger; public float Move; public float FireInterval; public int Bullets; public float Spread; }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneDir + "/MainMenu.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/ChapterSelect.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Level_01.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Level_02.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Level_03.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/Result.unity", true)
            };
        }

        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

        private static void EnsureFolder(string parent, string child)
        {
            string full = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Camera CreateCamera(Color background)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            return camera;
        }

        private static Canvas CreateCanvas()
        {
            GameObject go = new GameObject("Canvas");
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static Image CreateFullScreenPanel(Transform parent, Color color)
        {
            Image image = CreateUiObject("BackgroundPanel", parent).AddComponent<Image>();
            image.color = color;
            Stretch(image.rectTransform);
            return image;
        }

        private static void CreateGlow(Transform parent, Vector2 anchor, Vector2 size, Color color)
        {
            Image image = CreateUiObject("Glow", parent).AddComponent<Image>();
            SetRect(image.rectTransform, anchor, size);
            image.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            image.color = color;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor align, Vector2 anchor, Vector2 size)
        {
            Text text = CreateUiObject(name, parent).AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = align;
            text.text = content;
            text.color = Color.white;
            SetRect(text.rectTransform, anchor, size);
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchor, Vector2 size)
        {
            GameObject go = CreateUiObject(name, parent);
            Button button = go.AddComponent<Button>();
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.1f, 0.36f, 0.82f, 0.95f);
            SetRect(go.GetComponent<RectTransform>(), anchor, size);
            Text text = CreateText("Label", go.transform, label, 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), size);
            text.color = new Color(0.95f, 0.98f, 1f, 1f);
            return button;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            RectTransform rt = go.AddComponent<RectTransform>();
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
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(propertyName).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetArray(Object target, string propertyName, Object[] values)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(propertyName);
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(propertyName).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(propertyName).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetBool(Object target, string propertyName, bool value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(propertyName).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(propertyName).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnum(Object target, string propertyName, int value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(propertyName).enumValueIndex = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetColor(Object target, string propertyName, Color value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(propertyName).colorValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
