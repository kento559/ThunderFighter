using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Spawning
{
    public class LevelBackgroundController : MonoBehaviour
    {
        private const int LayerCount = 3;
        private readonly Transform[,] tiles = new Transform[LayerCount, 2];
        private readonly float[] layerSpeeds = { 0.16f, 0.48f, 1.2f };
        private readonly float[] layerDepths = { 28f, 24f, 20f };
        private float tileHeight;
        private float worldWidth;
        private float worldHeight;
        private float nextMeteorAt;
        private Sprite nearDebrisSprite;
        private LevelTheme activeTheme;

        public static void Ensure(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            LevelBackgroundController existing = Object.FindFirstObjectByType<LevelBackgroundController>();
            if (existing != null)
            {
                return;
            }

            GameObject go = new GameObject("[Level] Background");
            LevelBackgroundController controller = go.AddComponent<LevelBackgroundController>();
            controller.Build(camera);
        }

        private void Update()
        {
            if (tileHeight <= 0f)
            {
                return;
            }

            for (int layer = 0; layer < LayerCount; layer++)
            {
                for (int i = 0; i < 2; i++)
                {
                    Transform tile = tiles[layer, i];
                    if (tile == null)
                    {
                        continue;
                    }

                    tile.position += Vector3.down * (layerSpeeds[layer] * Time.deltaTime);
                    if (tile.position.y <= -tileHeight)
                    {
                        float highest = Mathf.Max(tiles[layer, 0] != null ? tiles[layer, 0].position.y : 0f, tiles[layer, 1] != null ? tiles[layer, 1].position.y : 0f);
                        tile.position = new Vector3(0f, highest + tileHeight, layerDepths[layer]);
                    }
                }
            }

            if (Time.time >= nextMeteorAt)
            {
                nextMeteorAt = Time.time + (activeTheme == LevelTheme.AsteroidBelt ? Random.Range(1.6f, 3.4f) : Random.Range(2.8f, 5.2f));
                SpawnMeteor();
            }
        }

        private void Build(Camera camera)
        {
            LevelDefinition level = CampaignRuntime.CurrentLevel ?? CampaignCatalog.GetBySceneName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            activeTheme = level != null ? level.Theme : LevelTheme.Orbital;
            worldHeight = camera.orthographicSize * 2f;
            worldWidth = worldHeight * camera.aspect;
            tileHeight = worldHeight;

            Sprite farSprite = Sprite.Create(CreateFarTexture(activeTheme), new Rect(0f, 0f, 1024f, 1024f), new Vector2(0.5f, 0.5f), 100f);
            Sprite midSprite = Sprite.Create(CreateMidTexture(activeTheme), new Rect(0f, 0f, 1024f, 1024f), new Vector2(0.5f, 0.5f), 100f);
            Sprite nearSprite = Sprite.Create(CreateNearTexture(activeTheme), new Rect(0f, 0f, 1024f, 1024f), new Vector2(0.5f, 0.5f), 100f);
            nearDebrisSprite = nearSprite;

            for (int layer = 0; layer < LayerCount; layer++)
            {
                Sprite sprite = layer == 0 ? farSprite : layer == 1 ? midSprite : nearSprite;
                int sortingOrder = layer == 0 ? -700 : layer == 1 ? -640 : -580;
                for (int i = 0; i < 2; i++)
                {
                    GameObject tile = new GameObject($"BackgroundTile_{layer}_{i}");
                    tile.transform.SetParent(transform, false);
                    tile.transform.position = new Vector3(0f, i * worldHeight, layerDepths[layer]);
                    tile.transform.localScale = new Vector3(worldWidth * 1.04f, worldHeight * 1.04f, 1f);
                    SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = sortingOrder;
                    tiles[layer, i] = tile.transform;
                }
            }

            BuildThemeAccents();
            nextMeteorAt = Time.time + 1.8f;
        }

        private void BuildThemeAccents()
        {
            switch (activeTheme)
            {
                case LevelTheme.AsteroidBelt:
                    CreatePlanet("DustMoon", new Vector3(worldWidth * 0.3f, worldHeight * 0.22f, layerDepths[0] - 1f), 1.2f, new Color(0.62f, 0.48f, 0.32f, 0.26f), new Color(0.18f, 0.12f, 0.08f, 0.4f), -688);
                    CreateCloudBand("DustCloud", new Vector3(-worldWidth * 0.16f, worldHeight * 0.1f, layerDepths[1] - 0.5f), new Vector2(5.6f, 1.9f), new Color(0.52f, 0.34f, 0.18f, 0.18f), -620);
                    break;
                case LevelTheme.DeepSpace:
                    CreatePlanet("BlueGiant", new Vector3(-worldWidth * 0.36f, worldHeight * 0.24f, layerDepths[0] - 1f), 2.1f, new Color(0.24f, 0.52f, 0.94f, 0.32f), new Color(0.08f, 0.18f, 0.42f, 0.5f), -690);
                    CreateCloudBand("FleetGlow", new Vector3(worldWidth * 0.18f, -worldHeight * 0.04f, layerDepths[1] - 0.5f), new Vector2(6.4f, 2.2f), new Color(0.18f, 0.42f, 0.86f, 0.15f), -618);
                    break;
                default:
                    CreatePlanet("OrbitalPlanet", new Vector3(worldWidth * 0.34f, worldHeight * 0.26f, layerDepths[0] - 1f), 2.2f, new Color(0.34f, 0.58f, 0.9f, 0.42f), new Color(0.12f, 0.2f, 0.34f, 0.56f), -690);
                    CreateCloudBand("StationGlow", new Vector3(-worldWidth * 0.18f, worldHeight * 0.2f, layerDepths[1] - 0.5f), new Vector2(4.8f, 1.4f), new Color(0.28f, 0.44f, 0.74f, 0.16f), -620);
                    break;
            }
        }

        private static Texture2D CreateFarTexture(LevelTheme theme)
        {
            Texture2D texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            Color top = theme == LevelTheme.AsteroidBelt ? new Color(0.11f, 0.08f, 0.06f) : theme == LevelTheme.DeepSpace ? new Color(0.02f, 0.05f, 0.14f) : new Color(0.03f, 0.07f, 0.12f);
            Color bottom = theme == LevelTheme.AsteroidBelt ? new Color(0.02f, 0.015f, 0.01f) : new Color(0.01f, 0.015f, 0.04f);
            Color[] pixels = new Color[1024 * 1024];
            for (int y = 0; y < 1024; y++)
            {
                float v = y / 1023f;
                Color baseColor = Color.Lerp(top, bottom, v);
                for (int x = 0; x < 1024; x++)
                {
                    float u = x / 1023f;
                    float cloud = Mathf.PerlinNoise(u * 2.2f + (int)theme * 0.17f, v * 2f + 0.3f);
                    pixels[(y * 1024) + x] = baseColor + (cloud > 0.58f ? baseColor * ((cloud - 0.58f) * 0.35f) : Color.clear);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D CreateMidTexture(LevelTheme theme)
        {
            Texture2D texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[1024 * 1024];
            for (int y = 0; y < 1024; y++)
            {
                for (int x = 0; x < 1024; x++)
                {
                    float u = x / 1023f;
                    float v = y / 1023f;
                    Color color = theme == LevelTheme.AsteroidBelt ? new Color(0.05f, 0.03f, 0.02f) : theme == LevelTheme.DeepSpace ? new Color(0.02f, 0.04f, 0.08f) : new Color(0.04f, 0.07f, 0.12f);
                    float star = Mathf.PerlinNoise((u + 0.2f) * 24f, (v + 0.4f) * 24f);
                    if (star > 0.93f)
                    {
                        float intensity = Mathf.InverseLerp(0.93f, 1f, star);
                        color = Color.Lerp(color, Color.white, intensity);
                    }
                    pixels[(y * 1024) + x] = color;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D CreateNearTexture(LevelTheme theme)
        {
            Texture2D texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[1024 * 1024];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0f, 0f, 0f, 0f);
            }

            int shardCount = theme == LevelTheme.AsteroidBelt ? 130 : 90;
            for (int shard = 0; shard < shardCount; shard++)
            {
                int cx = Random.Range(0, 1024);
                int cy = Random.Range(0, 1024);
                int rx = Random.Range(5, theme == LevelTheme.AsteroidBelt ? 22 : 16);
                int ry = Random.Range(2, 8);
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Color color = theme == LevelTheme.AsteroidBelt ? new Color(0.78f, 0.66f, 0.48f, 0.26f) : theme == LevelTheme.DeepSpace ? new Color(0.44f, 0.72f, 1f, 0.18f) : new Color(0.62f, 0.72f, 0.84f, 0.18f);
                for (int y = -24; y <= 24; y++)
                {
                    for (int x = -24; x <= 24; x++)
                    {
                        int px = cx + x;
                        int py = cy + y;
                        if (px < 0 || py < 0 || px >= 1024 || py >= 1024)
                        {
                            continue;
                        }
                        float localX = x * Mathf.Cos(angle) - y * Mathf.Sin(angle);
                        float localY = x * Mathf.Sin(angle) + y * Mathf.Cos(angle);
                        float dist = Mathf.Abs(localX) / rx + Mathf.Abs(localY) / ry;
                        if (dist <= 1f)
                        {
                            float alpha = (1f - dist) * color.a;
                            pixels[(py * 1024) + px] = Color.Lerp(pixels[(py * 1024) + px], new Color(color.r, color.g, color.b, alpha), alpha);
                        }
                    }
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private void CreatePlanet(string name, Vector3 position, float radius, Color innerColor, Color outerTint, int sortingOrder)
        {
            GameObject planet = new GameObject(name);
            planet.transform.SetParent(transform, false);
            planet.transform.position = position;
            planet.transform.localScale = new Vector3(radius, radius, 1f);
            SpriteRenderer renderer = planet.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Hull);
            renderer.sortingOrder = sortingOrder;
            renderer.color = innerColor;

            GameObject halo = new GameObject(name + "_Halo");
            halo.transform.SetParent(planet.transform, false);
            halo.transform.localScale = new Vector3(1.34f, 1.34f, 1f);
            SpriteRenderer haloRenderer = halo.AddComponent<SpriteRenderer>();
            haloRenderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            haloRenderer.sortingOrder = sortingOrder - 1;
            haloRenderer.color = outerTint;
        }

        private void CreateCloudBand(string name, Vector3 position, Vector2 scale, Color color, int sortingOrder)
        {
            GameObject cloud = new GameObject(name);
            cloud.transform.SetParent(transform, false);
            cloud.transform.position = position;
            cloud.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            SpriteRenderer renderer = cloud.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
        }

        private void SpawnMeteor()
        {
            if (nearDebrisSprite == null)
            {
                return;
            }

            GameObject meteor = new GameObject("_BackgroundMeteor");
            meteor.transform.SetParent(transform, false);
            float startX = Random.Range(-worldWidth * 0.48f, worldWidth * 0.48f);
            meteor.transform.position = new Vector3(startX, worldHeight * 0.62f, 18f);
            meteor.transform.localScale = Vector3.one * Random.Range(activeTheme == LevelTheme.AsteroidBelt ? 0.26f : 0.18f, activeTheme == LevelTheme.AsteroidBelt ? 0.46f : 0.34f);
            meteor.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-28f, 28f));

            SpriteRenderer renderer = meteor.AddComponent<SpriteRenderer>();
            renderer.sprite = nearDebrisSprite;
            renderer.sortingOrder = -560;
            renderer.color = activeTheme == LevelTheme.DeepSpace ? new Color(0.62f, 0.82f, 1f, 0.38f) : new Color(1f, 0.92f, 0.8f, 0.42f);

            TrailRenderer trail = meteor.AddComponent<TrailRenderer>();
            trail.time = 0.42f;
            trail.startWidth = 0.14f;
            trail.endWidth = 0.02f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.sortingOrder = -561;
            trail.startColor = activeTheme == LevelTheme.DeepSpace ? new Color(0.52f, 0.84f, 1f, 0.5f) : new Color(1f, 0.82f, 0.48f, 0.5f);
            trail.endColor = new Color(1f, 0.4f, 0.12f, 0f);

            BackgroundMeteor motion = meteor.AddComponent<BackgroundMeteor>();
            motion.Setup(new Vector3(Random.Range(-0.45f, 0.45f), -Random.Range(2.4f, 3.5f), 0f), Random.Range(-120f, 120f), 3.2f);
        }
    }
}
