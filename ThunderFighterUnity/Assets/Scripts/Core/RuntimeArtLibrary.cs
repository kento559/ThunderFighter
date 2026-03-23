using System.Collections.Generic;
using UnityEngine;

namespace ThunderFighter.Core
{
    public enum RuntimeArtSpriteId
    {
        PlayerShip = 0,
        EnemyShip = 1,
        BossShip = 2,
        PlayerBullet = 3,
        EnemyBullet = 4,
        MuzzleFlash = 5,
        PlayerShipDamaged = 6,
        EnemyShipDamaged = 7,
        BossShipDamaged = 8,
        PlayerFragmentA = 9,
        PlayerFragmentB = 10,
        EnemyFragmentA = 11,
        EnemyFragmentB = 12,
        BossFragmentA = 13,
        BossFragmentB = 14,
        EnemyShipElite = 15,
        EnemyShipEliteDamaged = 16,
        EnemyEliteFragmentA = 17,
        EnemyEliteFragmentB = 18,
        BossShipPhase2 = 19,
        BossShipPhase2Damaged = 20,
        BossPhase2FragmentA = 21,
        BossPhase2FragmentB = 22,
        SkillNovaIcon = 23,
        SkillOverdriveIcon = 24,
        SkillNovaCast = 25,
        SkillOverdriveCast = 26,
        PlayerShipBalanced = 27,
        PlayerShipBalancedDamaged = 28,
        PlayerShipRapid = 29,
        PlayerShipRapidDamaged = 30,
        PlayerShipHeavy = 31,
        PlayerShipHeavyDamaged = 32,
        PlayerBalancedFragmentA = 33,
        PlayerBalancedFragmentB = 34,
        PlayerRapidFragmentA = 35,
        PlayerRapidFragmentB = 36,
        PlayerHeavyFragmentA = 37,
        PlayerHeavyFragmentB = 38
    }

    public static class RuntimeArtLibrary
    {
        private static readonly Dictionary<RuntimeArtSpriteId, Sprite> Cache = new Dictionary<RuntimeArtSpriteId, Sprite>();

        public static Sprite Get(RuntimeArtSpriteId id)
        {
            if (Cache.TryGetValue(id, out Sprite sprite) && sprite != null)
            {
                return sprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(GetResourcePath(id));
            if (texture == null)
            {
                return null;
            }

            texture.filterMode = FilterMode.Bilinear;
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                GetPixelsPerUnit(id));
            sprite.name = id.ToString();
            Cache[id] = sprite;
            return sprite;
        }

