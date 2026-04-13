using ThunderFighter.Combat;
using UnityEngine;

namespace ThunderFighter.Enemy
{
    [RequireComponent(typeof(Collider2D))]
    public class RamDamage : MonoBehaviour
    {
        [SerializeField] private int collisionDamage = 1;
        [SerializeField] private Faction faction = Faction.Enemy;

        private void OnTriggerEnter2D(Collider2D other)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            damageable.TakeDamage(collisionDamage, new DamageSource(gameObject, faction));
        }
    }
}
