using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core;
using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private Button _settingsButton;
        private bool SandboxSubMenuVis;
        private VisualElement _sandboxSubMenu;

        private Button _creditsButton;
        private Button _creditsCloseButton;
        private VisualElement _creditsOverlay;
        private ScrollView _creditsScroll;
        private bool _creditsOpen;
        private VisualElement _root;
        private ScrollView _mainMenuScroll;

        /// <summary>
        /// Caches UI Toolkit buttons and binds click handlers.
        /// </summary>
        private void OnEnable()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;
            _root = root;
            root.style.flexGrow = 1f;
            root.style.flexShrink = 0f;
            root.style.minHeight = 0f;
            root.style.width = new Length(100f, LengthUnit.Percent);
            root.style.height = new Length(100f, LengthUnit.Percent);

            RegisterMainMenuContentViewportLayout(root);
            RegisterCreditsInterface(root);
            if (_root != null)
            {
                _root.RegisterCallback<NavigationCancelEvent>(OnRootNavigationCancel, TrickleDown.TrickleDown);
            }

            _playButton = root.Q<Button>("PlayButton");
            _quitButton = root.Q<Button>("QuitButton");
            _sandboxButton = root.Q<Button>("SandboxButton");
            _sandboxSubMenu = root.Q<VisualElement>("SubMenu");
            _settingsButton = root.Q<Button>("SettingsButton");
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
            if (_settingsButton != null)
            {
                _settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClick);
            }
            else
            {
                Debug.LogError("MainMenuController: Settings Button not found.");
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
            if (_mainMenuScroll != null)
            {
                _mainMenuScroll.UnregisterCallback<GeometryChangedEvent>(OnMainMenuScrollGeometryChanged);
            }
            _mainMenuScroll = null;
            if (_root != null)
            {
                _root.UnregisterCallback<NavigationCancelEvent>(OnRootNavigationCancel, TrickleDown.TrickleDown);
            }
            _root = null;
            SetMainMenuButtonsEnabledForModal(false);
            _creditsOpen = false;
            if (_creditsOverlay != null)
            {
                _creditsOverlay.style.display = DisplayStyle.None;
            }
            UnregisterCreditsInterface();
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
            if (_settingsButton != null)
            {
                _settingsButton.UnregisterCallback<ClickEvent>(OnSettingsButtonClick);
            }

            if (_quitButton != null && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                _quitButton.UnregisterCallback<ClickEvent>(OnQuitClicked);
            }
        }

        /// <summary>
        /// When the main menu is shown, Escape closes the credits overlay if it is open.
        /// Uses the Input System package, consistent with active input handling in Player Settings.
        /// </summary>
        private void Update()
        {
            if (!_creditsOpen)
            {
                return;
            }
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CloseCredits();
            }
        }

        /// <summary>
        /// Routes UI Toolkit navigation cancel (e.g. gamepad B) to closing the credits overlay when it is open.
        /// </summary>
        private void OnRootNavigationCancel(NavigationCancelEvent evt)
        {
            if (!_creditsOpen)
            {
                return;
            }
            CloseCredits();
            evt.StopPropagation();
        }

        /// <summary>
        /// Sets the main menu content column to at least the scroll viewport height so layout can stretch the stack
        /// between the top and bottom of the view (insets on the scroll view sit against the screen edges).
        /// </summary>
        private void RegisterMainMenuContentViewportLayout(VisualElement root)
        {
            ScrollView menuScroll = root.Q<ScrollView>("MenuScroll");
            if (menuScroll == null)
            {
                return;
            }
            _mainMenuScroll = menuScroll;
            _mainMenuScroll.RegisterCallback<GeometryChangedEvent>(OnMainMenuScrollGeometryChanged);
            SyncMainMenuContentWrapToViewport();
        }

        private void OnMainMenuScrollGeometryChanged(GeometryChangedEvent evt)
        {
            SyncMainMenuContentWrapToViewport();
        }

        private void SyncMainMenuContentWrapToViewport()
        {
            if (_root == null)
            {
                return;
            }
            ScrollView menuScroll = _root.Q<ScrollView>("MenuScroll");
            VisualElement wrap = _root.Q<VisualElement>("MainMenuContentWrap");
            if (menuScroll == null || wrap == null)
            {
                return;
            }
            float h = 0f;
            if (menuScroll.contentViewport != null)
            {
                h = menuScroll.contentViewport.resolvedStyle.height;
            }
            if (h < 1f)
            {
                h = menuScroll.resolvedStyle.height;
            }
            if (h > 1f)
            {
                wrap.style.minHeight = h;
            }
        }

        /// <summary>
        /// Locates the credits UI, fills the scroll view from <see cref="CreditsCatalog"/>, and registers handlers.
        /// </summary>
        private void RegisterCreditsInterface(VisualElement root)
        {
            _creditsButton = root.Q<Button>("CreditsButton");
            _creditsCloseButton = root.Q<Button>("CreditsCloseButton");
            _creditsOverlay = root.Q<VisualElement>("CreditsOverlay");
            _creditsScroll = root.Q<ScrollView>("CreditsScroll");
            if (_creditsButton == null || _creditsCloseButton == null || _creditsOverlay == null || _creditsScroll == null)
            {
                Debug.LogError("MainMenuController: Credits UI incomplete (CreditsButton, CreditsCloseButton, CreditsOverlay, or CreditsScroll missing).");
                return;
            }
            _creditsOpen = false;
            _creditsOverlay.pickingMode = PickingMode.Position;
            _creditsOverlay.style.position = Position.Absolute;
            _creditsOverlay.style.left = 0f;
            _creditsOverlay.style.top = 0f;
            _creditsOverlay.style.right = 0f;
            _creditsOverlay.style.bottom = 0f;
            _creditsOverlay.style.display = DisplayStyle.None;
            _creditsButton.RegisterCallback<ClickEvent>(OnCreditsButtonClick);
            _creditsCloseButton.RegisterCallback<ClickEvent>(OnCreditsCloseButtonClick);
            _creditsOverlay.RegisterCallback<ClickEvent>(OnCreditsOverlayClick, TrickleDown.TrickleDown);
            BuildCreditsScrollContent();
        }

        /// <summary>
        /// Removes credits-related callbacks; called when the component disables.
        /// </summary>
        private void UnregisterCreditsInterface()
        {
            if (_creditsButton != null)
            {
                _creditsButton.UnregisterCallback<ClickEvent>(OnCreditsButtonClick);
            }
            if (_creditsCloseButton != null)
            {
                _creditsCloseButton.UnregisterCallback<ClickEvent>(OnCreditsCloseButtonClick);
            }
            if (_creditsOverlay != null)
            {
                _creditsOverlay.UnregisterCallback<ClickEvent>(OnCreditsOverlayClick, TrickleDown.TrickleDown);
            }
            _creditsButton = null;
            _creditsCloseButton = null;
            _creditsOverlay = null;
            _creditsScroll = null;
        }

        /// <summary>
        /// Fills the credits scroll with a centered game-style team list (name/role) and subsections; third-party
        /// lines appear only when <see cref="CreditsCatalog.ThirdPartyAttributions"/> has entries.
        /// </summary>
        private void BuildCreditsScrollContent()
        {
            if (_creditsScroll == null)
            {
                return;
            }
            _creditsScroll.contentContainer.Clear();
            bool firstTeamSection = true;
            foreach ((string teamHeading, IReadOnlyList<string> teamLines) in CreditsCatalog.TeamByFunctionSections)
            {
                AddCreditsSectionHeading(_creditsScroll.contentContainer, teamHeading, firstTeamSection);
                firstTeamSection = false;
                foreach (string line in teamLines)
                {
                    AddNameRoleOrSingleLine(_creditsScroll.contentContainer, line);
                }
            }
            AddCreditsSectionHeading(_creditsScroll.contentContainer, CreditsCatalog.AiAssistanceSectionHeading, false);
            foreach (string line in CreditsCatalog.AiAssistanceLines)
            {
                AddNameRoleOrSingleLine(_creditsScroll.contentContainer, line);
            }
            IReadOnlyList<string> thirdParty = CreditsCatalog.ThirdPartyAttributions;
            if (thirdParty.Count > 0)
            {
                AddCreditsSectionHeading(_creditsScroll.contentContainer, CreditsCatalog.ThirdPartySectionHeading, false);
                foreach (string line in thirdParty)
                {
                    if (line == null)
                    {
                        continue;
                    }
                    string t = line.Trim();
                    if (t.Length == 0)
                    {
                        continue;
                    }
                    Label row = new Label(t);
                    row.AddToClassList("credits-credit-line");
                    _creditsScroll.contentContainer.Add(row);
                }
            }
        }

        /// <summary>
        /// Adds a centered, all-caps section label for the credits panel.
        /// </summary>
        private void AddCreditsSectionHeading(VisualElement parent, string title, bool isFirst)
        {
            Label heading = new Label(title);
            heading.AddToClassList("credits-section-heading");
            if (isFirst)
            {
                heading.AddToClassList("credits-section-heading-first");
            }
            parent.Add(heading);
        }

        /// <summary>
        /// Adds a name and role in two lines when the line contains the catalog em-dash separator; otherwise one centered line.
        /// </summary>
        private void AddNameRoleOrSingleLine(VisualElement parent, string line)
        {
            if (line == null)
            {
                return;
            }
            string trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                return;
            }
            int idx = trimmed.IndexOf(CreditsCatalog.NameRoleSeparator, StringComparison.Ordinal);
            if (idx < 0)
            {
                Label one = new Label(trimmed);
                one.AddToClassList("credits-credit-line");
                parent.Add(one);
                return;
            }
            string namePart = trimmed.Substring(0, idx).Trim();
            int sepLen = CreditsCatalog.NameRoleSeparator.Length;
            string rolePart = trimmed.Substring(idx + sepLen).Trim();
            VisualElement block = new VisualElement();
            block.AddToClassList("credits-credit-block");
            if (namePart.Length > 0)
            {
                Label nameLabel = new Label(namePart);
                nameLabel.AddToClassList("credits-credit-name");
                block.Add(nameLabel);
            }
            if (rolePart.Length > 0)
            {
                Label roleLabel = new Label(rolePart);
                roleLabel.AddToClassList("credits-credit-role");
                block.Add(roleLabel);
            }
            parent.Add(block);
        }

        /// <summary>
        /// Opens the credits modal and refocuses the close control for gamepad/keyboard.
        /// </summary>
        private void OnCreditsButtonClick(ClickEvent _)
        {
            OpenCredits();
        }

        /// <summary>
        /// Dismisses the credits overlay when Close is used.
        /// </summary>
        private void OnCreditsCloseButtonClick(ClickEvent _)
        {
            CloseCredits();
        }

        /// <summary>
        /// Closes the credits overlay when the dimmed backdrop is pressed (not the panel content).
        /// </summary>
        private void OnCreditsOverlayClick(ClickEvent evt)
        {
            if (evt.target != _creditsOverlay)
            {
                return;
            }
            CloseCredits();
        }

        /// <summary>
        /// Shows the credits overlay and hides the sandbox flyout.
        /// </summary>
        private void OpenCredits()
        {
            if (_creditsOverlay == null || _creditsScroll == null)
            {
                return;
            }
            AudioManager.TryPlayButtonSfx();
            if (SandboxSubMenuVis && _sandboxSubMenu != null)
            {
                SandboxSubMenuVis = false;
                _sandboxSubMenu.visible = false;
            }
            _creditsOpen = true;
            SetMainMenuButtonsEnabledForModal(true);
            _creditsOverlay.BringToFront();
            _creditsOverlay.style.display = DisplayStyle.Flex;
            if (_creditsCloseButton != null)
            {
                _creditsCloseButton.Focus();
            }
        }

        /// <summary>
        /// Hides the credits overlay and returns focus to the main Credits menu button.
        /// </summary>
        private void CloseCredits()
        {
            if (_creditsOverlay == null)
            {
                return;
            }
            if (!_creditsOpen)
            {
                return;
            }
            AudioManager.TryPlayButtonSfx();
            _creditsOpen = false;
            _creditsOverlay.style.display = DisplayStyle.None;
            SetMainMenuButtonsEnabledForModal(false);
            if (_creditsButton != null)
            {
                _creditsButton.Focus();
            }
        }

        /// <summary>
        /// Disables the primary main menu buttons so focus and navigation stay inside the credits modal while it is open.
        /// </summary>
        private void SetMainMenuButtonsEnabledForModal(bool isModalOpen)
        {
            bool isEnabled = !isModalOpen;
            if (_playButton != null)
            {
                _playButton.SetEnabled(isEnabled);
            }
            if (_quitButton != null)
            {
                _quitButton.SetEnabled(isEnabled);
            }
            if (_sandboxButton != null)
            {
                _sandboxButton.SetEnabled(isEnabled);
            }
            if (_settingsButton != null)
            {
                _settingsButton.SetEnabled(isEnabled);
            }
            if (_creditsButton != null)
            {
                _creditsButton.SetEnabled(isEnabled);
            }
        }

        /// <summary>
        /// Opens the task scenario scene from the main menu.
        /// </summary>
        private void OnPlayClicked(ClickEvent e)
        {
            AudioManager.TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.TaskScenarioScene);
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

            AudioManager.TryPlayButtonSfx();
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
            AudioManager.TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.WardrobeScene);
        }

        private void OnSettingsButtonClick(ClickEvent evt)
        {
            AudioManager.TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.SettingsScene);
        }
    }
}
