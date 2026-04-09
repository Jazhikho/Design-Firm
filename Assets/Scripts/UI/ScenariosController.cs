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
        SceneManager.LoadScene(GameConstants.MainMenuScene);
    } 

    private void NextScene()
    {
        SceneManager.LoadScene(GameConstants.Wardrobe); 
    }
}
