namespace ThunderFighter.Core
{
    public enum ShipId
    {
        Balanced = 0,
        Rapid = 1,
        Heavy = 2
    }

    public enum ShipArchetype
    {
        Balanced = 0,
        Rapid = 1,
        Heavy = 2
    }

    public enum PickupKind
    {
        WeaponLevel = 0,
        Repair = 1,
        SkillEnergy = 2,
        FireRateBuff = 3,
        DamageBuff = 4,
        ProjectileSpeedBuff = 5,
        MagnetBuff = 6,
        GuardBuff = 7
    }

    public enum PickupBuffType
    {
        FireRate = 0,
        Damage = 1,
        ProjectileSpeed = 2,
        Magnet = 3,
        Guard = 4
    }

    [System.Serializable]
    public sealed class RuntimeBuffState
    {
        public PickupBuffType BuffType;
        public float RemainingSeconds;
        public float Magnitude;
        public int Stacks;
    }

    [System.Serializable]
    public sealed class PlayerRuntimeLoadout
    {
        public ShipId ShipId;
        public int WeaponLevel;

        public PlayerRuntimeLoadout(ShipId shipId)
        {
            ShipId = shipId;
            WeaponLevel = 1;
        }
    }
}
