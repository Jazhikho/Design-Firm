using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ScenariosController : MonoBehaviour
{
    private Button _backButton;
    private Button _nextButton;

    /// <summary>
    /// Caches scenario screen buttons and binds handlers.
    /// </summary>
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        _backButton = root.Q<Button>("btnBack");
        _nextButton = root.Q<Button>("btnNext");
        if (_backButton == null)
        {
            Debug.LogError("ScenariosController: btnBack not found in UXML.");
            return;
        }

        if (_nextButton == null)
        {
            Debug.LogError("ScenariosController: btnNext not found in UXML.");
            return;
        }

        _backButton.clicked += BackScene;
        _nextButton.clicked += NextScene;
    }

    /// <summary>
    /// Unbinds scenario screen button handlers.
    /// </summary>
    private void OnDisable()
    {
        if (_backButton != null)
        {
            _backButton.clicked -= BackScene;
        }

        if (_nextButton != null)
        {
            _nextButton.clicked -= NextScene;
        }
    }

    /// <summary>
    /// Returns from scenario selection to the main menu.
    /// </summary>
    private void BackScene()
    {
        SceneManager.LoadScene(GameConstants.MainMenuScene);
    }

    /// <summary>
    /// Advances from scenario selection to wardrobe scene.
    /// </summary>
    private void NextScene()
    {
        SceneManager.LoadScene(GameConstants.WardrobeScene);
    }
}
