using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ScenariosController : MonoBehaviour
{
    private Button btnBack;

    private Button btnNext;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        btnBack = root.Q<Button>("btnBack");
        btnBack.clicked += BackScene;

        btnNext = root.Q<Button>("btnNext");
        btnNext.clicked += NextScene;
    }

    private void OnDisable()
    {
        btnBack.clicked -= BackScene;
        btnNext.clicked -= NextScene;
    }

    private void BackScene()
    {
        SceneManager.LoadScene("mainMenuScene"); // TODO: replace scene name with game constant
    }

    private void NextScene()
    {
        SceneManager.LoadScene("wardrobeScene"); // TODO: replace scene name with game constant
    }
}
