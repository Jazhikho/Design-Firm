using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ResultsController : MonoBehaviour
{
    private Button _retryButton;
    private Button _mainMenuButton;
    private Button _retryScenarioButton;

    /// <summary>
    /// Caches result screen buttons and binds handlers.
    /// </summary>
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        _retryButton = root.Q<Button>("btnRetry");
        _mainMenuButton = root.Q<Button>("btnMainMenu");
        _retryScenarioButton = root.Q<Button>("btnNewScenario");
        if (_retryButton == null)
        {
            Debug.LogError("ResultsController: btnRetry not found in UXML.");
            return;
        }

        if (_mainMenuButton == null)
        {
            Debug.LogError("ResultsController: btnMainMenu not found in UXML.");
            return;
        }

        if (_retryScenarioButton == null)
        {
            Debug.LogError("ResultsController: btnNewScenario not found in UXML.");
            return;
        }

        _retryButton.clicked += GoToWardrobe;
        _mainMenuButton.clicked += GoToMainMenu;
        _retryScenarioButton.clicked += NewScenario;
    }

    /// <summary>
    /// Unbinds result screen button handlers.
    /// </summary>
    private void OnDisable()
    {
        if (_retryButton != null)
        {
            _retryButton.clicked -= GoToWardrobe;
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.clicked -= GoToMainMenu;
        }

        if (_retryScenarioButton != null)
        {
            _retryScenarioButton.clicked -= NewScenario;
        }
    }

    /// <summary>
    /// Loads the wardrobe scene for another outfit attempt.
    /// </summary>
    private void GoToWardrobe()
    {
        SceneManager.LoadScene(GameConstants.WardrobeScene);
    }

    /// <summary>
    /// Returns to the main menu scene.
    /// </summary>
    private void GoToMainMenu()
    {
        SceneManager.LoadScene(GameConstants.MainMenuScene);
    }

    /// <summary>
    /// Loads another scenario prompt.
    /// </summary>
    private void NewScenario()
    {
        SceneManager.LoadScene(GameConstants.TaskScenarioScene);
    }
}
