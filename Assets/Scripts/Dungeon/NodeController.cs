using System.Collections;
using UnityEngine;
using TMPro;
using PathOfPower.Data;
using PathOfPower.Core;

namespace PathOfPower.Dungeon
{
    public enum NodeVisualState { Default, Available, Current, Visited, Dead }

    public class NodeController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _iconRenderer;
        [SerializeField] private SpriteRenderer _bgRenderer;
        [SerializeField] private TextMeshPro    _powerLabel;
        [SerializeField] private GameObject     _availableGlow;
        [SerializeField] private GameObject     _currentMarker;

        // Color palette
        private static readonly Color COLOR_START       = new Color(0.3f, 0.6f, 1.0f);
        private static readonly Color COLOR_ENEMY       = new Color(0.9f, 0.2f, 0.2f);
        private static readonly Color COLOR_BUFF        = new Color(0.2f, 0.85f, 0.3f);
        private static readonly Color COLOR_MULTIPLIER  = new Color(1.0f, 0.8f, 0.1f);
        private static readonly Color COLOR_TRAP        = new Color(1.0f, 0.45f, 0.0f);
        private static readonly Color COLOR_BOSS        = new Color(0.6f, 0.1f, 0.9f);
        private static readonly Color COLOR_CHEST       = new Color(1.0f, 0.9f, 0.3f);
        private static readonly Color COLOR_VISITED     = new Color(0.35f, 0.35f, 0.4f);
        private static readonly Color COLOR_AVAILABLE   = Color.white;

        public NodeData Data { get; private set; }

        public void Initialize(NodeData data)
        {
            Data = data;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (Data == null) return;

            // Background color by type
            if (_bgRenderer != null)
                _bgRenderer.color = GetColorForType(Data.type);

            // Power label
            if (_powerLabel != null)
                _powerLabel.text = GetLabelText();

            if (_availableGlow != null) _availableGlow.SetActive(false);
            if (_currentMarker != null) _currentMarker.SetActive(false);
        }

        private string GetLabelText()
        {
            switch (Data.type)
            {
                case NodeType.Start:       return "START";
                case NodeType.Exit:        return "EXIT";
                case NodeType.Boss:        return $"BOSS\n{Data.power:F0}";
                case NodeType.Enemy:       return $"{Data.power:F0}";
                case NodeType.Buff:        return $"+{Data.power:F0}";
                case NodeType.Multiplier:  return $"x{Data.power:F1}";
                case NodeType.Trap:
                    return Data.power < 1f ? $"-{Data.power * 100f:F0}%" : $"-{Data.power:F0}";
                case NodeType.Chest:       return $"${Data.power:F0}";
                default:                   return "?";
            }
        }

        private Color GetColorForType(NodeType type)
        {
            switch (type)
            {
                case NodeType.Start:      return COLOR_START;
                case NodeType.Enemy:      return COLOR_ENEMY;
                case NodeType.Buff:       return COLOR_BUFF;
                case NodeType.Multiplier: return COLOR_MULTIPLIER;
                case NodeType.Trap:       return COLOR_TRAP;
                case NodeType.Boss:       return COLOR_BOSS;
                case NodeType.Chest:      return COLOR_CHEST;
                default:                  return Color.gray;
            }
        }

        public void SetState(NodeVisualState state)
        {
            if (_availableGlow != null) _availableGlow.SetActive(state == NodeVisualState.Available);
            if (_currentMarker != null) _currentMarker.SetActive(state == NodeVisualState.Current);

            if (_bgRenderer != null)
            {
                if (state == NodeVisualState.Visited || state == NodeVisualState.Dead)
                    _bgRenderer.color = COLOR_VISITED;
                else if (state == NodeVisualState.Available)
                    _bgRenderer.color = Color.Lerp(GetColorForType(Data.type), COLOR_AVAILABLE, 0.3f);
                else
                    _bgRenderer.color = GetColorForType(Data.type);
            }
        }

        public void OnClicked()
        {
            RunManager.Instance?.TryMoveToNode(Data.id);
        }

        private void OnMouseDown()
        {
            OnClicked();
        }

        public void PlayDeathAnimation()
        {
            StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        public void PlayPulseAnimation()
        {
            StartCoroutine(PulseCoroutine());
        }

        private IEnumerator PulseCoroutine()
        {
            Vector3 original = transform.localScale;
            Vector3 big = original * 1.25f;
            float t = 0f;
            while (t < 0.15f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(original, big, t / 0.15f); yield return null; }
            t = 0f;
            while (t < 0.15f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(big, original, t / 0.15f); yield return null; }
        }
    }
}
