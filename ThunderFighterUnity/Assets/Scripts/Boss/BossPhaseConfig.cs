using UnityEngine;

namespace ThunderFighter.Boss
{
    [CreateAssetMenu(menuName = "ThunderFighter/Config/BossPhaseConfig", fileName = "BossPhaseConfig")]
    public class BossPhaseConfig : ScriptableObject
    {
        [field: SerializeField] public float TriggerHpRatio { get; private set; } = 1f;
        [field: SerializeField] public float MoveSpeed { get; private set; } = 1.8f;
        [field: SerializeField] public float FireInterval { get; private set; } = 0.45f;
        [field: SerializeField] public int BulletsPerShot { get; private set; } = 3;
        [field: SerializeField] public float SpreadAngle { get; private set; } = 22f;
    }
}
