using System;
using System.Collections.Generic;
using PathOfPower.Data;

namespace PathOfPower.Procedural
{
    public static class LevelGenerator
    {
        /// <summary>
        /// Generate a level with a given seed and difficulty (1-10).
        /// Guarantees at least one valid winnable path from startNode to boss.
        /// </summary>
        public static LevelData GenerateLevel(int seed, int difficulty)
        {
            Random rng = new Random(seed);

            int layers = 3 + Math.Min(difficulty, 5);         // 4-8 layers
            int nodesPerLayer = 2 + (difficulty > 5 ? 1 : 0); // 2-3 nodes per layer

            float startingPower = 1f;
            float enemyPowerMin = difficulty * 1.5f;
            float enemyPowerMax = difficulty * 4.0f;

            var nodes = new List<NodeData>();
            int nextId = 0;

            // -- Layer 0: Start node
            var startNode = new NodeData(nextId++, NodeType.Start, 0f, nodesPerLayer / 2, 0, new List<int>(), "Start");
            nodes.Add(startNode);

            List<int> prevLayerIds = new List<int> { startNode.id };

            // -- Middle layers
            for (int layer = 1; layer <= layers; layer++)
            {
                int nodeCount = rng.Next(2, nodesPerLayer + 1);
                List<int> thisLayerIds = new List<int>();

                for (int i = 0; i < nodeCount; i++)
                {
                    int gridX = i - nodeCount / 2;
                    int gridY = layer;

                    NodeType type = PickNodeType(rng, difficulty);
                    float power = CalculatePower(rng, type, difficulty, enemyPowerMin, enemyPowerMax);
                    string label = FormatLabel(type, power);

                    var node = new NodeData(nextId++, type, power, gridX, gridY, new List<int>(), label);
                    node.enemyType = PickEnemyType(rng, difficulty);
                    nodes.Add(node);
                    thisLayerIds.Add(node.id);
                }

                // Connect previous layer → current layer
                foreach (int prevId in prevLayerIds)
                {
                    // Each prev node connects to 1-2 nodes in this layer
                    int connections = rng.Next(1, Math.Min(3, thisLayerIds.Count + 1));
                    List<int> shuffled = Shuffle(new List<int>(thisLayerIds), rng);
                    for (int c = 0; c < connections && c < shuffled.Count; c++)
                    {
                        int targetId = shuffled[c];
                        NodeData prevNode = nodes.Find(n => n.id == prevId);
                        if (prevNode != null && !prevNode.connections.Contains(targetId))
                            prevNode.connections.Add(targetId);
                    }
                }

                prevLayerIds = thisLayerIds;
            }

            // -- Boss node at top
            int bossGridX = 0;
            int bossGridY = layers + 1;
            float bossPower = CalculateBossPower(difficulty);
            var bossNode = new NodeData(nextId++, NodeType.Boss, bossPower, bossGridX, bossGridY, new List<int>(), $"BOSS\n{bossPower:F0}");
            bossNode.enemyType = EnemyType.Boss;
            nodes.Add(bossNode);

            // Connect last layer to boss
            foreach (int lastId in prevLayerIds)
            {
                NodeData lastNode = nodes.Find(n => n.id == lastId);
                if (lastNode != null && !lastNode.connections.Contains(bossNode.id))
                    lastNode.connections.Add(bossNode.id);
            }

            var level = new LevelData
            {
                id          = $"generated_{seed}",
                name        = $"Dungeon (D{difficulty})",
                biome       = "Generated",
                startNodeId = startNode.id,
                bossNodeId  = bossNode.id,
                nodes       = nodes
            };

            // Verify at least one valid path exists; if not, inject a buff path
            if (!HasValidPath(level, startingPower))
                InjectSolvablePath(level, rng, startingPower, nodes, startNode, bossNode);

            return level;
        }

        private static NodeType PickNodeType(Random rng, int difficulty)
        {
            // Distribution changes with difficulty
            int roll = rng.Next(100);

            if (difficulty <= 3)
            {
                if (roll < 40) return NodeType.Enemy;
                if (roll < 60) return NodeType.Buff;
                if (roll < 75) return NodeType.Multiplier;
                if (roll < 85) return NodeType.Trap;
                return NodeType.Chest;
            }
            else if (difficulty <= 6)
            {
                if (roll < 50) return NodeType.Enemy;
                if (roll < 65) return NodeType.Buff;
                if (roll < 78) return NodeType.Multiplier;
                if (roll < 92) return NodeType.Trap;
                return NodeType.Chest;
            }
            else
            {
                if (roll < 55) return NodeType.Enemy;
                if (roll < 67) return NodeType.Buff;
                if (roll < 78) return NodeType.Multiplier;
                if (roll < 95) return NodeType.Trap;
                return NodeType.Chest;
            }
        }

        private static EnemyType PickEnemyType(Random rng, int difficulty)
        {
            int roll = rng.Next(100);
            if (difficulty < 5)
                return roll < 70 ? EnemyType.Basic : (roll < 90 ? EnemyType.Tank : EnemyType.Mage);
            else
                return roll < 50 ? EnemyType.Basic : (roll < 75 ? EnemyType.Tank : EnemyType.Mage);
        }

