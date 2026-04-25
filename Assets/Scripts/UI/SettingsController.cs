using System.Collections;
using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI{
    public class SettingsController : MonoBehaviour
    {
        private Button _MainMenuButton;
        private VisualElement _Root;
        private VisualElement _SliderElement;
        private Slider _Slider;

        private void OnEnable()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            _Root = uiDocument.rootVisualElement;
            _MainMenuButton = _Root.Q<Button>("MainMenuButton");
            _SliderElement = _Root.Q<VisualElement>("SliderElement");
            _Slider = _Root.Q<Slider>("MasterSlider");

            float SliderValue = AudioManager.Instance.gameObject.GetComponent<AudioSource>().volume;

            if (_MainMenuButton != null)
            {
                _MainMenuButton.RegisterCallback<ClickEvent>(OnBackButton);
            }
            else
            {
                Debug.LogError("SettingsController: Could not find back button.");
            }

            if (_Slider != null)
            {
                _Slider.RegisterCallback<ChangeEvent<float>>(SliderValueChange);
                _Slider.value = SliderValue * 100;
            }
            else
            {
                Debug.LogError("SettingsController: Could not find master slider.");
            }
        }
        private void OnDisable()
        {
            if (_MainMenuButton != null)
            {
                _MainMenuButton.UnregisterCallback<ClickEvent>(OnBackButton);
            }
        }

        private void OnBackButton(ClickEvent evt)
        {
            AudioManager.TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.MainMenuScene);
        }

        private void SliderValueChange(ChangeEvent<float> evt)
        {
            float slideVal = evt.newValue;
            AudioManager.Instance.gameObject.GetComponent<AudioSource>().volume = slideVal * 0.01f;
        }


    }
}