        private static string GetResourcePath(RuntimeArtSpriteId id)
        {
            switch (id)
            {
                case RuntimeArtSpriteId.PlayerShip:
                    return "GeneratedArt/PlayerShip";
                case RuntimeArtSpriteId.EnemyShip:
                    return "GeneratedArt/EnemyShip";
                case RuntimeArtSpriteId.BossShip:
                    return "GeneratedArt/BossShip";
                case RuntimeArtSpriteId.PlayerBullet:
                    return "GeneratedArt/PlayerBullet";
                case RuntimeArtSpriteId.EnemyBullet:
                    return "GeneratedArt/EnemyBullet";
                case RuntimeArtSpriteId.MuzzleFlash:
                    return "GeneratedArt/MuzzleFlash";
                case RuntimeArtSpriteId.PlayerShipDamaged:
                    return "GeneratedArt/PlayerShip_Damaged";
                case RuntimeArtSpriteId.EnemyShipDamaged:
                    return "GeneratedArt/EnemyShip_Damaged";
                case RuntimeArtSpriteId.BossShipDamaged:
                    return "GeneratedArt/BossShip_Damaged";
                case RuntimeArtSpriteId.PlayerFragmentA:
                    return "GeneratedArt/PlayerFragmentA";
                case RuntimeArtSpriteId.PlayerFragmentB:
                    return "GeneratedArt/PlayerFragmentB";
                case RuntimeArtSpriteId.EnemyFragmentA:
                    return "GeneratedArt/EnemyFragmentA";
                case RuntimeArtSpriteId.EnemyFragmentB:
                    return "GeneratedArt/EnemyFragmentB";
                case RuntimeArtSpriteId.BossFragmentA:
                    return "GeneratedArt/BossFragmentA";
                case RuntimeArtSpriteId.BossFragmentB:
                    return "GeneratedArt/BossFragmentB";
                case RuntimeArtSpriteId.EnemyShipElite:
                    return "GeneratedArt/EnemyShip_Elite";
                case RuntimeArtSpriteId.EnemyShipEliteDamaged:
                    return "GeneratedArt/EnemyShip_Elite_Damaged";
                case RuntimeArtSpriteId.EnemyEliteFragmentA:
                    return "GeneratedArt/EnemyEliteFragmentA";
                case RuntimeArtSpriteId.EnemyEliteFragmentB:
                    return "GeneratedArt/EnemyEliteFragmentB";
                case RuntimeArtSpriteId.BossShipPhase2:
                    return "GeneratedArt/BossShip_Phase2";
                case RuntimeArtSpriteId.BossShipPhase2Damaged:
                    return "GeneratedArt/BossShip_Phase2_Damaged";
                case RuntimeArtSpriteId.BossPhase2FragmentA:
                    return "GeneratedArt/BossPhase2FragmentA";
                case RuntimeArtSpriteId.BossPhase2FragmentB:
                    return "GeneratedArt/BossPhase2FragmentB";
                case RuntimeArtSpriteId.SkillNovaIcon:
                    return "GeneratedArt/SkillNovaIcon";
                case RuntimeArtSpriteId.SkillOverdriveIcon:
                    return "GeneratedArt/SkillOverdriveIcon";
                case RuntimeArtSpriteId.SkillNovaCast:
                    return "GeneratedArt/SkillNovaCast";
                case RuntimeArtSpriteId.SkillOverdriveCast:
                    return "GeneratedArt/SkillOverdriveCast";
                case RuntimeArtSpriteId.PlayerShipBalanced:
                    return "GeneratedArt/PlayerShip_Balanced";
                case RuntimeArtSpriteId.PlayerShipBalancedDamaged:
                    return "GeneratedArt/PlayerShip_Balanced_Damaged";
                case RuntimeArtSpriteId.PlayerShipRapid:
                    return "GeneratedArt/PlayerShip_Rapid";
                case RuntimeArtSpriteId.PlayerShipRapidDamaged:
                    return "GeneratedArt/PlayerShip_Rapid_Damaged";
                case RuntimeArtSpriteId.PlayerShipHeavy:
                    return "GeneratedArt/PlayerShip_Heavy";
                case RuntimeArtSpriteId.PlayerShipHeavyDamaged:
                    return "GeneratedArt/PlayerShip_Heavy_Damaged";
                case RuntimeArtSpriteId.PlayerBalancedFragmentA:
                    return "GeneratedArt/PlayerBalancedFragmentA";
                case RuntimeArtSpriteId.PlayerBalancedFragmentB:
                    return "GeneratedArt/PlayerBalancedFragmentB";
                case RuntimeArtSpriteId.PlayerRapidFragmentA:
                    return "GeneratedArt/PlayerRapidFragmentA";
                case RuntimeArtSpriteId.PlayerRapidFragmentB:
                    return "GeneratedArt/PlayerRapidFragmentB";
                case RuntimeArtSpriteId.PlayerHeavyFragmentA:
                    return "GeneratedArt/PlayerHeavyFragmentA";
                case RuntimeArtSpriteId.PlayerHeavyFragmentB:
                    return "GeneratedArt/PlayerHeavyFragmentB";
                default:
                    return string.Empty;
            }
        }

