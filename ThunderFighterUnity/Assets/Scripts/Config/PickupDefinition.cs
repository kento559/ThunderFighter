using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Config
{
    [CreateAssetMenu(menuName = "ThunderFighter/Config/PickupDefinition", fileName = "PickupDefinition")]
    public class PickupDefinition : ScriptableObject
    {
        public PickupKind Kind = PickupKind.WeaponLevel;
        public string DisplayNameEnglish = "Power Core";
        public string DisplayNameChinese = "火力核心";
        public string ShortEnglish = "+WP";
        public string ShortChinese = "+火力";
        public GeneratedSpriteKind IconKind = GeneratedSpriteKind.Ring;
        public Color AccentColor = new Color(0.54f, 0.94f, 1f, 1f);
        public float DurationSeconds = 10f;
        public float Magnitude = 0f;
        public int IntValue = 0;

        public string GetDisplayName(bool chinese)
        {
            return chinese ? DisplayNameChinese : DisplayNameEnglish;
        }

        public string GetShortLabel(bool chinese)
        {
            return chinese ? ShortChinese : ShortEnglish;
        }
    }
}
