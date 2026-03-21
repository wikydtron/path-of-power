using UnityEngine;
using UnityEngine.SceneManagement;
using PathOfPower.Core;

namespace PathOfPower.Core
{
    public enum GameState
    {
        MainMenu,
        InRun,
        RunOver,
        UpgradeScreen,
        DailyChallenge
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Scene name constants
        public const string SCENE_BOOTSTRAP    = "Bootstrap";
        public const string SCENE_MAIN_MENU    = "MainMenu";
        public const string SCENE_GAME         = "Game";
        public const string SCENE_UPGRADES     = "UpgradeScreen";

        public GameState State { get; private set; } = GameState.MainMenu;
        public PlayerData PlayerData { get; private set; }
        public bool IsDaily { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PlayerData = SaveSystem.Load();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            SceneManager.LoadScene(SCENE_MAIN_MENU);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // RunManager self-initializes in Game scene; we just broadcast state
            Debug.Log($"[GameManager] Scene loaded: {scene.name} | State: {State}");
        }

        public void StartNewRun(bool isDaily = false)
        {
            IsDaily = isDaily;
            State = isDaily ? GameState.DailyChallenge : GameState.InRun;
            SceneManager.LoadScene(SCENE_GAME);
        }

        public void EndRun(bool won, int goldEarned)
        {
            State = GameState.RunOver;

            PlayerData.totalRuns++;
            if (won) PlayerData.totalWins++;
            PlayerData.totalGold += goldEarned;

            SaveGame();
        }

        public void GoToUpgradeScreen()
        {
            State = GameState.UpgradeScreen;
            SceneManager.LoadScene(SCENE_UPGRADES);
        }

        public void GoToMainMenu()
        {
            State = GameState.MainMenu;
            SceneManager.LoadScene(SCENE_MAIN_MENU);
        }

        public void SaveGame()
        {
            SaveSystem.Save(PlayerData);
        }

        public void SpendGold(int amount)
        {
            PlayerData.totalGold -= amount;
            SaveGame();
        }

        public void UpdateHighScore(int power)
        {
            if (power > PlayerData.highScore)
            {
                PlayerData.highScore = power;
                SaveGame();
            }
        }
    }
}
