using System.Collections.Generic;
using UnityEngine;

namespace ThunderFighter.Spawning
{
    [CreateAssetMenu(menuName = "ThunderFighter/Config/WaveConfig", fileName = "WaveConfig")]
    public class WaveConfig : ScriptableObject
    {
        [field: SerializeField] public List<WaveEntry> Waves { get; private set; } = new List<WaveEntry>();
        [field: SerializeField] public float BossSpawnDelayAfterLastWave { get; private set; } = 3f;
    }
}
