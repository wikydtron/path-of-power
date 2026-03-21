using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathOfPower.Core;
using PathOfPower.Upgrades;
using PathOfPower.Data;

namespace PathOfPower.UI
{
    public class UpgradeMenuUI : MonoBehaviour
    {
        [SerializeField] private Transform       _upgradeCardContainer;
        [SerializeField] private GameObject      _upgradeCardPrefab;
        [SerializeField] private TextMeshProUGUI _goldLabel;
        [SerializeField] private Button          _startRunButton;
        [SerializeField] private Button          _mainMenuButton;

        private List<UpgradeCardUI> _cards = new List<UpgradeCardUI>();

        private void Start()
        {
            _startRunButton?.onClick.AddListener(OnStartRun);
            _mainMenuButton?.onClick.AddListener(OnMainMenu);

            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.OnUpgradePurchased += OnUpgradePurchased;

            PopulateUpgrades();
            RefreshGold();
        }

        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.OnUpgradePurchased -= OnUpgradePurchased;
        }

        private void PopulateUpgrades()
        {
            if (_upgradeCardContainer == null || _upgradeCardPrefab == null) return;
            if (UpgradeManager.Instance == null) return;

            foreach (Transform child in _upgradeCardContainer) Destroy(child.gameObject);
            _cards.Clear();

            var upgrades = UpgradeManager.Instance.GetAllUpgrades();
            foreach (var def in upgrades)
            {
                GameObject cardGo = Instantiate(_upgradeCardPrefab, _upgradeCardContainer);
                var card = cardGo.GetComponent<UpgradeCardUI>();
                if (card != null)
                {
                    card.Setup(def, OnBuyUpgrade);
                    _cards.Add(card);
                }
            }
        }

        private void OnBuyUpgrade(string upgradeId)
        {
            if (UpgradeManager.Instance == null) return;
            bool success = UpgradeManager.Instance.Purchase(upgradeId);
            if (success)
            {
                AudioManager.Instance?.PlaySFX("buff");
                RefreshAllCards();
                RefreshGold();
            }
        }

        private void OnUpgradePurchased(string id)
        {
            RefreshAllCards();
            RefreshGold();
        }

        private void RefreshAllCards()
        {
            foreach (var card in _cards) card.Refresh();
        }

        private void RefreshGold()
        {
            if (_goldLabel == null || GameManager.Instance == null) return;
            _goldLabel.text = $"Gold: {GameManager.Instance.PlayerData.totalGold}";
        }

        private void OnStartRun()
        {
            AudioManager.Instance?.PlaySFX("buttonClick");
            GameManager.Instance?.StartNewRun(false);
        }

        private void OnMainMenu()
        {
            AudioManager.Instance?.PlaySFX("buttonClick");
            GameManager.Instance?.GoToMainMenu();
        }
    }

    // Inline card component — attach this to the upgrade card prefab
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _descLabel;
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private TextMeshProUGUI _costLabel;
        [SerializeField] private Button          _buyButton;

        private UpgradeDefinition _def;
        private System.Action<string> _onBuy;

        public void Setup(UpgradeDefinition def, System.Action<string> onBuy)
        {
            _def   = def;
            _onBuy = onBuy;
            _buyButton?.onClick.AddListener(() => _onBuy?.Invoke(_def.id));
            Refresh();
        }

        public void Refresh()
        {
            if (_def == null || GameManager.Instance == null) return;

            int currentLevel = GameManager.Instance.PlayerData.GetUpgradeLevel(_def.id);
            bool maxed = currentLevel >= _def.maxLevel;

            if (_nameLabel != null) _nameLabel.text = _def.name;
            if (_levelLabel != null) _levelLabel.text = maxed ? "MAX" : $"Lv {currentLevel}/{_def.maxLevel}";

            if (_descLabel != null)
            {
                float val = maxed ? _def.values[_def.maxLevel - 1] : (currentLevel < _def.values.Count ? _def.values[currentLevel] : 0f);
                _descLabel.text = _def.description.Replace("{value}", FormatValue(val));
            }

            if (_costLabel != null)
                _costLabel.text = maxed ? "MAXED" : $"{_def.costs[currentLevel]} Gold";

            if (_buyButton != null)
                _buyButton.interactable = !maxed && (UpgradeManager.Instance?.CanAfford(_def.id) ?? false);
        }

        private string FormatValue(float val)
        {
            return val < 1f ? $"{val * 100f:F0}%" : val.ToString("F0");
        }
    }
}
