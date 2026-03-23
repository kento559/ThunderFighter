namespace ThunderFighter.Combat
{
    public interface IDamageable
    {
        void TakeDamage(int amount, DamageSource source);
    }
}
