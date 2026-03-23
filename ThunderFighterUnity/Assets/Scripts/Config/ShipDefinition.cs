using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Config
{
    [CreateAssetMenu(menuName = "ThunderFighter/Config/ShipDefinition", fileName = "ShipDefinition")]
    public class ShipDefinition : ScriptableObject
    {
        public ShipId ShipId = ShipId.Balanced;
        public ShipArchetype Archetype = ShipArchetype.Balanced;
        public string DisplayNameEnglish = "Vanguard";
        public string DisplayNameChinese = "先锋型";
        public string SubtitleEnglish = "Balanced assault frame";
        public string SubtitleChinese = "均衡突击机体";
        public Color AccentColor = new Color(0.48f, 0.88f, 1f, 1f);
        public Color ShipTint = Color.white;
        public Color ProjectileTint = Color.white;
        public Vector3 ProjectileScale = Vector3.one;
        public float MoveSpeedMultiplier = 1f;
        public int MaxHpBonus = 0;
        public float PrimaryIntervalMultiplier = 1f;
        public float SupportIntervalMultiplier = 1f;
        public float ProjectileSpeedMultiplier = 1f;
        public int DamageBonus = 0;
        public bool SupportCannonsEnabled = true;
        public string SkillOneEnglish = "Plasma Nova";
        public string SkillOneChinese = "等离子新星";
        public string SkillTwoEnglish = "Overdrive";
        public string SkillTwoChinese = "过载";
        public float SkillOneCost = 40f;
        public float SkillTwoCost = 60f;
        public float SkillOnePower = 1f;
        public float SkillTwoPower = 1f;
        public float SkillTwoDuration = 4.5f;

        public string GetDisplayName(bool chinese)
        {
            return chinese ? DisplayNameChinese : DisplayNameEnglish;
        }

        public string GetSubtitle(bool chinese)
        {
            return chinese ? SubtitleChinese : SubtitleEnglish;
        }

        public string GetSkillOneName(bool chinese)
        {
            return chinese ? SkillOneChinese : SkillOneEnglish;
        }

        public string GetSkillTwoName(bool chinese)
        {
            return chinese ? SkillTwoChinese : SkillTwoEnglish;
        }
    }
}
