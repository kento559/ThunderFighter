using System;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Combat
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private Faction faction = Faction.Enemy;
        [SerializeField] private int maxHp = 10;
        [SerializeField] private float invincibleSecondsAfterHit = 0.1f;
        [SerializeField] private bool notifyPlayerHpEvents;
        [SerializeField] private bool notifyBossHpEvents;
        [SerializeField] private bool notifyDeathAsPlayer;
        [SerializeField] private bool notifyDeathAsBoss;

        public int CurrentHp { get; private set; }
        public int MaxHp => maxHp;
        public Faction Faction => faction;

        public event Action OnDied;
        public event Action<int, int> OnHpChanged;
        public event Action<int, int> OnDamaged;

        private float invincibleUntil;
        private bool isDead;

        private void Awake()
        {
            CurrentHp = maxHp;
        }

        private void Start()
        {
            PublishHp();
        }

        public void TakeDamage(int amount, DamageSource source)
        {
            if (isDead || amount <= 0)
            {
                return;
            }

            if (Time.time < invincibleUntil)
            {
                return;
            }

            if (source.Faction == faction && source.Faction != Faction.Neutral)
            {
                return;
            }

            Player.PlayerBuffController playerBuffs = GetComponent<Player.PlayerBuffController>();
            if (playerBuffs != null)
            {
                amount = playerBuffs.ModifyIncomingDamage(amount);
            }

            CurrentHp = Mathf.Max(CurrentHp - amount, 0);
            invincibleUntil = Time.time + invincibleSecondsAfterHit;
            OnDamaged?.Invoke(amount, CurrentHp);
            PublishHp();

            if (notifyPlayerHpEvents)
            {
                GameEvents.RaisePlayerDamaged(CurrentHp, MaxHp);
            }

            if (CurrentHp == 0)
            {
                isDead = true;
                OnDied?.Invoke();

                if (notifyDeathAsPlayer)
                {
                    GameEvents.RaisePlayerDied();
                }

                if (notifyDeathAsBoss)
                {
                    GameEvents.RaiseBossDied();
                }
            }
        }

        public void RestoreFull()
        {
            isDead = false;
            CurrentHp = maxHp;
            PublishHp();
        }

        public void RestoreHp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            isDead = false;
            CurrentHp = Mathf.Clamp(CurrentHp + amount, 0, maxHp);
            PublishHp();
        }

        public void GrantInvincibility(float duration)
        {
            invincibleUntil = Mathf.Max(invincibleUntil, Time.time + Mathf.Max(0f, duration));
        }

        public void ForceSetMaxHp(int value, bool restoreFull)
        {
            maxHp = Mathf.Max(1, value);
            if (restoreFull)
            {
                RestoreFull();
            }
            else
            {
                CurrentHp = Mathf.Clamp(CurrentHp, 0, maxHp);
                PublishHp();
            }
        }

        private void PublishHp()
        {
            OnHpChanged?.Invoke(CurrentHp, MaxHp);

            if (notifyPlayerHpEvents)
            {
                GameEvents.RaisePlayerHpChanged(CurrentHp, MaxHp);
            }

            if (notifyBossHpEvents)
            {
                float ratio = MaxHp > 0 ? (float)CurrentHp / MaxHp : 0f;
                GameEvents.RaiseBossHpChanged(ratio);
            }
        }
    }
}
