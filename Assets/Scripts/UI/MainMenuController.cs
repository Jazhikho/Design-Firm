using Assets.Scripts.Core;
using Assets.Scripts.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private Button _creditsCloseButton;
        private VisualElement _creditsOverlay;
        private ScrollView _creditsScroll;
        private bool _creditsOpen;

        /// <summary>
        /// Caches UI Toolkit buttons and binds click handlers.
        /// </summary>
        private void OnEnable()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;

            RegisterCreditsInterface(_root);
            _root?.RegisterCallback<NavigationCancelEvent>(OnRootNavigationCancel, TrickleDown.TrickleDown);

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
            _root?.UnregisterCallback<NavigationCancelEvent>(OnRootNavigationCancel, TrickleDown.TrickleDown);
            _root = null;
            SetMainMenuButtonsEnabledForModal(false);
            _creditsOpen = false;
            if (_creditsOverlay != null)
                _creditsOverlay.style.display = DisplayStyle.None;

            UnregisterCreditsInterface();
            _playButton?.UnregisterCallback<ClickEvent>(OnPlayClicked);
            _sandboxButton?.UnregisterCallback<ClickEvent>(OnSandboxClicked);
            _sandboxFButton?.UnregisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);
            _sandboxMButton?.UnregisterCallback<ClickEvent>(OnSandboxSubMenuButtonClick);
            _settingsButton?.UnregisterCallback<ClickEvent>(OnSettingsButtonClick);

            if (_quitButton != null && Application.platform != RuntimePlatform.WebGLPlayer)
                _quitButton.UnregisterCallback<ClickEvent>(OnQuitClicked);
        }

        /// <summary>
        /// When the main menu is shown, Escape closes the credits overlay if it is open.
        /// Uses the Input System package, consistent with active input handling in Player Settings.
        /// </summary>
        private void Update()
        {
            if (!_creditsOpen) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame)
                CloseCredits();
        }

        /// <summary>
        /// Routes UI Toolkit navigation cancel (e.g. gamepad B) to closing the credits overlay when it is open.
        /// </summary>
        private void OnRootNavigationCancel(NavigationCancelEvent evt)
        {
            if (!_creditsOpen) return;

            CloseCredits();
            evt.StopPropagation();
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
            _creditsScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _creditsScroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
            _creditsScroll.contentContainer.style.minWidth = 0f;
            _creditsScroll.contentContainer.style.width = new Length(100f, LengthUnit.Percent);
            _creditsScroll.contentContainer.style.maxWidth = new Length(100f, LengthUnit.Percent);
            _creditsScroll.contentContainer.style.flexDirection = FlexDirection.Column;
            _creditsScroll.contentContainer.style.alignItems = Align.Stretch;
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
            _creditsButton?.UnregisterCallback<ClickEvent>(OnCreditsButtonClick);
            _creditsCloseButton?.UnregisterCallback<ClickEvent>(OnCreditsCloseButtonClick);
            _creditsOverlay?.UnregisterCallback<ClickEvent>(OnCreditsOverlayClick, TrickleDown.TrickleDown);

            _creditsButton = null;
            _creditsCloseButton = null;
            _creditsOverlay = null;
            _creditsScroll = null;
        }

        /// <summary>
        /// Fills the credits scroll with section headings and one label per name (no role sub-lines); third-party
        /// lines appear only when <see cref="CreditsCatalog.ThirdPartyAttributions"/> has entries.
        /// </summary>
        private void BuildCreditsScrollContent()
        {
            if (_creditsScroll == null) return;

            _creditsScroll.contentContainer.Clear();
            bool firstTeamSection = true;
            foreach ((string teamHeading, IReadOnlyList<string> teamLines) in CreditsCatalog.TeamByFunctionSections)
            {
                AddCreditsSectionHeading(_creditsScroll.contentContainer, teamHeading, firstTeamSection);
                firstTeamSection = false;
                foreach (string personName in teamLines)
                {
                    AddCreditsNameLine(_creditsScroll.contentContainer, personName);
                }
            }
            AddCreditsSectionHeading(_creditsScroll.contentContainer, CreditsCatalog.AiAssistanceSectionHeading, false);
            foreach (string toolName in CreditsCatalog.AiAssistanceNames)
            {
                AddCreditsNameLine(_creditsScroll.contentContainer, toolName);
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
                    Label row = new(t);
                    row.AddToClassList("credits-name-line");
                    _creditsScroll.contentContainer.Add(row);
                }
            }
        }

        /// <summary>
        /// Adds a centered, all-caps section label for the credits panel.
        /// </summary>
        private void AddCreditsSectionHeading(VisualElement parent, string title, bool isFirst)
        {
            Label heading = new(title);
            heading.AddToClassList("credits-section-heading");
            if (isFirst)
                heading.AddToClassList("credits-section-heading-first");

            parent.Add(heading);
        }

        /// <summary>
        /// Adds a single centered name line for credits (team, AI tools, or third-party text).
        /// </summary>
        private void AddCreditsNameLine(VisualElement parent, string name)
        {
            if (name == null) return;

            string trimmed = name.Trim();
            if (trimmed.Length == 0) return;

            Label row = new(trimmed);
            row.AddToClassList("credits-name-line");
            parent.Add(row);
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
            if (evt.target != _creditsOverlay) return;

            CloseCredits();
        }

        /// <summary>
        /// Shows the credits overlay and hides the sandbox flyout.
        /// </summary>
        private void OpenCredits()
        {
            if (_creditsOverlay == null || _creditsScroll == null) return;

            AudioManager.TryPlayButtonSfx();
            if (_sandboxSubMenu.visible && _sandboxSubMenu != null)
            {
                _sandboxSubMenu.visible = false;
            }
            _creditsOpen = true;
            SetMainMenuButtonsEnabledForModal(true);
            _creditsOverlay.BringToFront();
            _creditsOverlay.style.display = DisplayStyle.Flex;
            _creditsCloseButton?.Focus();
        }

        /// <summary>
        /// Hides the credits overlay and returns focus to the main Credits menu button.
        /// </summary>
        private void CloseCredits()
        {
            if (_creditsOverlay == null) return;

            if (!_creditsOpen) return;

            AudioManager.TryPlayButtonSfx();
            _creditsOpen = false;
            _creditsOverlay.style.display = DisplayStyle.None;
            SetMainMenuButtonsEnabledForModal(false);
            _creditsButton?.Focus();
        }

        /// <summary>
        /// Disables the primary main menu buttons so focus and navigation stay inside the credits modal while it is open.
        /// </summary>
        private void SetMainMenuButtonsEnabledForModal(bool isModalOpen)
        {
            bool isEnabled = !isModalOpen;

            _playButton?.SetEnabled(isEnabled);
            _quitButton?.SetEnabled(isEnabled);
            _sandboxButton?.SetEnabled(isEnabled);
            _settingsButton?.SetEnabled(isEnabled);
            _creditsButton?.SetEnabled(isEnabled);
        }

        /// <summary>
        /// Opens the task scenario scene from the main menu.
        /// </summary>
        private void OnPlayClicked(ClickEvent e)
        {
            AudioManager.TryPlayButtonSfx();
            NavigationManager.Instance.NavigateToScene(GameConstants.TaskScenarioScene);
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
