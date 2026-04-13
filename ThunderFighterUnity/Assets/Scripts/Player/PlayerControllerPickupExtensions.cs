using System.Reflection;
using ThunderFighter.Combat;
using ThunderFighter.Config;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Player
{
    public static class PlayerControllerPickupExtensions
    {
        public static float GetPickupMagnetMultiplier(this PlayerController player)
        {
            if (player == null)
            {
                return 1f;
            }

            PlayerBuffController buffs = player.GetComponent<PlayerBuffController>();
            return buffs != null ? buffs.MagnetRadiusMultiplier : 1f;
        }

        public static void ApplyPickup(this PlayerController player, PickupDefinition definition)
        {
            if (player == null || definition == null)
            {
                return;
            }

            HealthComponent health = player.GetComponent<HealthComponent>();
            PlayerBuffController buffs = player.GetComponent<PlayerBuffController>() ?? player.gameObject.AddComponent<PlayerBuffController>();
            WeaponController weapon = player.GetComponent<WeaponController>();
            System.Type type = player.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            FieldInfo runtimeLoadoutField = type.GetField("runtimeLoadout", flags);
            PlayerRuntimeLoadout loadout = runtimeLoadoutField != null ? runtimeLoadoutField.GetValue(player) as PlayerRuntimeLoadout : CampaignRuntime.CurrentLoadout;
            if (loadout == null)
            {
                loadout = new PlayerRuntimeLoadout(CampaignProgressService.GetSelectedShipId());
                CampaignRuntime.CurrentLoadout = loadout;
                runtimeLoadoutField?.SetValue(player, loadout);
            }

            FieldInfo skillEnergyField = type.GetField("skillEnergy", flags);
            MethodInfo getSkillEnergyMax = type.GetMethod("GetSkillEnergyMax", flags);
            MethodInfo publishSkill = type.GetMethod("PublishSkillEnergy", flags);
            MethodInfo publishLoadout = type.GetMethod("PublishLoadout", flags);
            MethodInfo publishBuffSummary = type.GetMethod("PublishBuffSummary", flags);
            MethodInfo applyRuntimeWeaponState = type.GetMethod("ApplyRuntimeWeaponState", flags);
            MethodInfo spawnPickupFlash = type.GetMethod("SpawnPickupFlash", flags);

            float skillEnergy = skillEnergyField != null ? (float)skillEnergyField.GetValue(player) : 0f;
            float maxEnergy = getSkillEnergyMax != null ? (float)getSkillEnergyMax.Invoke(player, null) : 100f;

            switch (definition.Kind)
            {
                case PickupKind.WeaponLevel:
                    if (loadout.WeaponLevel < 4)
                    {
                        loadout.WeaponLevel++;
                        weapon?.SetWeaponLevel(loadout.WeaponLevel);
                        publishLoadout?.Invoke(player, null);
                    }
                    else
                    {
                        health?.RestoreHp(10);
                        skillEnergy = Mathf.Min(maxEnergy, skillEnergy + 10f);
                    }
                    break;
                case PickupKind.Repair:
                    health?.RestoreHp(definition.IntValue);
                    break;
                case PickupKind.SkillEnergy:
                    skillEnergy = Mathf.Min(maxEnergy, skillEnergy + definition.IntValue);
                    break;
                case PickupKind.FireRateBuff:
                    buffs.AddOrRefresh(PickupBuffType.FireRate, definition.DurationSeconds, definition.Magnitude);
                    break;
                case PickupKind.DamageBuff:
                    buffs.AddOrRefresh(PickupBuffType.Damage, definition.DurationSeconds, definition.Magnitude);
                    break;
                case PickupKind.ProjectileSpeedBuff:
                    buffs.AddOrRefresh(PickupBuffType.ProjectileSpeed, definition.DurationSeconds, definition.Magnitude);
                    break;
                case PickupKind.MagnetBuff:
                    buffs.AddOrRefresh(PickupBuffType.Magnet, definition.DurationSeconds, definition.Magnitude);
                    break;
                case PickupKind.GuardBuff:
                    buffs.AddOrRefresh(PickupBuffType.Guard, definition.DurationSeconds, definition.Magnitude);
                    break;
            }

            if (skillEnergyField != null)
            {
                skillEnergyField.SetValue(player, skillEnergy);
            }

            publishSkill?.Invoke(player, null);
            publishBuffSummary?.Invoke(player, null);
            applyRuntimeWeaponState?.Invoke(player, null);
            spawnPickupFlash?.Invoke(player, new object[] { definition.AccentColor });
            GameEvents.RaisePickupCollected(definition.GetDisplayName(LocalizationService.IsChinese));
        }
    }
}
