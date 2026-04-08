using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuScript : MonoBehaviour
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

        _playButton.clicked += OnPlayClicked;
        _quitButton.clicked += OnQuitClicked;
    }

    /// <summary>
    /// Unbinds click handlers when this component disables.
    /// </summary>
    private void OnDisable()
    {
        if (_playButton != null)
        {
            _playButton.clicked -= OnPlayClicked;
        }

        if (_quitButton != null)
        {
            _quitButton.clicked -= OnQuitClicked;
        }
    }

    /// <summary>
    /// Opens the task scenario scene from the main menu.
    /// </summary>
    private void OnPlayClicked()
    {
        SceneManager.LoadScene(GameConstants.TaskScenarioScene);
    }

    /// <summary>
    /// Exits the application or stops play mode in editor.
    /// </summary>
    private void OnQuitClicked()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
