using System.Collections;
using UnityEngine;
using PathOfPower.Data;
using PathOfPower.Dungeon;
using PathOfPower.Entities;
using PathOfPower.Upgrades;
using PathOfPower.Procedural;

namespace PathOfPower.Core
{
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        public event System.Action          OnRunStarted;
        public event System.Action          OnLevelComplete;
        public event System.Action<bool>    OnRunComplete;
        public event System.Action          OnPlayerDied;

        [SerializeField] private DungeonBuilder _dungeonBuilder;
        [SerializeField] private UpgradeManager _upgradeManager;

        public int         CurrentLevelIndex { get; private set; }
        public LevelData   CurrentLevel      { get; private set; }
        public DungeonGraph DungeonGraph      { get; private set; }

        private const int TOTAL_LEVELS = 10;
        private bool _runActive;
        private bool _isDaily;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[RunManager] GameManager not found — starting debug run.");
                BeginRun(false);
                return;
            }
            BeginRun(GameManager.Instance.IsDaily);
        }

        public void BeginRun(bool isDaily)
        {
            _isDaily = isDaily;
            CurrentLevelIndex = 1;
            _runActive = true;

            PlayerData data = GameManager.Instance != null ? GameManager.Instance.PlayerData : new PlayerData();

            Player.Instance?.Initialize(data);
            if (Player.Instance != null)
            {
                Player.Instance.OnPlayerDied += HandlePlayerDied;
            }

            LoadLevel(CurrentLevelIndex);
            OnRunStarted?.Invoke();
        }

        private void LoadLevel(int index)
        {
            LevelData level;

            if (_isDaily && index == 1)
                level = DailyChallenge.GetDailyLevel();
            else
                level = DungeonLoader.LoadLevelByIndex(index);

            if (level == null)
            {
                Debug.LogError($"[RunManager] Could not load level {index}");
                return;
            }

            CurrentLevel  = level;
            DungeonGraph  = DungeonLoader.BuildGraph(level);

            _dungeonBuilder?.BuildDungeon(DungeonGraph, level.startNodeId);

            // Set player starting node
            Player.Instance?.SetCurrentNode(level.startNodeId);
            DungeonGraph.GetNode(level.startNodeId).visited = true;

            _dungeonBuilder?.HighlightAvailableNodes(level.startNodeId);
        }

        public void TryMoveToNode(int targetNodeId)
        {
            if (!_runActive) return;

            int currentId = Player.Instance?.CurrentNodeId ?? -1;
            if (currentId < 0) return;

            if (!DungeonGraph.IsValidMove(currentId, targetNodeId))
            {
                Debug.Log($"[RunManager] Invalid move: {currentId} → {targetNodeId}");
                return;
            }

            NodeData targetNode = DungeonGraph.GetNode(targetNodeId);
            if (targetNode == null) return;

            // Resolve interaction
            var result = CombatResolver.Resolve(
                Player.Instance.CurrentPower,
                targetNode,
                Player.Instance.GetAbilities()
            );

            // Apply result
            if (result.shieldUsed) Player.Instance.UseShield();

            if (!result.playerWon)
            {
                HandlePlayerDied();
                return;
            }

            // Update player state
            Player.Instance.SetPower(result.newPower);
            if (result.goldGained > 0) Player.Instance.AddGold(result.goldGained);

            // Update high score
            GameManager.Instance?.UpdateHighScore((int)result.newPower);

            // Mark node visited, move player
            targetNode.visited = true;
            Player.Instance.SetCurrentNode(targetNodeId);
            _dungeonBuilder?.MovePlayerTo(targetNodeId);
            _dungeonBuilder?.ClearHighlights();

            // Play audio
            UI.AudioManager.Instance?.PlaySFXForNode(targetNode.type, result.playerWon);

            // Flash power UI
            UI.HUDController.Instance?.PowerFlash();

            // Check if boss was defeated
            if (targetNode.type == NodeType.Boss)
            {
                StartCoroutine(DelayedLevelComplete());
                return;
            }

            // Highlight next available moves
            _dungeonBuilder?.HighlightAvailableNodes(targetNodeId);
        }

        private IEnumerator DelayedLevelComplete()
        {
            yield return new WaitForSeconds(1f);

            if (CurrentLevelIndex >= TOTAL_LEVELS)
            {
                CompleteRun(true);
            }
            else
            {
                CurrentLevelIndex++;
                OnLevelComplete?.Invoke();
                LoadLevel(CurrentLevelIndex);
            }
        }

        private void HandlePlayerDied()
        {
            if (!_runActive) return;
            _runActive = false;

            Debug.Log("[RunManager] Player died.");
            OnPlayerDied?.Invoke();
            UI.AudioManager.Instance?.PlaySFX("death");

            int gold = Player.Instance?.GoldThisRun ?? 0;
            GameManager.Instance?.EndRun(false, gold);
            UI.GameOverUI.Instance?.Show(false, Player.Instance?.CurrentPower ?? 0f, gold);
        }

        public void CompleteRun(bool won)
        {
            _runActive = false;

            int gold = Player.Instance?.GoldThisRun ?? 0;
            int bonus = won ? 50 : 0;
            gold += bonus;

            GameManager.Instance?.EndRun(won, gold);
            OnRunComplete?.Invoke(won);
            UI.AudioManager.Instance?.PlaySFX(won ? "win" : "death");
            UI.GameOverUI.Instance?.Show(won, Player.Instance?.CurrentPower ?? 0f, gold);
        }

        private void OnDestroy()
        {
            if (Player.Instance != null)
                Player.Instance.OnPlayerDied -= HandlePlayerDied;
        }
    }
}
