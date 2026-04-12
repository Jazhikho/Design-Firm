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
            SceneManager.LoadScene(GameConstants.TaskScenarioScene);
        }

        /// <summary>
        /// Exits the application or stops play mode in editor.
        /// </summary>
        private void OnQuitClicked(ClickEvent e)
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}