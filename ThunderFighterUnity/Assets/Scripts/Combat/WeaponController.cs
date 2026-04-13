using ThunderFighter.Config;
using ThunderFighter.Core;
using ThunderFighter.Player;
using UnityEngine;

namespace ThunderFighter.Combat
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private Transform[] supportFirePoints;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Faction ownerFaction = Faction.Player;
        [SerializeField] private bool usePooling = true;

        private float nextPrimaryFireTime;
        private float nextSupportFireTime;
        private float primaryIntervalMultiplier = 1f;
        private float supportIntervalMultiplier = 1f;
        private float projectileSpeedMultiplier = 1f;
        private int bonusDamage;
        private float damageMultiplier = 1f;
        private float progressionPrimaryMultiplier = 1f;
        private float progressionSupportMultiplier = 1f;
        private int progressionDamageBonus;

        private ShipArchetype playerArchetype = ShipArchetype.Balanced;
        private bool playerSupportEnabled = true;
        private int weaponLevel = 1;
        private float shipPrimaryIntervalMultiplier = 1f;
        private float shipSupportIntervalMultiplier = 1f;
        private float shipProjectileSpeedMultiplier = 1f;
        private int shipDamageBonus;
        private Color projectileTint = Color.white;
        private Vector3 projectileScale = Vector3.one;

        public bool TryFire()
        {
            if (weaponConfig == null || projectilePrefab == null || firePoints == null || firePoints.Length == 0)
            {
                return false;
            }

            bool fired = false;
            if (Time.time >= nextPrimaryFireTime)
            {
                nextPrimaryFireTime = Time.time + GetPrimaryInterval();
                FirePrimaryCannons();
                fired = true;
            }

            if (ShouldFireSupport() && Time.time >= nextSupportFireTime)
            {
                nextSupportFireTime = Time.time + GetSupportInterval();
                FireSupportCannons();
                fired = true;
            }

            return fired;
        }

        public void ApplyLoadout(ShipDefinition ship)
        {
            if (ship == null)
            {
                return;
            }

            playerArchetype = ship.Archetype;
            playerSupportEnabled = ship.SupportCannonsEnabled;
            shipPrimaryIntervalMultiplier = Mathf.Max(0.1f, ship.PrimaryIntervalMultiplier);
            shipSupportIntervalMultiplier = Mathf.Max(0.1f, ship.SupportIntervalMultiplier);
            shipProjectileSpeedMultiplier = Mathf.Max(0.2f, ship.ProjectileSpeedMultiplier);
            shipDamageBonus = ship.DamageBonus;
            projectileTint = ship.ProjectileTint;
            projectileScale = ship.ProjectileScale;
        }

        public void SetWeaponLevel(int level)
        {
            weaponLevel = Mathf.Clamp(level, 1, 4);
        }

        public int GetWeaponLevel()
        {
            return weaponLevel;
        }

        public void SetRuntimeModifiers(float primaryRateMultiplier, float supportRateMultiplier, float speedMultiplier, int damageBonus, float totalDamageMultiplier = 1f)
        {
            primaryIntervalMultiplier = Mathf.Max(0.1f, primaryRateMultiplier);
            supportIntervalMultiplier = Mathf.Max(0.1f, supportRateMultiplier);
            projectileSpeedMultiplier = Mathf.Max(0.2f, speedMultiplier);
            bonusDamage = damageBonus;
            damageMultiplier = Mathf.Max(0.25f, totalDamageMultiplier);
        }

        public void SetProgressionModifiers(float fireRateMultiplier, int damageBonus)
        {
            progressionPrimaryMultiplier = Mathf.Max(0.1f, fireRateMultiplier);
            progressionSupportMultiplier = Mathf.Max(0.1f, fireRateMultiplier);
            progressionDamageBonus = Mathf.Max(0, damageBonus);
        }

        public void ResetRuntimeModifiers()
        {
            primaryIntervalMultiplier = 1f;
            supportIntervalMultiplier = 1f;
            projectileSpeedMultiplier = 1f;
            bonusDamage = 0;
            damageMultiplier = 1f;
        }

        public void ConfigurePlayerFirePoints(Transform mainFirePoint, Transform leftSupportFirePoint, Transform rightSupportFirePoint)
        {
            if (mainFirePoint != null)
            {
                firePoints = new[] { mainFirePoint };
            }

            if (leftSupportFirePoint != null && rightSupportFirePoint != null)
            {
                supportFirePoints = new[] { leftSupportFirePoint, rightSupportFirePoint };
            }
        }

        public bool FireBurstPattern(int projectileCount, float spreadAngle, float centerAngle, float speedMultiplier = 1f, int damageOffset = 0)
        {
            if (weaponConfig == null || projectilePrefab == null || firePoints == null || firePoints.Length == 0 || projectileCount <= 0)
            {
                return false;
            }

            Transform primary = firePoints[0];
            float startAngle = projectileCount > 1 ? -spreadAngle * 0.5f : 0f;
            float step = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0f;
            int damage = ComputeDamage(weaponConfig.ProjectileDamage + damageOffset);
            float speed = weaponConfig.ProjectileSpeed * shipProjectileSpeedMultiplier * projectileSpeedMultiplier * Mathf.Max(0.2f, speedMultiplier);

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = centerAngle + startAngle + step * i;
                Quaternion rotation = primary.rotation * Quaternion.Euler(0f, 0f, angle);
                SpawnProjectile(primary.position, rotation, damage, speed, false, projectileTint, projectileScale);
            }

            SpawnMuzzleFlash(primary.position, false);
            return true;
        }

        private float GetPrimaryInterval()
        {
            return weaponConfig.FireInterval * Mathf.Max(0.1f, primaryIntervalMultiplier * progressionPrimaryMultiplier * shipPrimaryIntervalMultiplier);
        }

        private float GetSupportInterval()
        {
            return weaponConfig.SupportFireInterval * Mathf.Max(0.1f, supportIntervalMultiplier * progressionSupportMultiplier * shipSupportIntervalMultiplier);
        }

        private bool ShouldFireSupport()
        {
            return weaponConfig.SupportCannonsEnabled && playerSupportEnabled && ownerFaction == Faction.Player && weaponLevel >= 2;
        }

        private int ComputeDamage(int baseDamage)
        {
            int total = baseDamage + progressionDamageBonus + bonusDamage + shipDamageBonus;
            total = Mathf.Max(1, total);
            return Mathf.Max(1, Mathf.RoundToInt(total * damageMultiplier));
        }

        private void FirePrimaryCannons()
        {
            if (ownerFaction != Faction.Player)
            {
                foreach (Transform firePoint in firePoints)
                {
                    SpawnProjectile(firePoint.position, firePoint.rotation, ComputeDamage(weaponConfig.ProjectileDamage), weaponConfig.ProjectileSpeed * projectileSpeedMultiplier, false, Color.white, Vector3.one);
                    SpawnMuzzleFlash(firePoint.position, false);
                }
                return;
            }

            Transform primary = firePoints[0];
            switch (playerArchetype)
            {
                case ShipArchetype.Rapid:
                    FireRapidPrimary(primary);
                    break;
                case ShipArchetype.Heavy:
                    FireHeavyPrimary(primary);
                    break;
                default:
                    FireBalancedPrimary(primary);
                    break;
            }
        }

        private void FireSupportCannons()
        {
            if (ownerFaction != Faction.Player)
            {
                FireDefaultSupportCannons();
                return;
            }

            switch (playerArchetype)
            {
                case ShipArchetype.Rapid:
                    FireRapidSupport();
                    break;
                case ShipArchetype.Heavy:
                    FireHeavySupport();
                    break;
                default:
                    FireBalancedSupport();
                    break;
            }
        }

        private void FireBalancedPrimary(Transform primary)
        {
            int damage = ComputeDamage(weaponConfig.ProjectileDamage);
            float speed = weaponConfig.ProjectileSpeed * shipProjectileSpeedMultiplier * projectileSpeedMultiplier;
            switch (weaponLevel)
            {
                case 1:
                    SpawnProjectile(primary.position, primary.rotation, damage, speed, false, projectileTint, projectileScale);
                    break;
                case 2:
                    SpawnPattern(primary, new[] { -0.18f, 0f, 0.18f }, new[] { -5f, 0f, 5f }, damage, speed, projectileTint, projectileScale, false);
                    break;
                case 3:
                    SpawnPattern(primary, new[] { -0.28f, -0.12f, 0f, 0.12f, 0.28f }, new[] { -11f, -4f, 0f, 4f, 11f }, damage, speed, projectileTint, projectileScale, false);
                    break;
                default:
                    SpawnPattern(primary, new[] { -0.38f, -0.25f, -0.12f, 0f, 0.12f, 0.25f, 0.38f }, new[] { -16f, -10f, -5f, 0f, 5f, 10f, 16f }, damage + 1, speed * 1.02f, projectileTint, projectileScale, false);
                    break;
            }

            SpawnMuzzleFlash(primary.position, false);
        }

        private void FireRapidPrimary(Transform primary)
        {
            int damage = ComputeDamage(Mathf.Max(1, weaponConfig.ProjectileDamage));
            float speed = weaponConfig.ProjectileSpeed * shipProjectileSpeedMultiplier * projectileSpeedMultiplier * 1.08f;
            Vector3 rapidScale = new Vector3(projectileScale.x * 0.86f, projectileScale.y * 0.9f, 1f);
            switch (weaponLevel)
            {
                case 1:
                    SpawnPattern(primary, new[] { -0.16f, 0f, 0.16f }, new[] { -4f, 0f, 4f }, damage, speed, projectileTint, rapidScale, false);
                    break;
                case 2:
                    SpawnPattern(primary, new[] { -0.24f, -0.08f, 0.08f, 0.24f }, new[] { -9f, -3f, 3f, 9f }, damage, speed, projectileTint, rapidScale, false);
                    break;
                case 3:
                    SpawnPattern(primary, new[] { -0.34f, -0.2f, -0.07f, 0.07f, 0.2f, 0.34f }, new[] { -13f, -8f, -3f, 3f, 8f, 13f }, damage, speed * 1.02f, projectileTint, rapidScale, false);
                    break;
                default:
                    SpawnPattern(primary, new[] { -0.44f, -0.31f, -0.18f, -0.06f, 0.06f, 0.18f, 0.31f, 0.44f }, new[] { -17f, -12f, -7f, -2f, 2f, 7f, 12f, 17f }, damage, speed * 1.04f, projectileTint, rapidScale, false);
                    break;
            }

            SpawnMuzzleFlash(primary.position, false);
        }

        private void FireHeavyPrimary(Transform primary)
        {
            int damage = ComputeDamage(weaponConfig.ProjectileDamage + 1);
            float speed = weaponConfig.ProjectileSpeed * shipProjectileSpeedMultiplier * projectileSpeedMultiplier * 0.94f;
            Vector3 heavyScale = new Vector3(projectileScale.x * 1.18f, projectileScale.y * 1.28f, 1f);
            switch (weaponLevel)
            {
                case 1:
                    SpawnProjectile(primary.position, primary.rotation, damage + 1, speed, false, projectileTint, heavyScale);
                    break;
                case 2:
                    SpawnPattern(primary, new[] { -0.2f, 0f, 0.2f }, new[] { -6f, 0f, 6f }, damage + 1, speed, projectileTint, heavyScale, false);
                    break;
                case 3:
                    SpawnPattern(primary, new[] { -0.28f, -0.12f, 0f, 0.12f, 0.28f }, new[] { -10f, -4f, 0f, 4f, 10f }, damage + 2, speed, projectileTint, heavyScale, false);
                    break;
                default:
                    SpawnPattern(primary, new[] { -0.38f, -0.2f, 0f, 0.2f, 0.38f }, new[] { -14f, -7f, 0f, 7f, 14f }, damage + 2, speed * 0.98f, projectileTint, heavyScale, false);
                    SpawnProjectile(primary.position, primary.rotation, damage + 4, speed * 1.1f, false, new Color(1f, 0.82f, 0.56f, 1f), new Vector3(heavyScale.x * 1.26f, heavyScale.y * 1.48f, 1f));
                    break;
            }

            SpawnMuzzleFlash(primary.position, false);
        }

        private void FireBalancedSupport()
        {
            Transform primary = firePoints[0];
            int damage = ComputeDamage(weaponConfig.SupportProjectileDamage);
            float speed = weaponConfig.SupportProjectileSpeed * shipProjectileSpeedMultiplier * projectileSpeedMultiplier;
            Vector3 scale = new Vector3(projectileScale.x * 0.94f, projectileScale.y * 0.94f, 1f);

            if (weaponLevel == 2)
            {
                SpawnSupportPair(primary, damage, speed, 8f, scale);
            }
            else if (weaponLevel == 3)
            {
                SpawnSupportPair(primary, damage + 1, speed, 17f, scale);
            }
            else
            {
                SpawnSupportPair(primary, damage + 1, speed, 20f, scale);
                SpawnSupportPair(primary, damage, speed * 0.98f, 8f, new Vector3(scale.x * 0.86f, scale.y * 0.92f, 1f));
            }
        }

        private void FireRapidSupport()
        {
            Transform primary = firePoints[0];
            int damage = ComputeDamage(Mathf.Max(1, weaponConfig.SupportProjectileDamage));
            float speed = weaponConfig.SupportProjectileSpeed * shipProjectileSpeedMultiplier * projectileSpeedMultiplier * 1.1f;
            Vector3 scale = new Vector3(projectileScale.x * 0.78f, projectileScale.y * 0.84f, 1f);
            SpawnSupportPair(primary, damage, speed, weaponLevel >= 4 ? 24f : 16f, scale);
            if (weaponLevel >= 3)
            {
                SpawnSupportPair(primary, damage, speed, weaponLevel >= 4 ? 10f : 6f, scale);
            }
        }

        private void FireHeavySupport()
        {
            Transform primary = firePoints[0];
            int damage = ComputeDamage(weaponConfig.SupportProjectileDamage + (weaponLevel >= 4 ? 2 : 1));
            float speed = weaponConfig.SupportProjectileSpeed * shipProjectileSpeedMultiplier * projectileSpeedMultiplier * 0.92f;
            Vector3 scale = new Vector3(projectileScale.x * 1.06f, projectileScale.y * 1.18f, 1f);
            SpawnSupportPair(primary, damage, speed, 13f, scale);
            if (weaponLevel >= 4)
            {
                SpawnSupportPair(primary, damage, speed, 5f, scale);
            }
        }

        private void FireDefaultSupportCannons()
        {
            Transform primary = firePoints[0];
            Vector3 leftPos;
            Vector3 rightPos;
            Quaternion leftRot;
            Quaternion rightRot;

            if (supportFirePoints != null && supportFirePoints.Length >= 2 && supportFirePoints[0] != null && supportFirePoints[1] != null)
            {
                leftPos = supportFirePoints[0].position;
                rightPos = supportFirePoints[1].position;
                leftRot = supportFirePoints[0].rotation * Quaternion.Euler(0f, 0f, weaponConfig.SupportSpreadAngle);
                rightRot = supportFirePoints[1].rotation * Quaternion.Euler(0f, 0f, -weaponConfig.SupportSpreadAngle);
            }
            else
            {
                leftPos = primary.position - primary.right * weaponConfig.SupportHorizontalOffset + primary.up * weaponConfig.SupportForwardOffset;
                rightPos = primary.position + primary.right * weaponConfig.SupportHorizontalOffset + primary.up * weaponConfig.SupportForwardOffset;
                leftRot = primary.rotation * Quaternion.Euler(0f, 0f, weaponConfig.SupportSpreadAngle);
                rightRot = primary.rotation * Quaternion.Euler(0f, 0f, -weaponConfig.SupportSpreadAngle);
            }

            int supportDamage = ComputeDamage(weaponConfig.SupportProjectileDamage);
            SpawnProjectile(leftPos, leftRot, supportDamage, weaponConfig.SupportProjectileSpeed * projectileSpeedMultiplier, true, Color.white, Vector3.one);
            SpawnProjectile(rightPos, rightRot, supportDamage, weaponConfig.SupportProjectileSpeed * projectileSpeedMultiplier, true, Color.white, Vector3.one);
            SpawnMuzzleFlash(leftPos, true);
            SpawnMuzzleFlash(rightPos, true);
        }

        private void SpawnSupportPair(Transform primary, int damage, float speed, float spread, Vector3 scale)
        {
            ResolveSupportSockets(primary, out Vector3 leftPos, out Vector3 rightPos, out Quaternion leftRot, out Quaternion rightRot);
            leftRot *= Quaternion.Euler(0f, 0f, spread);
            rightRot *= Quaternion.Euler(0f, 0f, -spread);
            SpawnProjectile(leftPos, leftRot, damage, speed, true, projectileTint, scale);
            SpawnProjectile(rightPos, rightRot, damage, speed, true, projectileTint, scale);
            SpawnMuzzleFlash(leftPos, true);
            SpawnMuzzleFlash(rightPos, true);
        }

        private void ResolveSupportSockets(Transform primary, out Vector3 leftPos, out Vector3 rightPos, out Quaternion leftRot, out Quaternion rightRot)
        {
            if (supportFirePoints != null && supportFirePoints.Length >= 2 && supportFirePoints[0] != null && supportFirePoints[1] != null)
            {
                leftPos = supportFirePoints[0].position;
                rightPos = supportFirePoints[1].position;
                leftRot = supportFirePoints[0].rotation;
                rightRot = supportFirePoints[1].rotation;
                return;
            }

            leftPos = primary.position - primary.right * weaponConfig.SupportHorizontalOffset + primary.up * weaponConfig.SupportForwardOffset;
            rightPos = primary.position + primary.right * weaponConfig.SupportHorizontalOffset + primary.up * weaponConfig.SupportForwardOffset;
            leftRot = primary.rotation;
            rightRot = primary.rotation;
        }

        private void SpawnPattern(Transform origin, float[] xOffsets, float[] angles, int damage, float speed, Color tint, Vector3 scale, bool supportShot)
        {
            for (int i = 0; i < xOffsets.Length; i++)
            {
                float angle = i < angles.Length ? angles[i] : 0f;
                SpawnOffsetProjectile(origin, xOffsets[i], angle, damage, speed, supportShot, tint, scale);
            }
        }

        private void SpawnOffsetProjectile(Transform origin, float horizontalOffset, float angle, int damage, float speed, bool supportShot, Color tint, Vector3 scale)
        {
            Vector3 position = origin.position + origin.right * horizontalOffset;
            Quaternion rotation = origin.rotation * Quaternion.Euler(0f, 0f, angle);
            SpawnProjectile(position, rotation, damage, speed, supportShot, tint, scale);
        }

        private void SpawnProjectile(Vector3 position, Quaternion rotation, int damage, float speed, bool supportShot, Color tint, Vector3 scale)
        {
            Projectile projectile = null;
            if (usePooling && ProjectilePool.Instance != null)
            {
                projectile = ProjectilePool.Instance.Spawn(projectilePrefab, position, rotation);
            }

            if (projectile == null)
            {
                projectile = Instantiate(projectilePrefab, position, rotation);
            }

            projectile.Setup(ownerFaction, damage, speed);
            if (ownerFaction == Faction.Player)
            {
                projectile.SetSourceProfile(playerArchetype, weaponLevel);
                Color finalTint = supportShot ? Color.Lerp(tint, Color.white, 0.18f) : tint;
                Vector3 finalScale = supportShot ? new Vector3(scale.x * 0.92f, scale.y * 0.94f, 1f) : scale;
                if (playerArchetype == ShipArchetype.Rapid && !supportShot)
                {
                    finalScale = new Vector3(finalScale.x * 0.9f, finalScale.y * 0.94f, 1f);
                }
                else if (playerArchetype == ShipArchetype.Heavy)
                {
                    finalScale = new Vector3(finalScale.x * 1.06f, finalScale.y * 1.12f, 1f);
                }
                projectile.SetVisualOverride(finalTint, finalScale);
            }
        }

        private void SpawnMuzzleFlash(Vector3 position, bool supportFlash)
        {
            GameObject flash = new GameObject(supportFlash ? "_SupportMuzzleFlash" : "_MuzzleFlash");
            flash.transform.position = position;
            flash.transform.localScale = Vector3.one * (supportFlash ? weaponConfig.MuzzleFlashSize * 1.25f : weaponConfig.MuzzleFlashSize * 1.45f);
            SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
            Sprite flashSprite = RuntimeArtLibrary.Get(RuntimeArtSpriteId.MuzzleFlash);
            renderer.sprite = flashSprite != null ? flashSprite : GeneratedSpriteLibrary.Get(GeneratedSpriteKind.Flash);
            Color mainColor = ownerFaction == Faction.Player ? Color.Lerp(projectileTint, Color.white, supportFlash ? 0.44f : 0.26f) : (supportFlash ? new Color(0.82f, 0.96f, 1f, 0.95f) : new Color(1f, 0.92f, 0.68f, 0.98f));
            renderer.color = mainColor;
            renderer.sortingOrder = 180;

            GameObject coreFlash = new GameObject("_MuzzleCore");
            coreFlash.transform.position = position;
            coreFlash.transform.localScale = Vector3.one * (supportFlash ? weaponConfig.MuzzleFlashSize * 0.55f : weaponConfig.MuzzleFlashSize * 0.72f);
            SpriteRenderer coreRenderer = coreFlash.AddComponent<SpriteRenderer>();
            coreRenderer.sprite = renderer.sprite;
            coreRenderer.color = Color.Lerp(mainColor, Color.white, 0.34f);
            coreRenderer.sortingOrder = 181;

            Object.Destroy(flash, Mathf.Max(0.02f, weaponConfig.MuzzleFlashDuration));
            Object.Destroy(coreFlash, Mathf.Max(0.02f, weaponConfig.MuzzleFlashDuration * 0.65f));
        }
    }
}

