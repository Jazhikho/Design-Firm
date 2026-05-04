using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Scripts.Core
{
    public enum SfxType
    {
        CommonButton,
        SubmitOutfit,
        IdealOutfitMatch,
        LessThanIdealOutfitMatch,
        EquipClothingItem,
        TimerEnd
    }

    [Serializable]
    public class SfxEntry
    {
        public SfxType Type;
        public AudioClip Clip;
    }

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

        private const string MasterVolumeParam = "MasterVolume";
        private const string MusicVolumeParam = "MusicVolume";
        private const string SfxVolumeParam = "SFXVolume";

        private const string MasterVolumeKey = "volume_master";
        private const string MusicVolumeKey = "volume_music";
        private const string SfxVolumeKey = "volume_sfx";

        private const float DefaultVolume = 1f;
        private const float MinDb = -80f;
        private const float MaxDb = 0f;

        [Header("Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup musicMixerGroup;

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private bool playMusicOnStart = true;

        [Header("SFX")]
        [SerializeField] private List<SfxEntry> sfxEntries = new();

        private readonly Dictionary<SfxType, AudioClip> _sfxClips = new();

        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        public float MasterVolume => PlayerPrefs.GetFloat(MasterVolumeKey, DefaultVolume);
        public float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
        public float SfxVolume => PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);

        public float CommonButtonSfxLength => GetSfxLength(SfxType.CommonButton);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = GetComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = 1f;
            _sfxSource.outputAudioMixerGroup = sfxMixerGroup;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = 1f;
            _musicSource.outputAudioMixerGroup = musicMixerGroup;

            BuildSfxLookup();
        }

        private void Start()
        {
            ApplySavedVolumes();

            if (playMusicOnStart)
                PlayBackgroundMusic();
        }

        private void OnValidate()
        {
            if (sfxEntries == null)
                return;

            HashSet<SfxType> seenTypes = new();

            foreach (SfxEntry entry in sfxEntries)
            {
                if (entry == null)
                    continue;

                if (!seenTypes.Add(entry.Type))
                    Debug.LogWarning($"Duplicate SFX entry found for {entry.Type}. Only one should exist.", this);
            }
        }

        private void BuildSfxLookup()
        {
            _sfxClips.Clear();

            foreach (SfxEntry entry in sfxEntries)
            {
                if (entry == null)
                    continue;

                if (entry.Clip == null)
                {
                    Debug.LogWarning($"SFX entry '{entry.Type}' has no AudioClip assigned.", this);
                    continue;
                }

                if (_sfxClips.ContainsKey(entry.Type))
                {
                    Debug.LogWarning($"Duplicate SFX entry '{entry.Type}' ignored.", this);
                    continue;
                }

                _sfxClips.Add(entry.Type, entry.Clip);
            }
        }

        public void PlaySfx(SfxType type)
        {
            if (_sfxSource == null)
            {
                Debug.LogWarning("SFX source is missing; SFX was skipped.", this);
                return;
            }

            if (!_sfxClips.TryGetValue(type, out AudioClip clip) || clip == null)
            {
                Debug.LogWarning($"Missing SFX clip for {type}.", this);
                return;
            }

            _sfxSource.PlayOneShot(clip);
        }

        public static void TryPlaySfx(SfxType type)
        {
            if (Instance == null)
            {
                Debug.LogWarning($"AudioManager.Instance is null; SFX '{type}' was skipped.");
                return;
            }

            Instance.PlaySfx(type);
        }

        public void PlayBackgroundMusic()
        {
            if (_musicSource == null)
            {
                Debug.LogWarning("Music source is missing; background music was skipped.", this);
                return;
            }

            if (backgroundMusic == null)
            {
                Debug.LogWarning("No background music clip assigned.", this);
                return;
            }

            if (_musicSource.clip == backgroundMusic && _musicSource.isPlaying)
                return;

            _musicSource.clip = backgroundMusic;
            _musicSource.Play();
        }

        public void StopBackgroundMusic()
        {
            if (_musicSource != null)
                _musicSource.Stop();
        }

        public void SetMasterVolume(float value)
        {
            SetVolume(MasterVolumeParam, MasterVolumeKey, value);
        }

        public void SetMusicVolume(float value)
        {
            SetVolume(MusicVolumeParam, MusicVolumeKey, value);
        }

        public void SetSfxVolume(float value)
        {
            SetVolume(SfxVolumeParam, SfxVolumeKey, value);
        }

        private void ApplySavedVolumes()
        {
            SetMixerVolume(MasterVolumeParam, MasterVolume);
            SetMixerVolume(MusicVolumeParam, MusicVolume);
            SetMixerVolume(SfxVolumeParam, SfxVolume);
        }

        private void SetVolume(string mixerParam, string prefsKey, float value)
        {
            value = Mathf.Clamp01(value);

            PlayerPrefs.SetFloat(prefsKey, value);
            PlayerPrefs.Save();

            SetMixerVolume(mixerParam, value);
        }

        private void SetMixerVolume(string mixerParam, float linearValue)
        {
            if (audioMixer == null)
            {
                Debug.LogWarning($"AudioMixer is not assigned. Could not set '{mixerParam}'.", this);
                return;
            }

            float dbValue = LinearToDecibels(linearValue);

            if (!audioMixer.SetFloat(mixerParam, dbValue))
                Debug.LogWarning($"AudioMixer parameter '{mixerParam}' was not found. Check exposed parameter name.", this);
        }

        private static float LinearToDecibels(float linearValue)
        {
            if (linearValue <= 0.0001f)
                return MinDb;

            return Mathf.Clamp(Mathf.Log10(linearValue) * 20f, MinDb, MaxDb);
        }

        private float GetSfxLength(SfxType type)
        {
            return _sfxClips.TryGetValue(type, out AudioClip clip) && clip != null
                ? clip.length
                : 0f;
        }
    }
}
