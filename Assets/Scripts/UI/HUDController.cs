using System.Collections;
using UnityEngine;
using TMPro;
using PathOfPower.Core;
using PathOfPower.Entities;

namespace PathOfPower.UI
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI _powerLabel;
        [SerializeField] private TextMeshProUGUI _goldLabel;
        [SerializeField] private TextMeshProUGUI _stageLabel;
        [SerializeField] private TextMeshProUGUI _levelNameLabel;
        [SerializeField] private RectTransform   _powerLabelRect;

        private float _displayPower;
        private float _targetPower;
        private bool  _animating;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (Player.Instance != null)
            {
                Player.Instance.OnPowerChanged += UpdatePower;
                _displayPower = Player.Instance.CurrentPower;
                _targetPower  = _displayPower;
                RefreshPowerLabel();
            }

            if (RunManager.Instance != null)
                RunManager.Instance.OnLevelComplete += OnLevelComplete;
        }

        private void OnDestroy()
        {
            if (Player.Instance != null) Player.Instance.OnPowerChanged -= UpdatePower;
            if (RunManager.Instance != null) RunManager.Instance.OnLevelComplete -= OnLevelComplete;
        }

        private void Update()
        {
            if (_animating)
            {
                _displayPower = Mathf.Lerp(_displayPower, _targetPower, Time.deltaTime * 8f);
                if (Mathf.Abs(_displayPower - _targetPower) < 0.05f)
                {
                    _displayPower = _targetPower;
                    _animating = false;
                }
                RefreshPowerLabel();
            }
        }

        public void UpdatePower(float power)
        {
            _targetPower = power;
            _animating   = true;
            PowerFlash();
        }

        public void UpdateGold(int gold)
        {
            if (_goldLabel != null)
                _goldLabel.text = $"Gold: {gold}";
        }

        public void SetLevelName(string levelName)
        {
            if (_levelNameLabel != null)
                _levelNameLabel.text = levelName;
        }

        public void SetStage(int current, int total)
        {
            if (_stageLabel != null)
                _stageLabel.text = $"Stage {current}/{total}";
        }

        public void PowerFlash()
        {
            if (_powerLabelRect == null) return;
            StopAllCoroutines();
            StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            Vector3 orig = _powerLabelRect.localScale;
            Vector3 big  = orig * 1.3f;
            float t = 0f;

            while (t < 0.12f) { t += Time.deltaTime; _powerLabelRect.localScale = Vector3.Lerp(orig, big, t / 0.12f); yield return null; }
            t = 0f;
            while (t < 0.12f) { t += Time.deltaTime; _powerLabelRect.localScale = Vector3.Lerp(big, orig, t / 0.12f); yield return null; }
            _powerLabelRect.localScale = orig;
        }

        private void RefreshPowerLabel()
        {
            if (_powerLabel != null)
                _powerLabel.text = $"{_displayPower:F0}";
        }

        private void OnLevelComplete()
        {
            if (RunManager.Instance?.CurrentLevel != null)
            {
                SetLevelName(RunManager.Instance.CurrentLevel.name);
                SetStage(RunManager.Instance.CurrentLevelIndex, 10);
            }
        }
    }
}
