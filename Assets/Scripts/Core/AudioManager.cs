using UnityEngine;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Singleton audio manager that persists across scenes.
    /// Owns one AudioSource for UI/gameplay SFX and a second for looping background music.
    /// Attach to a scene GameObject; assign clips in the Inspector.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        /// <summary>The single instance shared across all scenes.</summary>
        public static AudioManager Instance { get; private set; }

        [Header("SFX – UI")]
        [SerializeField] private AudioClip _buttonSfx;

        [Header("Music")]
        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] [Range(0f, 1f)] private float _musicVolume = 0.5f;

        [Header("SFX – Gameplay")]
        [SerializeField] private AudioClip _submitOutfitSfx;
        [SerializeField] private AudioClip _idealOutfitMatchSfx;
        [SerializeField] private AudioClip _lessThanIdealOutfitMatchSfx;
        [SerializeField] private AudioClip _equipClothingItemSfx;
        [SerializeField] private AudioClip _timerEndSfx;

        private AudioSource _audioSource;
        private AudioSource _musicSource;

        /// <summary>
        /// Duration of the button SFX in seconds.
        /// Returns 0 if no clip is assigned — callers can safely wait this long before acting.
        /// </summary>
        public float ButtonSfxLength => _buttonSfx != null ? _buttonSfx.length : 0f;

        /// <summary>
        /// Enforces singleton, survives scene loads, caches the SFX AudioSource,
        /// creates a dedicated music AudioSource, and starts background music.
        /// Duplicate instances (e.g. from scene reloads) are destroyed immediately.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = GetComponent<AudioSource>();

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;

            StartBackgroundMusic();
        }

        private void StartBackgroundMusic()
        {
            if (_backgroundMusic == null) return;
            _musicSource.clip = _backgroundMusic;
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
        }

        #region Music volume

        /// <summary>Gets or sets the background music volume (0–1).</summary>
        public float MusicVolume
        {
            get => _musicSource.volume;
            set => _musicSource.volume = Mathf.Clamp01(value);
        }

        public static bool TryGetMusicVolume(out float volume)
        {
            if (Instance != null) { volume = Instance.MusicVolume; return true; }
            volume = 1f;
            return false;
        }

        public static bool TrySetMusicVolume(float volume)
        {
            if (Instance != null) { Instance.MusicVolume = volume; return true; }
            return false;
        }

        #endregion

        #region SFX – UI

        /// <summary>
        /// Plays the button SFX via PlayOneShot so rapid clicks layer instead of cutting off.
        /// </summary>
        public void PlayButtonSfx()
        {
            if (_buttonSfx != null)
                _audioSource.PlayOneShot(_buttonSfx);
        }

        public static void TryPlayButtonSfx()
        {
            if (Instance != null)
                Instance.PlayButtonSfx();
            else
                Debug.LogWarning("AudioManager.Instance is null; button SFX was skipped.");
        }

        #endregion

        #region SFX – Gameplay

        public void PlaySubmitOutfitSfx()
        {
            if (_submitOutfitSfx != null)
                _audioSource.PlayOneShot(_submitOutfitSfx);
        }

        public static void TryPlaySubmitOutfitSfx()
        {
            if (Instance != null)
                Instance.PlaySubmitOutfitSfx();
            else
                Debug.LogWarning("AudioManager.Instance is null; submit outfit SFX was skipped.");
        }

        public void PlayIdealOutfitMatchSfx()
        {
            if (_idealOutfitMatchSfx != null)
                _audioSource.PlayOneShot(_idealOutfitMatchSfx);
        }

        public static void TryPlayIdealOutfitMatchSfx()
        {
            if (Instance != null)
                Instance.PlayIdealOutfitMatchSfx();
            else
                Debug.LogWarning("AudioManager.Instance is null; ideal outfit match SFX was skipped.");
        }

        public void PlayLessThanIdealOutfitMatchSfx()
        {
            if (_lessThanIdealOutfitMatchSfx != null)
                _audioSource.PlayOneShot(_lessThanIdealOutfitMatchSfx);
        }

        public static void TryPlayLessThanIdealOutfitMatchSfx()
        {
            if (Instance != null)
                Instance.PlayLessThanIdealOutfitMatchSfx();
            else
                Debug.LogWarning("AudioManager.Instance is null; less-than-ideal outfit match SFX was skipped.");
        }

        public void PlayEquipClothingItemSfx()
        {
            if (_equipClothingItemSfx != null)
                _audioSource.PlayOneShot(_equipClothingItemSfx);
        }

        public static void TryPlayEquipClothingItemSfx()
        {
            if (Instance != null)
                Instance.PlayEquipClothingItemSfx();
            else
                Debug.LogWarning("AudioManager.Instance is null; equip clothing item SFX was skipped.");
        }

        public void PlayTimerEndSfx()
        {
            if (_timerEndSfx != null)
                _audioSource.PlayOneShot(_timerEndSfx);
        }

        public static void TryPlayTimerEndSfx()
        {
            if (Instance != null)
                Instance.PlayTimerEndSfx();
            else
                Debug.LogWarning("AudioManager.Instance is null; timer end SFX was skipped.");
        }

        #endregion

        #region SFX master volume

        /// <summary>Gets or sets the SFX volume on the managed audio source.</summary>
        public float MasterVolume
        {
            get => _audioSource.volume;
            set => _audioSource.volume = Mathf.Clamp01(value);
        }

        public static bool TryGetMasterVolume(out float volume)
        {
            if (Instance != null) { volume = Instance.MasterVolume; return true; }
            volume = 1f;
            return false;
        }

        public static bool TrySetMasterVolume(float volume)
        {
            if (Instance != null) { Instance.MasterVolume = volume; return true; }
            return false;
        }

        #endregion
    }
}
