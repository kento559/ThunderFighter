using System.Collections;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Combat
{
    [RequireComponent(typeof(HealthComponent))]
    public class DamageFeedback : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private float punchScale = 1.08f;
        [SerializeField] private AudioSource audioSource;

        private HealthComponent health;
        private Coroutine flashRoutine;
        private Vector3 baseScale;
        private ProceduralShipVisual shipVisual;
        private SpriteRenderer primaryRenderer;

        private static AudioClip hitClip;
        private static AudioClip deathClip;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            shipVisual = GetComponent<ProceduralShipVisual>();

            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            if (shipVisual != null)
            {
                primaryRenderer = shipVisual.GetPrimaryRenderer();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            baseScale = transform.localScale;
            EnsureAudioClips();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDamaged += HandleDamaged;
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDamaged -= HandleDamaged;
                health.OnDied -= HandleDied;
            }
        }

        private void HandleDamaged(int amount, int hp)
        {
            if (primaryRenderer == null && shipVisual != null)
            {
                primaryRenderer = shipVisual.GetPrimaryRenderer();
            }

            flashColor = ResolveFlashColor();

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashAndPunch());
            bool heavyTarget = health != null && health.MaxHp >= 8;
            SpawnBurst(heavyTarget ? 18 : 12, heavyTarget ? 0.28f : 0.22f, heavyTarget ? 1.2f : 1f);
            SpawnHeatFlash(heavyTarget ? 1.4f : 1f, false);
            SpawnDebris(false, heavyTarget);

            if (hitClip != null)
            {
                PlayFeedbackClip(hitClip, heavyTarget ? 0.85f : 1.1f, 0.24f * GameSettingsService.SfxVolume);
            }
        }

        private void HandleDied()
        {
            if (primaryRenderer == null && shipVisual != null)
            {
                primaryRenderer = shipVisual.GetPrimaryRenderer();
            }

            flashColor = ResolveFlashColor();

            bool heavyTarget = health != null && health.MaxHp >= 8;
            SpawnBurst(heavyTarget ? 42 : 28, heavyTarget ? 0.7f : 0.5f, heavyTarget ? 2f : 1.6f);
            SpawnShockwaveRing(heavyTarget ? 2.2f : 1.55f, heavyTarget ? 0.42f : 0.3f);
            SpawnHeatFlash(heavyTarget ? 2.1f : 1.55f, true);
            SpawnDebris(true, heavyTarget);

            if (deathClip != null)
            {
                PlayFeedbackClip(deathClip, heavyTarget ? 0.72f : 0.95f, (heavyTarget ? 0.45f : 0.35f) * GameSettingsService.SfxVolume);
            }
        }

        private void PlayFeedbackClip(AudioClip clip, float pitch, float volume)
        {
            if (clip == null || volume <= 0.001f)
            {
                return;
            }

            if (audioSource != null && audioSource.isActiveAndEnabled && audioSource.gameObject.activeInHierarchy)
            {
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(clip, volume);
                audioSource.pitch = 1f;
                return;
            }

            GameObject temp = new GameObject("_DamageFeedbackAudio");
            temp.transform.position = transform.position;
            AudioSource tempSource = temp.AddComponent<AudioSource>();
            tempSource.playOnAwake = false;
            tempSource.spatialBlend = 0f;
            tempSource.pitch = pitch;
            tempSource.volume = 1f;
            tempSource.clip = clip;
            tempSource.PlayOneShot(clip, volume);
            Object.Destroy(temp, Mathf.Max(clip.length / Mathf.Max(0.01f, pitch), 0.4f));
        }

        private IEnumerator FlashAndPunch()
        {
            if (renderers == null || renderers.Length == 0)
            {
                yield break;
            }

            Color[] original = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    original[i] = renderers[i].color;
                    renderers[i].color = flashColor;
                }
            }

            Sprite originalSprite = primaryRenderer != null ? primaryRenderer.sprite : null;
            Sprite damagedSprite = GetDamagedSprite();
            if (primaryRenderer != null && damagedSprite != null)
            {
                primaryRenderer.sprite = damagedSprite;
            }

            transform.localScale = baseScale * punchScale;
            yield return new WaitForSeconds(flashDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = original[i];
                }
            }

            if (primaryRenderer != null && originalSprite != null)
            {
                primaryRenderer.sprite = originalSprite;
            }

            transform.localScale = baseScale;
            flashRoutine = null;
        }

        private void SpawnBurst(int count, float size, float speed)
        {
            GameObject go = new GameObject("_HitBurst");
            go.transform.position = transform.position;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.18f;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = flashColor;
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.18f;

            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 40;

            ps.Emit(count);
            ps.Play();
            Object.Destroy(go, 0.6f);
        }

        private void SpawnShockwaveRing(float maxScale, float duration)
        {
            GameObject ring = new GameObject("_ShockwaveRing");
            ring.transform.position = transform.position;
            SpriteRenderer renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Ring);
            renderer.color = new Color(1f, 0.86f, 0.62f, 0.9f);

            TransientSpriteEffect effect = ring.AddComponent<TransientSpriteEffect>();
            effect.Setup(
                new Vector3(0.25f, 0.25f, 1f),
                new Vector3(maxScale, maxScale, 1f),
                new Color(1f, 0.9f, 0.72f, 0.9f),
                new Color(1f, 0.55f, 0.1f, 0f),
                duration,
                150);
        }

        private void SpawnHeatFlash(float maxScale, bool intense)
        {
            GameObject flash = new GameObject("_HeatFlash");
            flash.transform.position = transform.position;
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash) ?? GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            renderer.color = intense ? new Color(1f, 0.96f, 0.86f, 0.84f) : new Color(1f, 0.92f, 0.76f, 0.62f);

            float duration = intense ? 0.18f : 0.11f;
            float startScale = intense ? 0.55f : 0.34f;
            TransientSpriteEffect effect = flash.AddComponent<TransientSpriteEffect>();
            effect.Setup(
                new Vector3(startScale, startScale, 1f),
                new Vector3(maxScale, maxScale, 1f),
                renderer.color,
                new Color(1f, 0.4f, 0.08f, 0f),
                duration,
                151);
        }

        private void SpawnDebris(bool destroyed, bool heavyTarget)
        {
            RuntimeArtSpriteId[] ids = GetFragmentIds();
            if (ids == null || ids.Length == 0)
            {
                return;
            }

            int count = destroyed ? (heavyTarget ? 8 : 5) : (heavyTarget ? 4 : 2);
            float speed = destroyed ? (heavyTarget ? 3.6f : 2.8f) : 1.8f;
            float scale = destroyed ? 0.88f : 0.62f;

            for (int i = 0; i < count; i++)
            {
                Sprite sprite = RuntimeArtLibrary.Get(ids[i % ids.Length]);
                if (sprite == null)
                {
                    continue;
                }

                GameObject shard = new GameObject(destroyed ? "_ExplosionShard" : "_HitShard");
                shard.transform.position = transform.position + (Vector3)Random.insideUnitCircle * (destroyed ? 0.32f : 0.14f);
                shard.transform.localScale = Vector3.one * (scale * Random.Range(0.85f, 1.2f));

                SpriteRenderer renderer = shard.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = Color.white;

                DebrisShard debris = shard.AddComponent<DebrisShard>();
                Vector2 dir = destroyed
                    ? Random.insideUnitCircle.normalized
                    : (Random.insideUnitCircle.normalized * 0.6f) + Vector2.up * 0.4f;
                debris.Setup(dir * Random.Range(speed * 0.72f, speed * 1.18f), Random.Range(-240f, 240f), destroyed ? 0.72f : 0.34f, heavyTarget ? 135 : 125);
            }
        }

        private Sprite GetDamagedSprite()
        {
            if (shipVisual == null)
            {
                return null;
            }

            return RuntimeArtLibrary.Get(shipVisual.CurrentDamagedSpriteId);
        }

        private RuntimeArtSpriteId[] GetFragmentIds()
        {
            if (shipVisual == null)
            {
                return null;
            }

            return shipVisual.CurrentFragmentIds;
        }

        private Color ResolveFlashColor()
        {
            Player.PlayerController player = GetComponent<Player.PlayerController>();
            if (player != null)
            {
                Color accent = player.CurrentAccentColor;
                return Color.Lerp(accent, Color.white, 0.58f);
            }

            return flashColor;
        }

        private static void EnsureAudioClips()
        {
            if (hitClip == null)
            {
                hitClip = BuildTone("hit", 1200f, 0.06f, 0.08f);
            }

            if (deathClip == null)
            {
                deathClip = BuildTone("death", 260f, 0.2f, 0.13f);
            }
        }

        private static AudioClip BuildTone(string clipName, float frequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
