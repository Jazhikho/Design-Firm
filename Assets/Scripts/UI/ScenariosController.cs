using Assets.Scripts.Core;
using Assets.Scripts.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ScenariosController : MonoBehaviour
    {
        private Button _backButton;
        private ScrollView _scenariosList;
        private VisualElement _root;
        private ToggleButtonGroup _decadeToggleGroup;
        private ToggleButtonGroup _categoryToggleGroup;
        private List<string> _decades;
        private List<string> _categories;
        private Label _lblErrorMessage;

        /// <summary>
        /// Caches scenario screen buttons, binds handlers, and subscribes to scenario data loading.
        /// </summary>
        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _lblErrorMessage = _root.Q<Label>("lblErrorMessage");
            EnsureStatusLabelExists();

            _backButton = _root.Q<Button>("btnBack");
            if (_backButton == null)
            {
                Debug.LogError("ScenariosController: btnBack not found in UXML.");
                return;
            }
            _backButton.RegisterCallback<ClickEvent>(BackScene);

            _scenariosList = _root.Q<ScrollView>("scrollScenariosList");

            if (ScenarioState.Instance.IsScenariosLoaded)
            {
                RefreshViewState();
            }
            else
            {
                ScenarioState.Instance.ScenariosLoaded += OnScenariosLoaded;
            }
        }

        /// <summary>
        /// Unbinds handlers and unsubscribes from the loading event.
        /// </summary>
        private void OnDisable()
        {
            if (_backButton != null)
            {
                _backButton.UnregisterCallback<ClickEvent>(BackScene);
            }

            if (_decadeToggleGroup != null)
            {
                _decadeToggleGroup.UnregisterValueChangedCallback(OnFilterChanged);
            }

            if (_categoryToggleGroup != null)
            {
                _categoryToggleGroup.UnregisterValueChangedCallback(OnFilterChanged);
            }

            ScenarioState.Instance.ScenariosLoaded -= OnScenariosLoaded;
        }

        private void OnScenariosLoaded()
        {
            Debug.Log("ScenariosController: received ScenariosLoaded event.");
            RefreshViewState();
        }

        private void RefreshViewState()
        {
            ClearDynamicUi();

            if (StartupServices.HasCriticalFailure)
            {
                ShowStatus(
                    "Failed to load scenario data.\n\n" +
                    StartupServices.LastCriticalError,
                    isError: true);
                return;
            }

            if (!StartupServices.ScenariosLoaded)
            {
                ShowStatus("Loading scenarios...");
                return;
            }

            if (ScenarioState.Instance.Scenarios == null || ScenarioState.Instance.Scenarios.Count == 0)
            {
                ShowStatus("No scenarios were loaded.", isError: true);
                return;
            }

            HideStatus();
            RenderScenarioUi();
        }
        private void RenderScenarioUi()
        {
            VisualElement decadeFilters = _root.Q<VisualElement>("decadeFilters");
            if (decadeFilters == null)
            {
                Debug.LogError("ScenariosController: decadeFilters not found in UXML.");
                ShowStatus("Scenario UI is misconfigured: decade filters missing.", isError: true);
                return;
            }

            VisualElement categoryFilters = _root.Q<VisualElement>("categoryFilters");
            if (categoryFilters == null)
            {
                Debug.LogError("ScenariosController: categoryFilters not found in UXML.");
                ShowStatus("Scenario UI is misconfigured: category filters missing.", isError: true);
                return;
            }

            DisplayDecadeFilters(decadeFilters);
            DisplayCategoryFilters(categoryFilters);
            DisplayScenarios();

            Debug.Log($"ScenariosController: rendered scenario UI with {ScenarioState.Instance.Scenarios.Count} scenarios.");
        }
        private void ClearDynamicUi()
        {
            _scenariosList?.Clear();

            VisualElement decadeFilters = _root?.Q<VisualElement>("decadeFilters");
            decadeFilters?.Clear();

            VisualElement categoryFilters = _root?.Q<VisualElement>("categoryFilters");
            categoryFilters?.Clear();

            _decadeToggleGroup = null;
            _categoryToggleGroup = null;
        }

        private void EnsureStatusLabelExists()
        {
            if (_lblErrorMessage != null)
            {
                return;
            }

            _lblErrorMessage = new Label
            {
                name = "lblErrorMessage"
            };
            _lblErrorMessage.AddToClassList("status-label");
            _root.Add(_lblErrorMessage);
        }

        private void ShowStatus(string message, bool isError = false)
        {
            EnsureStatusLabelExists();
            _lblErrorMessage.text = message;
            _lblErrorMessage.style.display = DisplayStyle.Flex;

            if (isError)
            {
                _lblErrorMessage.AddToClassList("status-label-error");
            }
            else
            {
                _lblErrorMessage.RemoveFromClassList("status-label-error");
            }

            Debug.Log($"ScenariosController: status shown. Error={isError}. Message={message}");
        }

        private void HideStatus()
        {
            if (_lblErrorMessage != null)
            {
                _lblErrorMessage.style.display = DisplayStyle.None;
            }
        }

        private void OnFilterChanged(ChangeEvent<ToggleButtonGroupState> evt)
        {
            DisplayScenarios();
        }

        private HashSet<string> GetActiveFilters(ToggleButtonGroup group, List<string> values)
        {
            HashSet<string> active = new();

            if (group == null || values == null || values.Count == 0)
            {
                return active;
            }

            ToggleButtonGroupState state = group.value;
            for (int i = 0; i < values.Count; i++)
            {
                if (state[i])
                {
                    active.Add(values[i]);
                }
            }
            return active;
        }

        private void DisplayScenarios()
        {
            _scenariosList.Clear();

            IEnumerable<Scenario> filtered = ScenarioState.Instance.Scenarios;

            HashSet<string> activeDecades = GetActiveFilters(_decadeToggleGroup, _decades);
            if (activeDecades.Count > 0)
            {
                filtered = filtered.Where(s => activeDecades.Contains(s.era));
            }

            HashSet<string> activeCategories = GetActiveFilters(_categoryToggleGroup, _categories);
            if (activeCategories.Count > 0)
            {
                filtered = filtered.Where(s => activeCategories.Contains(s.category));
            }

            int count = 0;

            foreach (Scenario scenario in filtered)
            {
                VisualElement card = new();
                card.AddToClassList("scenario-card");

                Label nameLabel = new(scenario.name);
                nameLabel.AddToClassList("scenario-card-name");
                card.Add(nameLabel);

                Label descriptionLabel = new(scenario.description);
                descriptionLabel.AddToClassList("scenario-card-description");
                card.Add(descriptionLabel);

                Button selectButton = new() { text = ">>" };
                selectButton.AddToClassList("scenario-card-select");
                selectButton.clicked += () => SelectScenario(scenario);
                card.Add(selectButton);

                _scenariosList.Add(card);
                count++;
            }

            if (count == 0)
            {
                ShowStatus("No scenarios match the selected filters.");
            }
            else
            {
                HideStatus();
            }
        }

        private void SelectScenario(Scenario scenario)
        {
            if (scenario == null)
            {
                Debug.LogError("ScenariosController: attempted to select a null scenario.");
                return;
            }

            ScenarioState.Instance.ActiveScenario = scenario;
            Debug.Log($"ScenariosController: selected scenario '{scenario.name}'.");
            SceneManager.LoadScene(GameConstants.WardrobeScene);
        }

        /// <summary>
        /// Returns from scenario selection to the main menu.
        /// </summary>
        private void BackScene(ClickEvent e)
        {
            AudioManager.TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.MainMenuScene);
        }

        /// <summary>
        /// Adds toggle buttons to act as filters for each decade.
        /// </summary>
        /// <param name="decadeFilters">The VisualElement container for decade filter buttons.</param>
        private void DisplayDecadeFilters(VisualElement decadeFilters)
        {
            _decades = ScenarioState.Instance.Scenarios
                .Select(s => s.era)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            _decadeToggleGroup = new ToggleButtonGroup()
            {
                allowEmptySelection = true
            };
            _decadeToggleGroup.AddToClassList("filter-toggle-group");
            _decadeToggleGroup.RegisterValueChangedCallback(OnFilterChanged);
            decadeFilters.Add(_decadeToggleGroup);

            foreach (string decade in _decades)
            {
                Button decadeButton = new()
                {
                    text = decade,
                    name = $"toggle{decade}"
                };
                decadeButton.AddToClassList("filter-button");
                _decadeToggleGroup.Add(decadeButton);
            }
        }

        /// <summary>
        /// Adds toggle buttons to act as filters for each category.
        /// </summary>
        /// <param name="categoryFilters">The VisualElement container for category filter buttons.</param>
        private void DisplayCategoryFilters(VisualElement categoryFilters)
        {
            _categories = ScenarioState.Instance.Scenarios
                .Select(s => s.category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            _categoryToggleGroup = new ToggleButtonGroup()
            {
                allowEmptySelection = true
            };
            _categoryToggleGroup.AddToClassList("filter-toggle-group");
            _categoryToggleGroup.RegisterValueChangedCallback(OnFilterChanged);
            categoryFilters.Add(_categoryToggleGroup);

            foreach (string category in _categories)
            {
                Button categoryButton = new()
                {
                    text = category,
                    name = $"toggle{category}"
                };
                categoryButton.AddToClassList("filter-button");
                _categoryToggleGroup.Add(categoryButton);
            }
        }
    }
}