        private static float GetPixelsPerUnit(RuntimeArtSpriteId id)
        {
            switch (id)
            {
                case RuntimeArtSpriteId.BossShip:
                case RuntimeArtSpriteId.BossShipDamaged:
                case RuntimeArtSpriteId.BossShipPhase2:
                case RuntimeArtSpriteId.BossShipPhase2Damaged:
                    return 340f;
                case RuntimeArtSpriteId.PlayerShip:
                case RuntimeArtSpriteId.EnemyShip:
                case RuntimeArtSpriteId.PlayerShipDamaged:
                case RuntimeArtSpriteId.EnemyShipDamaged:
                case RuntimeArtSpriteId.EnemyShipElite:
                case RuntimeArtSpriteId.EnemyShipEliteDamaged:
                case RuntimeArtSpriteId.PlayerShipBalanced:
                case RuntimeArtSpriteId.PlayerShipBalancedDamaged:
                case RuntimeArtSpriteId.PlayerShipRapid:
                case RuntimeArtSpriteId.PlayerShipRapidDamaged:
                case RuntimeArtSpriteId.PlayerShipHeavy:
                case RuntimeArtSpriteId.PlayerShipHeavyDamaged:
                    return 300f;
                case RuntimeArtSpriteId.PlayerBullet:
                case RuntimeArtSpriteId.EnemyBullet:
                    return 260f;
                case RuntimeArtSpriteId.MuzzleFlash:
                case RuntimeArtSpriteId.SkillNovaIcon:
                case RuntimeArtSpriteId.SkillOverdriveIcon:
                case RuntimeArtSpriteId.SkillNovaCast:
                case RuntimeArtSpriteId.SkillOverdriveCast:
                    return 220f;
                case RuntimeArtSpriteId.PlayerFragmentA:
                case RuntimeArtSpriteId.PlayerFragmentB:
                case RuntimeArtSpriteId.PlayerBalancedFragmentA:
                case RuntimeArtSpriteId.PlayerBalancedFragmentB:
                case RuntimeArtSpriteId.PlayerRapidFragmentA:
                case RuntimeArtSpriteId.PlayerRapidFragmentB:
                case RuntimeArtSpriteId.PlayerHeavyFragmentA:
                case RuntimeArtSpriteId.PlayerHeavyFragmentB:
                case RuntimeArtSpriteId.EnemyFragmentA:
                case RuntimeArtSpriteId.EnemyFragmentB:
                case RuntimeArtSpriteId.EnemyEliteFragmentA:
                case RuntimeArtSpriteId.EnemyEliteFragmentB:
                    return 260f;
                case RuntimeArtSpriteId.BossFragmentA:
                case RuntimeArtSpriteId.BossFragmentB:
                case RuntimeArtSpriteId.BossPhase2FragmentA:
                case RuntimeArtSpriteId.BossPhase2FragmentB:
                    return 300f;
                default:
                    return 200f;
            }
        }

        public static RuntimeArtSpriteId GetPlayerShipSpriteId(ShipId shipId)
        {
            switch (shipId)
            {
                case ShipId.Rapid:
                    return RuntimeArtSpriteId.PlayerShipRapid;
                case ShipId.Heavy:
                    return RuntimeArtSpriteId.PlayerShipHeavy;
                default:
                    return RuntimeArtSpriteId.PlayerShipBalanced;
            }
        }

        public static RuntimeArtSpriteId GetPlayerShipDamagedSpriteId(ShipId shipId)
        {
            switch (shipId)
            {
                case ShipId.Rapid:
                    return RuntimeArtSpriteId.PlayerShipRapidDamaged;
                case ShipId.Heavy:
                    return RuntimeArtSpriteId.PlayerShipHeavyDamaged;
                default:
                    return RuntimeArtSpriteId.PlayerShipBalancedDamaged;
            }
        }

        public static RuntimeArtSpriteId[] GetPlayerFragmentIds(ShipId shipId)
        {
            switch (shipId)
            {
                case ShipId.Rapid:
                    return new[] { RuntimeArtSpriteId.PlayerRapidFragmentA, RuntimeArtSpriteId.PlayerRapidFragmentB };
                case ShipId.Heavy:
                    return new[] { RuntimeArtSpriteId.PlayerHeavyFragmentA, RuntimeArtSpriteId.PlayerHeavyFragmentB };
                default:
                    return new[] { RuntimeArtSpriteId.PlayerBalancedFragmentA, RuntimeArtSpriteId.PlayerBalancedFragmentB };
            }
        }
    }
}
