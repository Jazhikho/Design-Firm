using System.Collections;
using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        private Button _playButton;
        private Button _quitButton;

        /// <summary>
        /// Caches UI Toolkit buttons and binds click handlers.
        /// </summary>
        private void OnEnable()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;

            _playButton = root.Q<Button>("PlayButton");
            _quitButton = root.Q<Button>("QuitButton");
            if (_playButton == null)
            {
                Debug.LogError("MainMenuScript: PlayButton not found in UXML.");
                return;
            }

            if (_quitButton == null)
            {
                Debug.LogError("MainMenuScript: QuitButton not found in UXML.");
                return;
            }

            _playButton.RegisterCallback<ClickEvent>(OnPlayClicked);

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                _quitButton.style.display = DisplayStyle.None;
            }
            else
            {
                _quitButton.RegisterCallback<ClickEvent>(OnQuitClicked);
            }
        }

        /// <summary>
        /// Unbinds click handlers when this component disables.
        /// </summary>
        private void OnDisable()
        {
            if (_playButton != null)
            {
                _playButton.UnregisterCallback<ClickEvent>(OnPlayClicked);
            }

            if (_quitButton != null && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                _quitButton.UnregisterCallback<ClickEvent>(OnQuitClicked);
            }
        }

        /// <summary>
        /// Opens the task scenario scene from the main menu.
        /// </summary>
        private void OnPlayClicked(ClickEvent e)
        {
            TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.TaskScenarioScene);
        }

        /// <summary>
        /// Exits the application or stops play mode in editor.
        /// </summary>
        private void OnQuitClicked(ClickEvent e)
        {
            TryPlayButtonSfx();
            StartCoroutine(QuitAfterSfx());
        }

        /// <summary>
        /// Plays the main menu button click sound when an AudioManager is present in the scene.
        /// </summary>
        private void TryPlayButtonSfx()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonSfx();
            }
            else
            {
                Debug.LogWarning(
                    "MainMenuController: AudioManager.Instance is null; button SFX was skipped.");
            }
        }

        /// <summary>
        /// Waits for the configured button SFX length, then exits play mode or the built player.
        /// </summary>
        /// <returns>Coroutine iterator steps for Unity.</returns>
        private IEnumerator QuitAfterSfx()
        {
            float waitSeconds = 0f;
            if (AudioManager.Instance != null)
            {
                waitSeconds = AudioManager.Instance.ButtonSfxLength;
            }

            yield return new WaitForSeconds(waitSeconds);
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}