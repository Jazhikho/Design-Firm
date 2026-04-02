using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuScript : MonoBehaviour
{
    private Button playButton;
    private Button quitButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playButton = root.Q<Button>("PlayButton");
        quitButton = root.Q<Button>("QuitButton");

        playButton.clicked += OnPlayClicked;
        quitButton.clicked += OnQuitClicked;
    }

    void OnPlayClicked()
    {
        Debug.Log("Play Clicked");

        // Load your game scene
        SceneManager.LoadScene("taskScenarioScene");
    }

    void OnQuitClicked()
    {
        Debug.Log("Quit Clicked");

        // Quit the application
        Application.Quit();

#if UNITY_EDITOR
        // Stop play mode when testing in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}