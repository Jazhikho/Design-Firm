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

        private Button _sandboxButton;
        private Button _sandboxFButton;
        private Button _sandboxMButton;
        private bool SandboxSubMenuVis;
        private VisualElement _sandboxSubMenu;
        /// <summary>
        /// Caches UI Toolkit buttons and binds click handlers.
        /// </summary>
        private void OnEnable()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;

            _playButton = root.Q<Button>("PlayButton");
            _quitButton = root.Q<Button>("QuitButton");
            _sandboxButton = root.Q<Button>("SandboxButton");
            _sandboxSubMenu = root.Q<VisualElement>("SubMenu");
            if (_sandboxSubMenu != null)
            {
                _sandboxFButton = _sandboxSubMenu.Q<Button>("sandboxF");
                _sandboxMButton = _sandboxSubMenu.Q<Button>("sandboxM");
            }
            else
            {
                _sandboxFButton = null;
                _sandboxMButton = null;
            }

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

            if (_sandboxButton != null && _sandboxSubMenu != null && _sandboxFButton != null && _sandboxMButton != null)
            {
                _sandboxButton.RegisterCallback<ClickEvent>(OnSandboxClicked);
                _sandboxFButton.RegisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);
                _sandboxMButton.RegisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);
            }
            else
            {
                Debug.LogError(
                    "MainMenuController: Sandbox UI incomplete (SandboxButton, SubMenu, sandboxF, or sandboxM missing). Sandbox controls were not bound.");
            }

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

            if (_sandboxButton != null)
            {
                _sandboxButton.UnregisterCallback<ClickEvent>(OnSandboxClicked);
            }
            if (_sandboxFButton != null)
            {
                _sandboxFButton.UnregisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);
            }
            if (_sandboxMButton != null)
            {
                _sandboxMButton.UnregisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);
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

        /// <summary>
        /// Opens the sandbox gender choice submenu
        /// </summary>
        private void OnSandboxClicked(ClickEvent evt)
        {
            if (_sandboxSubMenu == null)
            {
                Debug.LogError("MainMenuController: Sandbox submenu is missing; cannot toggle visibility.");
                return;
            }

            TryPlayButtonSfx();
            if (SandboxSubMenuVis)
            {
                SandboxSubMenuVis = false;
                _sandboxSubMenu.visible = false;
            }
            else
            {
                SandboxSubMenuVis = true;
                _sandboxSubMenu.visible = true;
            }
        }

        /// <summary>
        /// Creates a sandbox dummy scenario for the active scenario; then sends the player to the wardrobe screen.
        /// </summary>
        private void OnSandboxSubMenuButtonClick(ClickEvent evt)
        {
            if (!SandboxSubMenuVis)
            {
                return;
            }

            Button button = evt.currentTarget as Button;
            if (button == null)
            {
                Debug.LogError("MainMenuController: Sandbox gender click did not resolve to a Button.");
                return;
            }

            Data.Scenario sandboxScenario = new();
            sandboxScenario.name = "sandbox";
            sandboxScenario.description = button.name;
            if (button.name == "sandboxF")
            {
                sandboxScenario.avatarImage = "Avatars/2000sFemModel.png";
            }
            else
            {
                sandboxScenario.avatarImage = "Avatars/MascModel.png";
            }

            ScenarioState.Instance.ActiveScenario = sandboxScenario;
            TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.WardrobeScene);
        }
    }
}
