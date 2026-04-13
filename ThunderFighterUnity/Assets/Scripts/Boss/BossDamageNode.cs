using ThunderFighter.Combat;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Boss
{
    public enum BossNodeType
    {
        Shield = 0,
        Core = 1,
        LeftBattery = 2,
        RightBattery = 3
    }

    public class BossDamageNode : MonoBehaviour, IDamageable
    {
        private BossController owner;
        private BossNodeType nodeType;
        public bool IsWeakPoint => nodeType == BossNodeType.Core;
        public BossNodeType NodeType => nodeType;

        public void Initialize(BossController controller, BossNodeType type)
        {
            owner = controller;
            nodeType = type;
        }

        public void TakeDamage(int amount, DamageSource source)
        {
            if (owner == null)
            {
                return;
            }

            owner.ProcessNodeHit(amount, source, nodeType, transform.position);
        }
    }
}
