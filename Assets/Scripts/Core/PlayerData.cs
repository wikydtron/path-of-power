using System;
using System.Collections.Generic;

namespace PathOfPower.Core
{
    [Serializable]
    public class PlayerData
    {
        public int totalGold;
        public int totalRuns;
        public int totalWins;
        public int highScore;           // highest power achieved in a single run
        public List<string> unlockedUpgrades = new List<string>();
        public List<string> upgradeLevelKeys = new List<string>();   // parallel to upgradeLevelValues
        public List<int> upgradeLevelValues = new List<int>();       // Dictionary<string,int> surrogate
        public string lastDailyDate;
        public int dailyBestScore;

        // Dictionary helper — Unity's JsonUtility can't serialize Dictionary directly
        public int GetUpgradeLevel(string upgradeId)
        {
            int idx = upgradeLevelKeys.IndexOf(upgradeId);
            return idx >= 0 ? upgradeLevelValues[idx] : 0;
        }

        public void SetUpgradeLevel(string upgradeId, int level)
        {
            int idx = upgradeLevelKeys.IndexOf(upgradeId);
            if (idx >= 0)
                upgradeLevelValues[idx] = level;
            else
            {
                upgradeLevelKeys.Add(upgradeId);
                upgradeLevelValues.Add(level);
            }
        }

        public bool HasUpgrade(string upgradeId) => unlockedUpgrades.Contains(upgradeId);
    }
}
