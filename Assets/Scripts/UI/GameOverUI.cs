using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathOfPower.Core;

namespace PathOfPower.UI
{
    public class GameOverUI : MonoBehaviour
    {
        public static GameOverUI Instance { get; private set; }

        [SerializeField] private GameObject      _panel;
        [SerializeField] private TextMeshProUGUI _headerLabel;
        [SerializeField] private TextMeshProUGUI _powerLabel;
        [SerializeField] private TextMeshProUGUI _goldLabel;
        [SerializeField] private Button          _continueButton;
        [SerializeField] private Button          _restartButton;
        [SerializeField] private RectTransform   _panelRect;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_panel != null) _panel.SetActive(false);

            _continueButton?.onClick.AddListener(OnContinue);
            _restartButton?.onClick.AddListener(OnRestart);
        }

        public void Show(bool won, float finalPower, int goldEarned)
        {
            if (_panel == null) return;

            _panel.SetActive(true);

            if (_headerLabel != null)
                _headerLabel.text = won ? "VICTORY!" : "DEFEATED";

            if (_powerLabel != null)
                _powerLabel.text = $"Final Power: {finalPower:F0}";

            if (_goldLabel != null)
                _goldLabel.text = $"Gold Earned: {goldEarned}";

            // Animate entrance
            if (_panelRect != null)
                StartCoroutine(EntranceAnimation());

            AudioManager.Instance?.PlaySFX(won ? "win" : "death");
        }

        private IEnumerator EntranceAnimation()
        {
            _panelRect.localScale = Vector3.zero;
            float t = 0f;
            float duration = 0.3f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = t / duration;
                // Ease out back
                float overshoot = 1.70158f;
                progress = progress - 1f;
                float scale = progress * progress * ((overshoot + 1f) * progress + overshoot) + 1f;
                _panelRect.localScale = Vector3.one * scale;
                yield return null;
            }
            _panelRect.localScale = Vector3.one;
        }

        private void OnContinue()
        {
            AudioManager.Instance?.PlaySFX("buttonClick");
            GameManager.Instance?.GoToUpgradeScreen();
        }

        private void OnRestart()
        {
            AudioManager.Instance?.PlaySFX("buttonClick");
            GameManager.Instance?.StartNewRun(false);
        }
    }
}
