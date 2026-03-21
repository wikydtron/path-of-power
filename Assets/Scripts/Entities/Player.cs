using UnityEngine;
using PathOfPower.Core;
using PathOfPower.Upgrades;

namespace PathOfPower.Entities
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        public float CurrentPower  { get; private set; }
        public int   CurrentNodeId { get; private set; } = -1;
        public bool  ShieldActive  => _abilities?.ShieldCharges > 0;
        public int   GoldThisRun   { get; private set; }

        public event System.Action<float> OnPowerChanged;
        public event System.Action        OnPlayerDied;
        public event System.Action        OnShieldUsed;

        private AbilitySystem _abilities;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize(PlayerData data)
        {
            _abilities   = new AbilitySystem(data);
            CurrentPower = UpgradeManager.Instance != null
                ? UpgradeManager.Instance.GetStartingPower()
                : 1f;
            GoldThisRun  = 0;
            CurrentNodeId = -1;

            Debug.Log($"[Player] Initialized — Starting power: {CurrentPower}");
        }

        public AbilitySystem GetAbilities() => _abilities;

        public void SetCurrentNode(int nodeId) => CurrentNodeId = nodeId;

        public void GainPower(float amount)
        {
            CurrentPower += amount;
            if (CurrentPower < 0.1f) CurrentPower = 0.1f;
            OnPowerChanged?.Invoke(CurrentPower);
        }

        public void SetPower(float value)
        {
            CurrentPower = Mathf.Max(0.1f, value);
            OnPowerChanged?.Invoke(CurrentPower);
        }

        public void AddGold(int amount)
        {
            GoldThisRun += amount;
        }

        public void Die()
        {
            Debug.Log("[Player] Player died.");
            OnPlayerDied?.Invoke();
        }

        public void UseShield()
        {
            _abilities?.UseShield();
            OnShieldUsed?.Invoke();
        }
    }
}
