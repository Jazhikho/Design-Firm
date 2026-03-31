using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class MainMenuScript : MonoBehaviour
{
    private Button playButton;
    private Button quitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playButton = root.Q<Button>("PlayButton");
        quitButton = root.Q<Button>("QuitButton");
    }

    void OnPlayClicked()
    {
        Debug.Log("Play Clicked");
    }

    void OnQuitClicked()
    {
        Debug.Log("Quit Clicked");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
