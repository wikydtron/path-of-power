using System.IO;
using UnityEngine;
using PathOfPower.Data;
using PathOfPower.Dungeon;

namespace PathOfPower.Dungeon
{
    public static class DungeonLoader
    {
        private static string LevelsPath => Path.Combine(Application.streamingAssetsPath, "Levels");

        public static LevelData LoadLevel(string levelId)
        {
            string path = Path.Combine(LevelsPath, $"{levelId}.json");

            if (!File.Exists(path))
            {
                Debug.LogError($"[DungeonLoader] Level file not found: {path}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                LevelData level = JsonUtility.FromJson<LevelData>(json);
                if (level == null) Debug.LogError($"[DungeonLoader] Failed to parse level: {levelId}");
                return level;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DungeonLoader] Exception loading {levelId}: {ex.Message}");
                return null;
            }
        }

        public static LevelData LoadLevelByIndex(int index)
        {
            // Levels are named level_01, level_02, etc.
            string id = $"level_{index:D2}";
            return LoadLevel(id);
        }

        public static UpgradeCollection LoadUpgrades()
        {
            string path = Path.Combine(LevelsPath, "upgrades.json");
            if (!File.Exists(path))
            {
                Debug.LogError($"[DungeonLoader] upgrades.json not found at {path}");
                return new UpgradeCollection();
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<UpgradeCollection>(json) ?? new UpgradeCollection();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DungeonLoader] Exception loading upgrades: {ex.Message}");
                return new UpgradeCollection();
            }
        }

        public static DungeonGraph BuildGraph(LevelData level)
        {
            if (level == null) return null;

            var graph = new DungeonGraph();
            foreach (var node in level.nodes)
                graph.AddNode(node);

            return graph;
        }
    }
}
