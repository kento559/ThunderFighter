using UnityEngine;

namespace ThunderFighter.Combat
{
    public enum Faction
    {
        Player = 0,
        Enemy = 1,
        Neutral = 2
    }

    public struct DamageSource
    {
        public readonly GameObject Instigator;
        public readonly Faction Faction;

        public DamageSource(GameObject instigator, Faction faction)
        {
            Instigator = instigator;
            Faction = faction;
        }
    }
}
