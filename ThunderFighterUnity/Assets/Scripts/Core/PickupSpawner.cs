using ThunderFighter.Combat;
using ThunderFighter.Config;
using ThunderFighter.Player;
using UnityEngine;

namespace ThunderFighter.Core
{
    public static class PickupSpawner
    {
        public static void SpawnEnemyDrop(Vector3 position, bool elite, int chapterIndex, EnemyBehaviorType behaviorType = EnemyBehaviorType.Auto)
        {
            PickupCatalog.EnsureInitialized();
            float roll = Random.value;
            if (!elite)
            {
                if (roll > Mathf.Lerp(0.42f, 0.55f, Mathf.InverseLerp(1f, 3f, chapterIndex)))
                {
                    return;
                }

                PickupKind kind = roll switch
                {
                    < 0.16f => PickupKind.Repair,
                    < 0.3f => PickupKind.SkillEnergy,
                    _ => PickupKind.WeaponLevel
                };
                SpawnPickup(position, PickupCatalog.Get(kind), false, kind == PickupKind.WeaponLevel);
                return;
            }

            if (behaviorType == EnemyBehaviorType.Flank)
            {
                PickupKind flankKind = chapterIndex >= 2
                    ? (roll < 0.55f ? PickupKind.MagnetBuff : PickupKind.FireRateBuff)
                    : (roll < 0.5f ? PickupKind.FireRateBuff : PickupKind.WeaponLevel);
                SpawnPickup(position, PickupCatalog.Get(flankKind), false, flankKind == PickupKind.WeaponLevel);
                return;
            }

            if (behaviorType == EnemyBehaviorType.Dive)
            {
                PickupKind diveKind = chapterIndex >= 3
                    ? (roll < 0.48f ? PickupKind.DamageBuff : PickupKind.ProjectileSpeedBuff)
                    : (roll < 0.5f ? PickupKind.ProjectileSpeedBuff : PickupKind.WeaponLevel);
                SpawnPickup(position, PickupCatalog.Get(diveKind), false, diveKind == PickupKind.WeaponLevel);
                return;
            }

            if (behaviorType == EnemyBehaviorType.Support)
            {
                PickupKind supportKind = chapterIndex >= 3
                    ? (roll < 0.45f ? PickupKind.GuardBuff : PickupKind.MagnetBuff)
                    : (roll < 0.5f ? PickupKind.SkillEnergy : PickupKind.FireRateBuff);
                SpawnPickup(position, PickupCatalog.Get(supportKind), false, supportKind == PickupKind.WeaponLevel);
                return;
            }

            PickupKind eliteKind = chapterIndex switch
            {
                1 => (roll < 0.45f ? PickupKind.WeaponLevel : PickupKind.FireRateBuff),
                2 => (roll < 0.34f ? PickupKind.WeaponLevel : (roll < 0.66f ? PickupKind.DamageBuff : PickupKind.ProjectileSpeedBuff)),
                _ => (roll < 0.28f ? PickupKind.WeaponLevel : (roll < 0.58f ? PickupKind.GuardBuff : PickupKind.MagnetBuff))
            };
            SpawnPickup(position, PickupCatalog.Get(eliteKind), false, true);
        }

        public static void SpawnBossDrops(Vector3 position, int chapterIndex)
        {
            PickupCatalog.EnsureInitialized();
            SpawnPickup(position + new Vector3(-0.8f, 0.1f, 0f), PickupCatalog.Get(PickupKind.WeaponLevel), true, true);
            SpawnPickup(position + new Vector3(0f, -0.15f, 0f), PickupCatalog.Get(PickupKind.SkillEnergy), true, true);
            SpawnPickup(position + new Vector3(0.8f, 0.1f, 0f), PickupCatalog.Get(chapterIndex >= 3 ? PickupKind.GuardBuff : PickupKind.DamageBuff), true, true);
        }

        public static PickupItem SpawnPickup(Vector3 position, PickupDefinition definition, bool bossDrop = false, bool premiumDrop = false)
        {
            if (definition == null)
            {
                return null;
            }

            GameObject go = new GameObject("Pickup_" + definition.Kind);
            go.transform.position = position;
            PickupItem item = go.AddComponent<PickupItem>();
            item.Initialize(definition);
            item.ConfigureFlightProfile(bossDrop, premiumDrop || definition.Kind == PickupKind.WeaponLevel);
            return item;
        }
    }
}
