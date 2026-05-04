using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingsController : MonoBehaviour
    {
        private UIDocument _uiDocument;

        private Button _mainMenuButton;

        private Slider _masterVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Slider _musicVolumeSlider;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("SettingsController requires a UIDocument.", this);
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;

            _mainMenuButton = root.Q<Button>("MainMenuButton");
            _mainMenuButton?.RegisterCallback<ClickEvent>(OnBackButton);

            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager.Instance is null. Settings sliders could not be initialized.", this);
                return;
            }

            _masterVolumeSlider = root.Q<Slider>("sliderMasterVolume");
            _sfxVolumeSlider = root.Q<Slider>("sliderSFXVolume");
            _musicVolumeSlider = root.Q<Slider>("sliderMusicVolume");

            SetupSlider(_masterVolumeSlider, AudioManager.Instance.MasterVolume);
            SetupSlider(_sfxVolumeSlider, AudioManager.Instance.SfxVolume);
            SetupSlider(_musicVolumeSlider, AudioManager.Instance.MusicVolume);

            _masterVolumeSlider?.RegisterValueChangedCallback(OnMasterChanged);
            _sfxVolumeSlider?.RegisterValueChangedCallback(OnSfxChanged);
            _musicVolumeSlider?.RegisterValueChangedCallback(OnMusicChanged);
        }

        private void OnDisable()
        {
            _mainMenuButton?.UnregisterCallback<ClickEvent>(OnBackButton);

            _masterVolumeSlider?.UnregisterValueChangedCallback(OnMasterChanged);
            _sfxVolumeSlider?.UnregisterValueChangedCallback(OnSfxChanged);
            _musicVolumeSlider?.UnregisterValueChangedCallback(OnMusicChanged);
        }

        private void OnBackButton(ClickEvent evt)
        {
            AudioManager.TryPlaySfx(SfxType.CommonButton);
            NavigationManager.Instance.NavigateToScene(GameConstants.MainMenuScene);
        }

        private void SetupSlider(Slider slider, float value)
        {
            if (slider == null) return;

            slider.lowValue = 0f;
            slider.highValue = 1f;
            slider.SetValueWithoutNotify(value);
        }

        private void OnMasterChanged(ChangeEvent<float> evt)
        {
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.SetMasterVolume(evt.newValue);
        }

        private void OnMusicChanged(ChangeEvent<float> evt)
        {
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.SetMusicVolume(evt.newValue);
        }

        private void OnSfxChanged(ChangeEvent<float> evt)
        {
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.SetSfxVolume(evt.newValue);
        }
    }
}
