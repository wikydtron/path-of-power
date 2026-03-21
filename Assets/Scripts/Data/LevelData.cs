using System;
using System.Collections.Generic;

namespace PathOfPower.Data
{
    [Serializable]
    public class LevelData
    {
        public string id;
        public string name;
        public string biome;
        public int startNodeId;
        public int bossNodeId;
        public List<NodeData> nodes = new List<NodeData>();
    }

    [Serializable]
    public class UpgradeDefinition
    {
        public string id;
        public string name;
        public string description;
        public int maxLevel;
        public List<int> costs = new List<int>();
        public List<float> values = new List<float>();
        public string category;
    }

    [Serializable]
    public class UpgradeCollection
    {
        public List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();
    }
}
