using UnityEngine;

namespace ThunderFighter.Config
{
    [CreateAssetMenu(menuName = "ThunderFighter/Config/WeaponConfig", fileName = "WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [field: SerializeField] public float FireInterval { get; private set; } = 0.12f;
        [field: SerializeField] public int ProjectileDamage { get; private set; } = 1;
        [field: SerializeField] public float ProjectileSpeed { get; private set; } = 14f;
        [field: SerializeField] public bool SupportCannonsEnabled { get; private set; } = true;
        [field: SerializeField] public float SupportFireInterval { get; private set; } = 0.24f;
        [field: SerializeField] public int SupportProjectileDamage { get; private set; } = 1;
        [field: SerializeField] public float SupportProjectileSpeed { get; private set; } = 13f;
        [field: SerializeField] public float SupportHorizontalOffset { get; private set; } = 0.5f;
        [field: SerializeField] public float SupportForwardOffset { get; private set; } = -0.05f;
        [field: SerializeField] public float SupportSpreadAngle { get; private set; } = 8f;
        [field: SerializeField] public float MuzzleFlashSize { get; private set; } = 0.24f;
        [field: SerializeField] public float MuzzleFlashDuration { get; private set; } = 0.06f;
    }
}
