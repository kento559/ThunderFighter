using ThunderFighter.Combat;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Boss
{
    public class BossDamageNode : MonoBehaviour, IDamageable
    {
        private BossController owner;
        private bool weakPoint;
        public bool IsWeakPoint => weakPoint;

        public void Initialize(BossController controller, bool isWeakPoint)
        {
            owner = controller;
            weakPoint = isWeakPoint;
        }

        public void TakeDamage(int amount, DamageSource source)
        {
            if (owner == null)
            {
                return;
            }

            owner.ProcessNodeHit(amount, source, weakPoint, transform.position);
        }
    }
}
