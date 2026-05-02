using UnityEngine;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Singleton audio manager that persists across scenes.
    /// Owns a dedicated AudioSource for UI sound effects.
    /// Attach to a scene GameObject; assign clips in the Inspector.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        /// <summary>The single instance shared across all scenes.</summary>
        public static AudioManager Instance { get; private set; }

        /// <summary>Clip played when any UI button is clicked.</summary>
        [SerializeField]
        private AudioClip _buttonSfx;

        [SerializeField]
        private AudioClip _equipSfx;
        [SerializeField]
        private AudioClip _unequipSfx;
        [SerializeField]
        private AudioClip _swapSfx;

        private AudioSource _audioSource;

        /// <summary>Key for <see cref="PlayOtherSFX"/> equip clip.</summary>
        public const string OtherSfxEquipKey = "equip";

        /// <summary>Key for <see cref="PlayOtherSFX"/> unequip clip.</summary>
        public const string OtherSfxUnequipKey = "unequip";

        /// <summary>Key for <see cref="PlayOtherSFX"/> swap clip.</summary>
        public const string OtherSfxSwapKey = "swap";

        /// <summary>
        /// Duration of the button SFX in seconds.
        /// Returns 0 if no clip is assigned — callers can safely wait this long before acting.
        /// </summary>
        public float ButtonSfxLength
        {
            get
            {
                if (_buttonSfx != null)
                    return _buttonSfx.length;
                else
                    return 0f;
            }
        }

        /// <summary>
        /// Enforces singleton, survives scene loads, and caches the AudioSource.
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
        }

        /// <summary>
        /// Plays the button SFX via PlayOneShot so rapid clicks layer instead of cutting off.
        /// No-ops silently if no clip is assigned.
        /// </summary>
        public void PlayButtonSfx()
        {
            if (_buttonSfx != null)
                _audioSource.PlayOneShot(_buttonSfx);
        }

        /// <summary>
        /// Invokes <see cref="PlayButtonSfx"/> on the live singleton when present; otherwise logs a warning.
        /// </summary>
        /// <remarks>
        /// Use from UI controllers so button feedback stays consistent without duplicating null checks.
        /// </remarks>
        public static void TryPlayButtonSfx()
        {
            if (Instance != null)
            {
                Instance.PlayButtonSfx();
            }
            else
            {
                Debug.LogWarning("AudioManager.Instance is null; button SFX was skipped.");
            }
        }

        /// <summary>
        /// Gets or sets the UI master volume on the managed audio source.
        /// </summary>
        public float MasterVolume
        {
            get
            {
                return _audioSource.volume;
            }
            set
            {
                _audioSource.volume = Mathf.Clamp01(value);
            }
        }

        /// <summary>
        /// Returns current master volume when an audio manager is available.
        /// </summary>
        public static bool TryGetMasterVolume(out float volume)
        {
            if (Instance != null)
            {
                volume = Instance.MasterVolume;
                return true;
            }

            volume = 1f;
            return false;
        }

        /// <summary>
        /// Applies a new master volume when an audio manager is available.
        /// </summary>
        public static bool TrySetMasterVolume(float volume)
        {
            if (Instance != null)
            {
                Instance.MasterVolume = volume;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Plays a sound identified by <paramref name="soundKey"/> (see <see cref="OtherSfxEquipKey"/>, <see cref="OtherSfxUnequipKey"/>, <see cref="OtherSfxSwapKey"/>).
        /// </summary>
        /// <param name="soundKey">Which wardrobe interaction clip to play.</param>
        public void PlayOtherSFX(string soundKey)
        {
            AudioClip otherSound = null;

            switch (soundKey)
            {
                case OtherSfxEquipKey:
                    otherSound = _equipSfx;
                    break;
                case OtherSfxUnequipKey:
                    otherSound = _unequipSfx;
                    break;
                case OtherSfxSwapKey:
                    otherSound = _swapSfx;
                    break;
            }
            if (otherSound != null)
            {
                _audioSource.PlayOneShot(otherSound);
            }
        }
        /// <summary>
        /// Calls <see cref="PlayOtherSFX"/> when <see cref="Instance"/> exists; otherwise logs a warning.
        /// </summary>
        /// <param name="soundKey">Same keys as <see cref="PlayOtherSFX"/>.</param>
        public static void TryPlayOtherSFX(string soundKey)
        {
            if (Instance != null)
            {
                Instance.PlayOtherSFX(soundKey);
            }
            else
            {
                Debug.LogWarning("AudioManager.Instance is null; other SFX was skipped.");
            }
        }
    }
}
