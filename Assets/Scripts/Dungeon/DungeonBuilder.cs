using System.Collections.Generic;
using UnityEngine;
using PathOfPower.Data;

namespace PathOfPower.Dungeon
{
    public class DungeonBuilder : MonoBehaviour
    {
        [SerializeField] private NodeController _nodePrefab;
        [SerializeField] private LineRenderer   _connectionLinePrefab;
        [SerializeField] private Transform      _playerToken;

        [Header("Layout")]
        [SerializeField] private float _nodeSpacingX = 2.5f;
        [SerializeField] private float _nodeSpacingY = 2.5f;

        private DungeonGraph                      _graph;
        private Dictionary<int, NodeController>   _nodeControllers = new Dictionary<int, NodeController>();
        private List<LineRenderer>                _lines           = new List<LineRenderer>();
        private int                               _startNodeId;

        public void BuildDungeon(DungeonGraph graph, int startNodeId)
        {
            ClearDungeon();
            _graph = graph;
            _startNodeId = startNodeId;

            // Spawn nodes
            foreach (var nodeData in graph.AllNodes)
            {
                Vector3 worldPos = GridToWorld(nodeData.gridX, nodeData.gridY);
                NodeController nc = Instantiate(_nodePrefab, worldPos, Quaternion.identity, transform);
                nc.Initialize(nodeData);
                _nodeControllers[nodeData.id] = nc;
            }

            // Draw connections
            foreach (var nodeData in graph.AllNodes)
            {
                foreach (int connId in nodeData.connections)
                {
                    if (!_nodeControllers.ContainsKey(connId)) continue;

                    // Only draw each connection once (from lower id to higher id)
                    if (nodeData.id > connId) continue;

                    Vector3 from = GridToWorld(nodeData.gridX, nodeData.gridY);
                    Vector3 to   = GridToWorld(_graph.GetNode(connId).gridX, _graph.GetNode(connId).gridY);

                    LineRenderer line = Instantiate(_connectionLinePrefab, transform);
                    line.positionCount = 2;
                    line.SetPosition(0, from);
                    line.SetPosition(1, to);
                    line.startWidth = 0.08f;
                    line.endWidth   = 0.08f;
                    _lines.Add(line);
                }
            }

            // Position player token at start
            if (_playerToken != null && _nodeControllers.ContainsKey(startNodeId))
                _playerToken.position = _nodeControllers[startNodeId].transform.position;

            // Center camera
            CenterCamera();
        }

        public void HighlightAvailableNodes(int fromNodeId)
        {
            ClearHighlights();

            if (_nodeControllers.TryGetValue(fromNodeId, out var current))
                current.SetState(NodeVisualState.Current);

            var available = _graph.GetAvailableNodes(fromNodeId);
            foreach (var node in available)
            {
                if (_nodeControllers.TryGetValue(node.id, out var nc))
                    nc.SetState(NodeVisualState.Available);
            }
        }

        public void ClearHighlights()
        {
            foreach (var kv in _nodeControllers)
            {
                NodeData data = kv.Value.Data;
                if (data.visited)
                    kv.Value.SetState(NodeVisualState.Visited);
                else
                    kv.Value.SetState(NodeVisualState.Default);
            }
        }

        public void MovePlayerTo(int nodeId)
        {
            if (!_nodeControllers.TryGetValue(nodeId, out var nc)) return;

            if (_playerToken != null)
                _playerToken.position = nc.transform.position;

            nc.PlayPulseAnimation();
        }

        public void KillNode(int nodeId)
        {
            if (_nodeControllers.TryGetValue(nodeId, out var nc))
                nc.PlayDeathAnimation();
        }

        private Vector3 GridToWorld(int gridX, int gridY)
        {
            return new Vector3(gridX * _nodeSpacingX, gridY * _nodeSpacingY, 0f);
        }

        private void CenterCamera()
        {
            if (Camera.main == null) return;

            Vector3 min = Vector3.positiveInfinity;
            Vector3 max = Vector3.negativeInfinity;

            foreach (var kv in _nodeControllers)
            {
                Vector3 p = kv.Value.transform.position;
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }

            Vector3 center = (min + max) / 2f;
            Camera.main.transform.position = new Vector3(center.x, center.y, -10f);

            float width  = (max.x - min.x) + 4f;
            float height = (max.y - min.y) + 4f;
            Camera.main.orthographicSize = Mathf.Max(height / 2f, width / (2f * Camera.main.aspect));
        }

        private void ClearDungeon()
        {
            foreach (var kv in _nodeControllers) if (kv.Value != null) Destroy(kv.Value.gameObject);
            _nodeControllers.Clear();

            foreach (var line in _lines) if (line != null) Destroy(line.gameObject);
            _lines.Clear();
        }
    }
}
