using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ResultsController : MonoBehaviour
{
    private Button btnRetry;
    private Button btnMainMenu;
    private Button btnRetryScenario;

    public void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        btnRetry = root.Q<Button>("btnRetry");
        btnRetry.clicked += GoToWardrobe;

        btnMainMenu = root.Q<Button>("btnMainMenu");
        btnMainMenu.clicked += GoToMainMenu;

        btnRetryScenario = root.Q<Button>("btnNewScenario");
        btnRetryScenario.clicked += NewScenario;
    }

    public void OnDisable()
    {
        btnRetry.clicked -= GoToWardrobe;
        btnMainMenu.clicked -= GoToMainMenu;
        btnRetryScenario.clicked -= NewScenario;
    }

    private void GoToWardrobe()
    {
        SceneManager.LoadScene("wardrobeScene"); // TODO: replace scene name with game constant
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("mainMenuScene"); // TODO: replace scene name with game constant
    }

    private void NewScenario()
    {
        SceneManager.LoadScene("taskScenarioScene"); // TODO: replace scene name with game constant
    }
}
