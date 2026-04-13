using UnityEngine;

namespace ThunderFighter.Config
{
    public enum EnemyBehaviorType
    {
        Auto = 0,
        Straight = 1,
        Strafe = 2,
        Flank = 3,
        Dive = 4,
        Support = 5
    }

    [CreateAssetMenu(menuName = "ThunderFighter/Config/EnemyConfig", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [field: SerializeField] public float MoveSpeed { get; private set; } = 2.5f;
        [field: SerializeField] public int MaxHp { get; private set; } = 3;
        [field: SerializeField] public int ScoreValue { get; private set; } = 100;
        [field: SerializeField] public bool CanShoot { get; private set; } = true;
        [field: SerializeField] public float ShootInterval { get; private set; } = 1.4f;
        [field: SerializeField] public EnemyBehaviorType BehaviorType { get; private set; } = EnemyBehaviorType.Auto;
        [field: SerializeField] public float StrafeAmplitude { get; private set; } = 1.2f;
        [field: SerializeField] public float StrafeFrequency { get; private set; } = 2.2f;
    }
}
