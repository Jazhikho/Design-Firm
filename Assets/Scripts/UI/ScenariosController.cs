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

        /// <summary>
        /// Caches scenario screen buttons, binds handlers, and subscribes to scenario data loading.
        /// </summary>
        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

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
                RefreshUI();
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
            RefreshUI();
        }

        private void RefreshUI()
        {
            VisualElement decadeFilters = _root.Q<VisualElement>("decadeFilters");
            DisplayDecadeFilters(decadeFilters);

            VisualElement categoryFilters = _root.Q<VisualElement>("categoryFilters");
            DisplayCategoryFilters(categoryFilters);

            DisplayScenarios();
        }

        private void OnFilterChanged(ChangeEvent<ToggleButtonGroupState> evt)
        {
            DisplayScenarios();
        }

        private HashSet<string> GetActiveFilters(ToggleButtonGroup group, List<string> values)
        {
            HashSet<string> active = new();
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

            HashSet<string> activeDecades = GetActiveFilters(_decadeToggleGroup, _decades);
            HashSet<string> activeCategories = GetActiveFilters(_categoryToggleGroup, _categories);

            IEnumerable<Scenario> filtered = ScenarioState.Instance.Scenarios;

            if (activeDecades.Count > 0)
            {
                filtered = filtered.Where(s => activeDecades.Contains(s.era));
            }

            if (activeCategories.Count > 0)
            {
                filtered = filtered.Where(s => activeCategories.Contains(s.category));
            }

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
            }
        }

        private void SelectScenario(Scenario scenario)
        {
            ScenarioState.Instance.ActiveScenario = scenario;
            SceneManager.LoadScene(GameConstants.WardrobeScene);
        }

        /// <summary>
        /// Returns from scenario selection to the main menu.
        /// </summary>
        private void BackScene(ClickEvent e)
        {
            SceneManager.LoadScene(GameConstants.MainMenuScene);
        }

        /// <summary>
        /// Adds toggle buttons to act as filters for each decade.
        /// </summary>
        /// <param name="decadeFilters">The VisualElement container for decade filter buttons.</param>
        private void DisplayDecadeFilters(VisualElement decadeFilters)
        {
            _decades = ScenarioState.Instance.Scenarios.Select(s => s.era).Distinct().ToList();

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
            _categories = ScenarioState.Instance.Scenarios.Select(s => s.category).Distinct().ToList();

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
