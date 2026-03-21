using System.Collections.Generic;
using PathOfPower.Data;

namespace PathOfPower.Dungeon
{
    public class DungeonGraph
    {
        private readonly Dictionary<int, NodeData> _nodes = new Dictionary<int, NodeData>();

        public IEnumerable<NodeData> AllNodes => _nodes.Values;

        public void AddNode(NodeData node)
        {
            _nodes[node.id] = node;
        }

        public NodeData GetNode(int id)
        {
            return _nodes.TryGetValue(id, out NodeData node) ? node : null;
        }

        public List<NodeData> GetConnectedNodes(int nodeId)
        {
            NodeData from = GetNode(nodeId);
            if (from == null) return new List<NodeData>();

            var result = new List<NodeData>();
            foreach (int connId in from.connections)
            {
                NodeData target = GetNode(connId);
                if (target != null) result.Add(target);
            }
            return result;
        }

        public List<NodeData> GetAvailableNodes(int fromNodeId)
        {
            // Returns connected nodes that haven't been visited yet
            var connected = GetConnectedNodes(fromNodeId);
            var available = new List<NodeData>();
            foreach (var node in connected)
                if (!node.visited) available.Add(node);
            return available;
        }

        public bool IsValidMove(int fromId, int toId)
        {
            NodeData from = GetNode(fromId);
            if (from == null) return false;
            return from.connections.Contains(toId);
        }

        public int Count => _nodes.Count;
    }
}
