using System.Collections.Generic;
using ThunderFighter.Config;
using UnityEngine;

namespace ThunderFighter.Core
{
    public static class ShipCatalog
    {
        private static readonly List<ShipDefinition> RuntimeShips = new List<ShipDefinition>();
        private static bool initialized;

        public static IReadOnlyList<ShipDefinition> GetAll()
        {
            EnsureInitialized();
            return RuntimeShips;
        }

        public static ShipDefinition GetSelected()
        {
            return GetById(CampaignProgressService.GetSelectedShipId()) ?? GetById(ShipId.Balanced);
        }

        public static ShipDefinition GetById(ShipId shipId)
        {
            EnsureInitialized();
            for (int i = 0; i < RuntimeShips.Count; i++)
            {
                if (RuntimeShips[i] != null && RuntimeShips[i].ShipId == shipId)
                {
                    return RuntimeShips[i];
                }
            }

            return null;
        }

        private static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            RuntimeShips.Clear();
            ShipDefinition[] loaded = Resources.LoadAll<ShipDefinition>("Ships");
            if (loaded != null && loaded.Length > 0)
            {
                RuntimeShips.AddRange(loaded);
            }

            if (RuntimeShips.Count == 0)
            {
                RuntimeShips.Add(CreateBalancedShip());
                RuntimeShips.Add(CreateRapidShip());
                RuntimeShips.Add(CreateHeavyShip());
            }
        }

        private static ShipDefinition CreateBalancedShip()
        {
            ShipDefinition ship = ScriptableObject.CreateInstance<ShipDefinition>();
            ship.name = "Ship_Balanced";
            ship.ShipId = ShipId.Balanced;
            ship.Archetype = ShipArchetype.Balanced;
            ship.DisplayNameEnglish = "Vanguard";
            ship.DisplayNameChinese = "先锋型";
            ship.SubtitleEnglish = "Balanced assault frame";
            ship.SubtitleChinese = "均衡突击机体";
            ship.AccentColor = new Color(0.5f, 0.9f, 1f, 1f);
            ship.ShipTint = Color.white;
            ship.ProjectileTint = new Color(0.72f, 0.96f, 1f, 1f);
            ship.ProjectileScale = new Vector3(1f, 1.02f, 1f);
            ship.MoveSpeedMultiplier = 1f;
            ship.MaxHpBonus = 0;
            ship.PrimaryIntervalMultiplier = 1f;
            ship.SupportIntervalMultiplier = 1f;
            ship.ProjectileSpeedMultiplier = 1f;
            ship.DamageBonus = 0;
            ship.SupportCannonsEnabled = true;
            ship.SkillOneEnglish = "Plasma Nova";
            ship.SkillOneChinese = "等离子新星";
            ship.SkillTwoEnglish = "Overdrive";
            ship.SkillTwoChinese = "过载";
            ship.SkillOneCost = 40f;
            ship.SkillTwoCost = 60f;
            ship.SkillOnePower = 1f;
            ship.SkillTwoPower = 1f;
            ship.SkillTwoDuration = 4.5f;
            return ship;
        }

        private static ShipDefinition CreateRapidShip()
        {
            ShipDefinition ship = ScriptableObject.CreateInstance<ShipDefinition>();
            ship.name = "Ship_Rapid";
            ship.ShipId = ShipId.Rapid;
            ship.Archetype = ShipArchetype.Rapid;
            ship.DisplayNameEnglish = "Tempest";
            ship.DisplayNameChinese = "暴雨型";
            ship.SubtitleEnglish = "Rapid-fire interception frame";
            ship.SubtitleChinese = "高速连射截击机体";
            ship.AccentColor = new Color(1f, 0.76f, 0.34f, 1f);
            ship.ShipTint = new Color(1f, 0.96f, 0.92f, 1f);
            ship.ProjectileTint = new Color(1f, 0.84f, 0.42f, 1f);
            ship.ProjectileScale = new Vector3(0.88f, 0.94f, 1f);
            ship.MoveSpeedMultiplier = 1.12f;
            ship.MaxHpBonus = -10;
            ship.PrimaryIntervalMultiplier = 0.76f;
            ship.SupportIntervalMultiplier = 0.78f;
            ship.ProjectileSpeedMultiplier = 1.08f;
            ship.DamageBonus = -1;
            ship.SupportCannonsEnabled = true;
            ship.SkillOneEnglish = "Blade Storm";
            ship.SkillOneChinese = "疾风弹幕";
            ship.SkillTwoEnglish = "Hyperdrive";
            ship.SkillTwoChinese = "超速推进";
            ship.SkillOneCost = 36f;
            ship.SkillTwoCost = 54f;
            ship.SkillOnePower = 1.2f;
            ship.SkillTwoPower = 1.2f;
            ship.SkillTwoDuration = 4.2f;
            return ship;
        }

        private static ShipDefinition CreateHeavyShip()
        {
            ShipDefinition ship = ScriptableObject.CreateInstance<ShipDefinition>();
            ship.name = "Ship_Heavy";
            ship.ShipId = ShipId.Heavy;
            ship.Archetype = ShipArchetype.Heavy;
            ship.DisplayNameEnglish = "Bulwark";
            ship.DisplayNameChinese = "壁垒型";
            ship.SubtitleEnglish = "Heavy cannon siege frame";
            ship.SubtitleChinese = "重炮攻城机体";
            ship.AccentColor = new Color(1f, 0.44f, 0.22f, 1f);
            ship.ShipTint = new Color(1f, 0.94f, 0.9f, 1f);
            ship.ProjectileTint = new Color(1f, 0.64f, 0.34f, 1f);
            ship.ProjectileScale = new Vector3(1.18f, 1.26f, 1f);
            ship.MoveSpeedMultiplier = 0.88f;
            ship.MaxHpBonus = 18;
            ship.PrimaryIntervalMultiplier = 1.28f;
            ship.SupportIntervalMultiplier = 1.18f;
            ship.ProjectileSpeedMultiplier = 0.94f;
            ship.DamageBonus = 2;
            ship.SupportCannonsEnabled = true;
            ship.SkillOneEnglish = "Siege Breaker";
            ship.SkillOneChinese = "破阵重炮";
            ship.SkillTwoEnglish = "Aegis Core";
            ship.SkillTwoChinese = "壁垒核心";
            ship.SkillOneCost = 46f;
            ship.SkillTwoCost = 58f;
            ship.SkillOnePower = 1.35f;
            ship.SkillTwoPower = 1.15f;
            ship.SkillTwoDuration = 4.8f;
            return ship;
        }
    }
}
