using UnityEngine;

namespace ThunderFighter.Config
{
    [CreateAssetMenu(menuName = "ThunderFighter/Config/PlayerConfig", fileName = "PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [field: SerializeField] public float MoveSpeed { get; private set; } = 6f;
        [field: SerializeField] public float DashSpeed { get; private set; } = 18f;
        [field: SerializeField] public float DashDuration { get; private set; } = 0.16f;
        [field: SerializeField] public float DashCooldown { get; private set; } = 0.55f;
        [field: SerializeField] public float DashInvincibleDuration { get; private set; } = 0.2f;
        [field: SerializeField] public Vector2 MoveBoundsX { get; private set; } = new Vector2(-8f, 8f);
        [field: SerializeField] public Vector2 MoveBoundsY { get; private set; } = new Vector2(-4.2f, 4.2f);
        [field: SerializeField] public float SkillEnergyMax { get; private set; } = 100f;
        [field: SerializeField] public float SkillEnergyRegenPerSecond { get; private set; } = 15f;
        [field: SerializeField] public float NovaCost { get; private set; } = 40f;
        [field: SerializeField] public float OverdriveCost { get; private set; } = 60f;
        [field: SerializeField] public float OverdriveDuration { get; private set; } = 4.5f;
        [field: SerializeField] public float OverdriveFireRateMultiplier { get; private set; } = 0.58f;
        [field: SerializeField] public float OverdriveProjectileSpeedMultiplier { get; private set; } = 1.25f;
        [field: SerializeField] public float NovaRadius { get; private set; } = 4.6f;
        [field: SerializeField] public int NovaDamage { get; private set; } = 4;
    }
}
