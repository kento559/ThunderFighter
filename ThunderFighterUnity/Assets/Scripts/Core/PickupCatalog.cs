using System.Collections.Generic;
using ThunderFighter.Config;
using UnityEngine;

namespace ThunderFighter.Core
{
    public static class PickupCatalog
    {
        private static readonly Dictionary<PickupKind, PickupDefinition> RuntimeDefinitions = new Dictionary<PickupKind, PickupDefinition>();
        private static bool initialized;

        public static PickupDefinition Get(PickupKind kind)
        {
            EnsureInitialized();
            RuntimeDefinitions.TryGetValue(kind, out PickupDefinition definition);
            return definition;
        }

        public static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            RuntimeDefinitions.Clear();
            PickupDefinition[] loaded = Resources.LoadAll<PickupDefinition>("Pickups");
            for (int i = 0; i < loaded.Length; i++)
            {
                if (loaded[i] != null)
                {
                    RuntimeDefinitions[loaded[i].Kind] = loaded[i];
                }
            }

            EnsureDefinition(Create(PickupKind.WeaponLevel, "Power Core", "火力核心", "+WP", "+火力", GeneratedSpriteKind.Cockpit, new Color(0.52f, 0.9f, 1f, 1f), 0f, 1f, 1));
            EnsureDefinition(Create(PickupKind.Repair, "Repair Cell", "修复单元", "+HP", "+生命", GeneratedSpriteKind.Engine, new Color(0.58f, 1f, 0.74f, 1f), 0f, 0f, 24));
            EnsureDefinition(Create(PickupKind.SkillEnergy, "Energy Cell", "能量电池", "+EN", "+能量", GeneratedSpriteKind.Ring, new Color(1f, 0.9f, 0.42f, 1f), 0f, 0f, 28));
            EnsureDefinition(Create(PickupKind.FireRateBuff, "Rapid Chip", "速射芯片", "+ROF", "+攻速", GeneratedSpriteKind.Flash, new Color(0.8f, 0.94f, 1f, 1f), 10f, 0.2f, 0));
            EnsureDefinition(Create(PickupKind.DamageBuff, "Warhead Core", "战斗弹芯", "+DMG", "+伤害", GeneratedSpriteKind.Nose, new Color(1f, 0.64f, 0.36f, 1f), 10f, 0.35f, 0));
            EnsureDefinition(Create(PickupKind.ProjectileSpeedBuff, "Velocity Chip", "极速芯片", "+SPD", "+弹速", GeneratedSpriteKind.Bullet, new Color(0.7f, 0.9f, 1f, 1f), 10f, 0.25f, 0));
            EnsureDefinition(Create(PickupKind.MagnetBuff, "Magnet Field", "吸附力场", "+MAG", "+吸附", GeneratedSpriteKind.Wing, new Color(0.82f, 0.88f, 1f, 1f), 12f, 1.2f, 0));
            EnsureDefinition(Create(PickupKind.GuardBuff, "Guard Matrix", "防御矩阵", "+DEF", "+减伤", GeneratedSpriteKind.Engine, new Color(0.48f, 0.92f, 1f, 1f), 10f, 0.35f, 0));
        }

        private static void EnsureDefinition(PickupDefinition definition)
        {
            if (!RuntimeDefinitions.ContainsKey(definition.Kind))
            {
                RuntimeDefinitions[definition.Kind] = definition;
            }
        }

        private static PickupDefinition Create(PickupKind kind, string en, string zh, string shortEn, string shortZh, GeneratedSpriteKind iconKind, Color accent, float duration, float magnitude, int intValue)
        {
            PickupDefinition definition = ScriptableObject.CreateInstance<PickupDefinition>();
            definition.name = "Pickup_" + kind;
            definition.Kind = kind;
            definition.DisplayNameEnglish = en;
            definition.DisplayNameChinese = zh;
            definition.ShortEnglish = shortEn;
            definition.ShortChinese = shortZh;
            definition.IconKind = iconKind;
            definition.AccentColor = accent;
            definition.DurationSeconds = duration;
            definition.Magnitude = magnitude;
            definition.IntValue = intValue;
            return definition;
        }
    }
}
