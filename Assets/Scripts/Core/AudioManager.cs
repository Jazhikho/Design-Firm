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

        private AudioSource _audioSource;

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
    }
}
