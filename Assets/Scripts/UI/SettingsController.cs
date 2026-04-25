using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingsController : MonoBehaviour
    {
        private Button _mainMenuButton;
        private VisualElement _root;
        private Slider _slider;

        private void OnEnable()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;
            UiDocumentPanelRootStretch.ApplyToPanelRootAndAppShell(_root);
            _mainMenuButton = _root.Q<Button>("MainMenuButton");
            _slider = _root.Q<Slider>("MasterSlider");

            if (_mainMenuButton != null)
            {
                _mainMenuButton.RegisterCallback<ClickEvent>(OnBackButton);
            }
            else
            {
                Debug.LogError("SettingsController: Could not find back button.");
            }

            if (_slider != null)
            {
                _slider.RegisterCallback<ChangeEvent<float>>(SliderValueChange);
                if (AudioManager.TryGetMasterVolume(out float masterVolume))
                {
                    _slider.value = masterVolume * 100f;
                }
                else
                {
                    Debug.LogWarning("SettingsController: AudioManager.Instance is null. Master volume slider defaulted to 100.");
                    _slider.value = 100f;
                }
            }
            else
            {
                Debug.LogError("SettingsController: Could not find master slider.");
            }
        }

        private void OnDisable()
        {
            if (_mainMenuButton != null)
            {
                _mainMenuButton.UnregisterCallback<ClickEvent>(OnBackButton);
            }

            if (_slider != null)
            {
                _slider.UnregisterCallback<ChangeEvent<float>>(SliderValueChange);
            }
        }

        private void OnBackButton(ClickEvent evt)
        {
            AudioManager.TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.MainMenuScene);
        }

        private void SliderValueChange(ChangeEvent<float> evt)
        {
            float sliderValue = evt.newValue * 0.01f;
            if (!AudioManager.TrySetMasterVolume(sliderValue))
            {
                Debug.LogWarning("SettingsController: AudioManager.Instance is null; master volume change was skipped.");
            }
        }
    }
}
