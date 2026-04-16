using UnityEngine;

namespace Assets.Scripts.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioClip _buttonSfx;
        private AudioSource _audioSource;

        public float ButtonSfxLength => _buttonSfx != null ? _buttonSfx.length : 0f;

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

        public void PlayButtonSfx()
        {
            if (_buttonSfx != null)
                _audioSource.PlayOneShot(_buttonSfx);
        }
    }
}
