using System;
using System.Collections.Generic;

namespace PathOfPower.Data
{
    public enum NodeType
    {
        Start,
        Enemy,
        Buff,
        Multiplier,
        Trap,
        Boss,
        Chest,
        Exit
    }

    public enum EnemyType
    {
        Basic,
        Tank,
        Mage,
        Boss
    }

    [Serializable]
    public class NodeData
    {
        public int id;
        public NodeType type;
        public EnemyType enemyType;
        public float power;       // enemy strength, buff amount, multiplier value, trap amount
        public int gridX;
        public int gridY;
        public List<int> connections = new List<int>();
        public bool visited;
        public string displayName;

        public NodeData() { }

        public NodeData(int id, NodeType type, float power, int gridX, int gridY, List<int> connections, string displayName = "")
        {
            this.id = id;
            this.type = type;
            this.power = power;
            this.gridX = gridX;
            this.gridY = gridY;
            this.connections = connections ?? new List<int>();
            this.displayName = displayName;
            this.visited = false;
            this.enemyType = EnemyType.Basic;
        }
    }
}
