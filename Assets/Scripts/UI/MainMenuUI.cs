using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathOfPower.Core;

namespace PathOfPower.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button          _playButton;
        [SerializeField] private Button          _dailyButton;
        [SerializeField] private Button          _upgradesButton;
        [SerializeField] private TextMeshProUGUI _statsLabel;
        [SerializeField] private TextMeshProUGUI _versionLabel;

        private const string VERSION = "v0.1.0";

        private void Awake()
        {
            // Auto-find refs if inspector didn't wire them
            if (_playButton     == null) _playButton     = FindButton("PlayButton");
            if (_dailyButton    == null) _dailyButton    = FindButton("DailyButton");
            if (_upgradesButton == null) _upgradesButton = FindButton("UpgradesButton");
            if (_statsLabel     == null) _statsLabel     = FindLabel("StatsLabel");
            if (_versionLabel   == null) _versionLabel   = FindLabel("VersionLabel");
        }

        private void Start()
        {
            _playButton?.onClick.AddListener(OnPlay);
            _dailyButton?.onClick.AddListener(OnDaily);
            _upgradesButton?.onClick.AddListener(OnUpgrades);

            if (_versionLabel != null) _versionLabel.text = VERSION;

            RefreshStats();
        }

        private void RefreshStats()
        {
            if (_statsLabel == null || GameManager.Instance == null) return;
            var data = GameManager.Instance.PlayerData;
            _statsLabel.text = $"Runs: {data.totalRuns}  |  Wins: {data.totalWins}  |  Best Power: {data.highScore}";
        }

        private void OnPlay()
        {
            AudioManager.Instance?.PlaySFX("buttonClick");
            GameManager.Instance?.StartNewRun(false);
        }

        private void OnDaily()
        {
            AudioManager.Instance?.PlaySFX("buttonClick");
            GameManager.Instance?.StartNewRun(true);
        }

        private void OnUpgrades()
        {
            AudioManager.Instance?.PlaySFX("buttonClick");
            GameManager.Instance?.GoToUpgradeScreen();
        }

        private Button FindButton(string goName)
        {
            var t = transform.Find(goName) ?? FindInCanvas(goName);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private TextMeshProUGUI FindLabel(string goName)
        {
            var t = transform.Find(goName) ?? FindInCanvas(goName);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private Transform FindInCanvas(string goName)
        {
            // Search all children of parent canvas
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return null;
            foreach (Transform child in canvas.transform.GetComponentsInChildren<Transform>(true))
                if (child.name == goName) return child;
            return null;
        }
    }
}