        private static float CalculatePower(Random rng, NodeType type, int difficulty, float min, float max)
        {
            float baseEnemy = (float)(rng.NextDouble() * (max - min) + min);

            switch (type)
            {
                case NodeType.Enemy:      return (float)Math.Round(baseEnemy, 0);
                case NodeType.Buff:       return (float)Math.Round(baseEnemy * 0.6f, 0);
                case NodeType.Multiplier: return rng.Next(2) == 0 ? 2f : 3f;
                case NodeType.Trap:
                    // 50% chance percentage trap, 50% flat
                    return rng.Next(2) == 0
                        ? (float)Math.Round(rng.NextDouble() * 0.3 + 0.1, 2)  // 10-40% reduction
                        : (float)Math.Round(baseEnemy * 0.4f, 0);              // flat reduction
                case NodeType.Chest:      return (float)Math.Round(difficulty * rng.Next(5, 20), 0);
                default:                  return 0f;
            }
        }

        private static float CalculateBossPower(int difficulty)
        {
            // Mirrors handcrafted boss power scaling
            float[] bossPowers = { 12, 25, 40, 70, 100, 150, 220, 320, 450, 700 };
            int idx = Math.Max(0, Math.Min(difficulty - 1, bossPowers.Length - 1));
            return bossPowers[idx];
        }

        private static string FormatLabel(NodeType type, float power)
        {
            switch (type)
            {
                case NodeType.Enemy:      return power.ToString("F0");
                case NodeType.Buff:       return $"+{power:F0}";
                case NodeType.Multiplier: return $"x{power:F0}";
                case NodeType.Trap:       return power < 1f ? $"-{power * 100:F0}%" : $"-{power:F0}";
                case NodeType.Chest:      return $"${power:F0}";
                default:                  return "";
            }
        }

        /// <summary>
        /// Simulate the best possible path using DFS and check if boss can be beaten.
        /// </summary>
        public static bool HasValidPath(LevelData level, float startingPower)
        {
            var nodeMap = new Dictionary<int, NodeData>();
            foreach (var n in level.nodes) nodeMap[n.id] = n;

            return DFSWinCheck(nodeMap, level.startNodeId, level.bossNodeId, startingPower, new HashSet<int>());
        }

        private static bool DFSWinCheck(Dictionary<int, NodeData> nodes, int currentId, int bossId, float power, HashSet<int> visited)
        {
            if (!nodes.TryGetValue(currentId, out var current)) return false;
            if (visited.Contains(currentId)) return false;

            visited = new HashSet<int>(visited) { currentId };

            // Simulate this node
            float newPower = SimulateNode(current, power);
            if (newPower <= 0f) return false;  // died

            if (currentId == bossId) return true;  // reached boss and survived

            foreach (int nextId in current.connections)
            {
                if (DFSWinCheck(nodes, nextId, bossId, newPower, visited))
                    return true;
            }
            return false;
        }

        private static float SimulateNode(NodeData node, float power)
        {
            switch (node.type)
            {
                case NodeType.Start:
                    return power;
                case NodeType.Enemy:
                case NodeType.Boss:
                    if (power > node.power) return power + node.power;
                    return -1f; // dead
                case NodeType.Buff:
                    return power + node.power;
                case NodeType.Multiplier:
                    return power * node.power;
                case NodeType.Trap:
                    if (node.power < 1f) return power * (1f - node.power);
                    return Math.Max(0.1f, power - node.power);
                case NodeType.Chest:
                    return power; // gold, no power change
                default:
                    return power;
            }
        }

        /// <summary>
        /// Inject a guaranteed winnable path by adding buff nodes alongside the current path.
        /// </summary>
        private static void InjectSolvablePath(LevelData level, Random rng, float startingPower,
            List<NodeData> nodes, NodeData startNode, NodeData bossNode)
        {
            // Add a buff chain from start to boss
            float neededPower = bossNode.power + 1f;
            float currentPower = startingPower;
            int   prevId       = startNode.id;
            int   nextId       = nodes.Count + 1000; // avoid id collisions
            int   gridXOffset  = 5;

            for (int layer = 1; currentPower <= neededPower; layer++)
            {
                float buffAmount = neededPower / 3f;
                var buffNode = new NodeData(nextId++, NodeType.Buff, buffAmount, gridXOffset, layer, new List<int>(), $"+{buffAmount:F0}");
                nodes.Add(buffNode);
                level.nodes.Add(buffNode);

                NodeData prevNode = nodes.Find(n => n.id == prevId);
                prevNode?.connections.Add(buffNode.id);

                currentPower += buffAmount;
                prevId = buffNode.id;

                if (layer > 10) break; // safety valve
            }

            NodeData lastInjected = nodes.Find(n => n.id == prevId);
            lastInjected?.connections.Add(bossNode.id);
        }

        private static List<T> Shuffle<T>(List<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }
    }
}
