using System.Collections.Generic;
using UnityEngine;
using PathOfPower.Core;
using PathOfPower.Data;
using PathOfPower.Dungeon;

namespace PathOfPower.Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        public event System.Action<string> OnUpgradePurchased;

        private UpgradeCollection _collection;
        private static Dictionary<string, UpgradeDefinition> _upgradeMap = new Dictionary<string, UpgradeDefinition>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            LoadUpgrades();
        }

        private void LoadUpgrades()
        {
            _collection = DungeonLoader.LoadUpgrades();
            _upgradeMap.Clear();
            if (_collection?.upgrades != null)
                foreach (var upgrade in _collection.upgrades)
                    _upgradeMap[upgrade.id] = upgrade;

            Debug.Log($"[UpgradeManager] Loaded {_upgradeMap.Count} upgrades.");
        }

        public List<UpgradeDefinition> GetAllUpgrades() => _collection?.upgrades ?? new List<UpgradeDefinition>();

        public UpgradeDefinition GetUpgrade(string id)
        {
            return _upgradeMap.TryGetValue(id, out var def) ? def : null;
        }

        public bool CanAfford(string id)
        {
            if (GameManager.Instance == null) return false;
            var def = GetUpgrade(id);
            if (def == null) return false;

            int currentLevel = GameManager.Instance.PlayerData.GetUpgradeLevel(id);
            if (currentLevel >= def.maxLevel) return false;

            int cost = def.costs[currentLevel]; // cost[0] = cost for first upgrade, etc.
            return GameManager.Instance.PlayerData.totalGold >= cost;
        }

        public bool Purchase(string id)
        {
            if (!CanAfford(id)) return false;

            var def = GetUpgrade(id);
            int currentLevel = GameManager.Instance.PlayerData.GetUpgradeLevel(id);
            int cost = def.costs[currentLevel];

            GameManager.Instance.SpendGold(cost);

            int newLevel = currentLevel + 1;
            GameManager.Instance.PlayerData.SetUpgradeLevel(id, newLevel);

            if (!GameManager.Instance.PlayerData.unlockedUpgrades.Contains(id))
                GameManager.Instance.PlayerData.unlockedUpgrades.Add(id);

            GameManager.Instance.SaveGame();
            OnUpgradePurchased?.Invoke(id);
            return true;
        }

        public bool IsUnlocked(string id) => GameManager.Instance?.PlayerData.HasUpgrade(id) ?? false;

        public float GetStartingPower()
        {
            float base_ = 1f;
            int level = GameManager.Instance?.PlayerData.GetUpgradeLevel("starting_power") ?? 0;
            return base_ + GetValueForLevel("starting_power", level);
        }

        public float GetBonusGainPercent()
        {
            int level = GameManager.Instance?.PlayerData.GetUpgradeLevel("bonus_gains") ?? 0;
            return GetValueForLevel("bonus_gains", level);
        }

        // Static accessor used by AbilitySystem (called before Instance may exist)
        public static float GetValueForLevel(string id, int level)
        {
            if (level <= 0) return 0f;
            if (!_upgradeMap.TryGetValue(id, out var def)) return 0f;
            int idx = level - 1;
            if (idx < 0 || idx >= def.values.Count) return 0f;
            return def.values[idx];
        }
    }
}
