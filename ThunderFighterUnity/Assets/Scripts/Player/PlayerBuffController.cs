using System.Collections.Generic;
using System.Text;
using ThunderFighter.Core;
using UnityEngine;

namespace ThunderFighter.Player
{
    public class PlayerBuffController : MonoBehaviour
    {
        private readonly List<RuntimeBuffState> activeBuffs = new List<RuntimeBuffState>();
        private readonly List<PickupBuffType> activeTypes = new List<PickupBuffType>();

        public float FireRateIntervalMultiplier { get; private set; } = 1f;
        public float ProjectileSpeedMultiplier { get; private set; } = 1f;
        public float DamageMultiplier { get; private set; } = 1f;
        public float MagnetRadiusMultiplier { get; private set; } = 1f;
        public float DamageReductionRatio { get; private set; }

        private void Update()
        {
            bool changed = false;
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                activeBuffs[i].RemainingSeconds -= Time.deltaTime;
                if (activeBuffs[i].RemainingSeconds <= 0f)
                {
                    activeBuffs.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
            {
                Recalculate();
            }
        }

        public void AddOrRefresh(PickupBuffType type, float duration, float magnitude)
        {
            RuntimeBuffState state = null;
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                if (activeBuffs[i].BuffType == type)
                {
                    state = activeBuffs[i];
                    break;
                }
            }

            if (state == null)
            {
                state = new RuntimeBuffState { BuffType = type, Stacks = 1 };
                activeBuffs.Add(state);
            }
            else
            {
                state.Stacks = Mathf.Min(state.Stacks + 1, 3);
            }

            state.RemainingSeconds = Mathf.Max(state.RemainingSeconds, duration);
            state.Magnitude = Mathf.Max(state.Magnitude, magnitude);
            Recalculate();
        }

        public int ModifyIncomingDamage(int amount)
        {
            if (amount <= 0)
            {
                return amount;
            }

            return Mathf.Max(1, Mathf.RoundToInt(amount * (1f - DamageReductionRatio)));
        }

        public string GetSummaryLabel(bool chinese)
        {
            if (activeBuffs.Count == 0)
            {
                return chinese ? "无临时增幅" : "No active buffs";
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append("  ");
                }

                builder.Append(GetBuffLabel(activeBuffs[i].BuffType, chinese));
                builder.Append(' ');
                builder.Append(Mathf.CeilToInt(activeBuffs[i].RemainingSeconds));
                builder.Append('s');
            }

            return builder.ToString();
        }

        public PickupBuffType[] GetActiveTypes()
        {
            return activeTypes.ToArray();
        }

        private void Recalculate()
        {
            FireRateIntervalMultiplier = 1f;
            ProjectileSpeedMultiplier = 1f;
            DamageMultiplier = 1f;
            MagnetRadiusMultiplier = 1f;
            DamageReductionRatio = 0f;
            activeTypes.Clear();

            for (int i = 0; i < activeBuffs.Count; i++)
            {
                RuntimeBuffState buff = activeBuffs[i];
                activeTypes.Add(buff.BuffType);
                float value = buff.Magnitude * Mathf.Max(1, buff.Stacks);
                switch (buff.BuffType)
                {
                    case PickupBuffType.FireRate:
                        FireRateIntervalMultiplier *= Mathf.Clamp(1f - value, 0.45f, 1f);
                        break;
                    case PickupBuffType.Damage:
                        DamageMultiplier += value;
                        break;
                    case PickupBuffType.ProjectileSpeed:
                        ProjectileSpeedMultiplier += value;
                        break;
                    case PickupBuffType.Magnet:
                        MagnetRadiusMultiplier += value;
                        break;
                    case PickupBuffType.Guard:
                        DamageReductionRatio = Mathf.Clamp01(DamageReductionRatio + value);
                        break;
                }
            }

            GameEvents.RaiseBuffStatusChanged(GetSummaryLabel(LocalizationService.IsChinese), GetActiveTypes());
        }

        private static string GetBuffLabel(PickupBuffType type, bool chinese)
        {
            return type switch
            {
                PickupBuffType.FireRate => chinese ? "攻速" : "ROF",
                PickupBuffType.Damage => chinese ? "伤害" : "DMG",
                PickupBuffType.ProjectileSpeed => chinese ? "弹速" : "SPD",
                PickupBuffType.Magnet => chinese ? "吸附" : "MAG",
                PickupBuffType.Guard => chinese ? "护盾" : "DEF",
                _ => chinese ? "增幅" : "BUFF"
            };
        }
    }
}
