using System;
using UnityEngine;

namespace ThunderFighter.Spawning
{
    [Serializable]
    public class WaveEntry
    {
        public GameObject enemyPrefab;
        public int count = 6;
        public float spawnInterval = 0.5f;
        public float startDelay = 0f;
    }
}
