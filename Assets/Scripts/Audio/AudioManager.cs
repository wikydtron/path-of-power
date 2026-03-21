using System.Collections.Generic;
using UnityEngine;
using PathOfPower.Data;

namespace PathOfPower.UI
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip _attackClip;
        [SerializeField] private AudioClip _winClip;
        [SerializeField] private AudioClip _deathClip;
        [SerializeField] private AudioClip _buffClip;
        [SerializeField] private AudioClip _multiplierClip;
        [SerializeField] private AudioClip _trapClip;
        [SerializeField] private AudioClip _buttonClickClip;
        [SerializeField] private AudioClip _chestClip;

        [Header("Music")]
        [SerializeField] private AudioClip _bgMusicClip;

        private Dictionary<string, AudioClip> _clipMap;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildClipMap();
        }

        private void Start()
        {
            PlayMusic();
        }

        private void BuildClipMap()
        {
            _clipMap = new Dictionary<string, AudioClip>
            {
                { "attack",      _attackClip      },
                { "win",         _winClip         },
                { "death",       _deathClip       },
                { "buff",        _buffClip        },
                { "multiplier",  _multiplierClip  },
                { "trap",        _trapClip        },
                { "buttonClick", _buttonClickClip },
                { "chest",       _chestClip       }
            };
        }

        public void PlaySFX(string clipName)
        {
            if (_sfxSource == null) return;
            if (_clipMap.TryGetValue(clipName, out var clip) && clip != null)
                _sfxSource.PlayOneShot(clip);
        }

        public void PlaySFXForNode(NodeType nodeType, bool playerWon)
        {
            switch (nodeType)
            {
                case NodeType.Enemy:
                case NodeType.Boss:
                    PlaySFX(playerWon ? "attack" : "death");
                    break;
                case NodeType.Buff:
                    PlaySFX("buff");
                    break;
                case NodeType.Multiplier:
                    PlaySFX("multiplier");
                    break;
                case NodeType.Trap:
                    PlaySFX("trap");
                    break;
                case NodeType.Chest:
                    PlaySFX("chest");
                    break;
            }
        }

        public void PlayMusic()
        {
            if (_musicSource == null || _bgMusicClip == null) return;
            _musicSource.clip = _bgMusicClip;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource?.Stop();
        }

        public void SetSFXVolume(float v)
        {
            if (_sfxSource != null) _sfxSource.volume = v;
        }

        public void SetMusicVolume(float v)
        {
            if (_musicSource != null) _musicSource.volume = v;
        }
    }
}
