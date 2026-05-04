using Assets.Scripts.Core;
using Assets.Scripts.Data;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        private VisualElement _root;
        private Button _playButton;
        private Button _quitButton;

        private Button _sandboxButton;
        private Button _sandboxFButton;
        private Button _sandboxMButton;

        private Button _settingsButton;
        private VisualElement _sandboxSubMenu;

        private Button _creditsButton;

        /// <summary>
        /// Caches UI Toolkit buttons and binds click handlers.
        /// </summary>
        private void OnEnable()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;

            _playButton = _root.Q<Button>("PlayButton");
            _playButton?.RegisterCallback<ClickEvent>(OnPlayClicked);

            _sandboxButton = _root.Q<Button>("SandboxButton");
            _sandboxButton?.RegisterCallback<ClickEvent>(OnSandboxClicked);

            _sandboxSubMenu = _root.Q<VisualElement>("SubMenu");
            _sandboxSubMenu.visible = false;

            _sandboxFButton = _root.Q<Button>("sandboxF");
            _sandboxFButton?.RegisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);

            _sandboxMButton = _root.Q<Button>("sandboxM");
            _sandboxMButton?.RegisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);

            _settingsButton = _root.Q<Button>("SettingsButton");
            _settingsButton?.RegisterCallback<ClickEvent>(OnSettingsButtonClick);

            _creditsButton = _root.Q<Button>("CreditsButton");
            _creditsButton?.RegisterCallback<ClickEvent>(OnCreditsButtonClick);

            _quitButton = _root.Q<Button>("QuitButton");
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                _quitButton.style.display = DisplayStyle.None;
            else
                _quitButton?.RegisterCallback<ClickEvent>(OnQuitClicked);
        }

        /// <summary>
        /// Unbinds click handlers when this component disables.
        /// </summary>
        private void OnDisable()
        {
            _playButton?.UnregisterCallback<ClickEvent>(OnPlayClicked);
            _sandboxButton?.UnregisterCallback<ClickEvent>(OnSandboxClicked);
            _sandboxFButton?.UnregisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);
            _sandboxMButton?.UnregisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);
            _settingsButton?.UnregisterCallback<ClickEvent>(OnSettingsButtonClick);
            _creditsButton?.UnregisterCallback<ClickEvent>(OnCreditsButtonClick);

            if (_quitButton != null && Application.platform != RuntimePlatform.WebGLPlayer)
                _quitButton.UnregisterCallback<ClickEvent>(OnQuitClicked);
        }

        /// <summary>
        /// Opens the credits modal and refocuses the close control for gamepad/keyboard.
        /// </summary>
        private void OnCreditsButtonClick(ClickEvent _)
        {
            AudioManager.Instance.PlayButtonSfx();
            NavigationManager.Instance.NavigateToScene(GameConstants.CreditsScene);
        }

        /// <summary>
        /// Opens the task scenario scene from the main menu.
        /// </summary>
        private void OnPlayClicked(ClickEvent e)
        {
            AudioManager.TryPlayButtonSfx();
            NavigationManager.Instance.NavigateToScene(GameConstants.ScenarioScene);
        }

        /// <summary>
        /// Exits the application or stops play mode in editor.
        /// </summary>
        private void OnQuitClicked(ClickEvent e)
        {
            AudioManager.TryPlayButtonSfx();
            StartCoroutine(QuitAfterSfx());
        }

        /// <summary>
        /// Waits for the configured button SFX length, then exits play mode or the built player.
        /// </summary>
        /// <returns>Coroutine iterator steps for Unity.</returns>
        private IEnumerator QuitAfterSfx()
        {
            float waitSeconds = 0f;
            if (AudioManager.Instance != null)
                waitSeconds = AudioManager.Instance.ButtonSfxLength;

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

            AudioManager.TryPlayButtonSfx();
            _sandboxSubMenu.visible = !_sandboxSubMenu.visible;
        }

        /// <summary>
        /// Creates a sandbox dummy scenario for the active scenario; then sends the player to the wardrobe screen.
        /// </summary>
        private void OnSandboxSubMenuButtonClick(ClickEvent evt)
        {
            if (!_sandboxSubMenu.visible) return;

            Button button = evt.currentTarget as Button;
            if (button == null)
            {
                Debug.LogError("MainMenuController: Sandbox gender click did not resolve to a Button.");
                return;
            }

            Scenario sandboxScenario = new()
            {
                name = "sandbox",
                description = button.name
            };
            if (button.name == "sandboxF")
                sandboxScenario.avatarImage = "Avatars/2000sFemModel.png";
            else
                sandboxScenario.avatarImage = "Avatars/MascModel.png";

            ScenarioState.Instance.ActiveScenario = sandboxScenario;
            AudioManager.TryPlayButtonSfx();
            NavigationManager.Instance.NavigateToScene(GameConstants.WardrobeScene);
        }

        private void OnSettingsButtonClick(ClickEvent evt)
        {
            AudioManager.TryPlayButtonSfx();
            NavigationManager.Instance.NavigateToScene(GameConstants.SettingsScene);
        }
    }
}
