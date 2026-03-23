using ThunderFighter.Combat;
using ThunderFighter.Config;
using ThunderFighter.Player;
using UnityEngine;

namespace ThunderFighter.Core
{
    public static class PickupSpawner
    {
        public static void SpawnEnemyDrop(Vector3 position, bool elite, int chapterIndex)
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
                SpawnPickup(position, PickupCatalog.Get(kind));
                return;
            }

            PickupKind eliteKind = chapterIndex switch
            {
                1 => (roll < 0.45f ? PickupKind.WeaponLevel : PickupKind.FireRateBuff),
                2 => (roll < 0.34f ? PickupKind.WeaponLevel : (roll < 0.66f ? PickupKind.DamageBuff : PickupKind.ProjectileSpeedBuff)),
                _ => (roll < 0.28f ? PickupKind.WeaponLevel : (roll < 0.58f ? PickupKind.GuardBuff : PickupKind.MagnetBuff))
            };
            SpawnPickup(position, PickupCatalog.Get(eliteKind));
        }

        public static void SpawnBossDrops(Vector3 position, int chapterIndex)
        {
            PickupCatalog.EnsureInitialized();
            SpawnPickup(position + new Vector3(-0.8f, 0.1f, 0f), PickupCatalog.Get(PickupKind.WeaponLevel));
            SpawnPickup(position + new Vector3(0f, -0.15f, 0f), PickupCatalog.Get(PickupKind.SkillEnergy));
            SpawnPickup(position + new Vector3(0.8f, 0.1f, 0f), PickupCatalog.Get(chapterIndex >= 3 ? PickupKind.GuardBuff : PickupKind.DamageBuff));
        }

        public static PickupItem SpawnPickup(Vector3 position, PickupDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            GameObject go = new GameObject("Pickup_" + definition.Kind);
            go.transform.position = position;
            PickupItem item = go.AddComponent<PickupItem>();
            item.Initialize(definition);
            return item;
        }
    }
}
