using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private Color fadeColor = new(0.12f, 0.03f, 0.12f, 1f);
    [SerializeField] private float fadeDuration = 0.35f;

    private UIDocument _uiDocument;
    private VisualElement _fadeOverlay;
    private bool _isNavigating;

    private void Awake()
    {
        Instance = this;
        _uiDocument = GetComponent<UIDocument>();
        _isNavigating = true; // block navigation until fade-in completes
    }

    private void Start()
    {
        // Create overlay in Start() — after UIDocument.OnEnable() has populated
        // the root, so the overlay is the last child and renders on top.
        CreateFadeOverlay();

        // Always fade in: start fully opaque so the first render shows solid color,
        // then animate to transparent.
        _fadeOverlay.style.opacity = 1f;
        _fadeOverlay.style.display = DisplayStyle.Flex;
        _fadeOverlay.pickingMode = PickingMode.Position;
        StartCoroutine(FadeIn());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void NavigateToScene(string sceneName)
    {
        if (_isNavigating) return;
        StartCoroutine(NavigateRoutine(sceneName));
    }

    public void NavigateToScene(int sceneBuildIndex)
    {
        if (_isNavigating) return;
        StartCoroutine(NavigateRoutine(sceneBuildIndex));
    }

    private IEnumerator NavigateRoutine(string sceneName)
    {
        _isNavigating = true;
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator NavigateRoutine(int sceneBuildIndex)
    {
        _isNavigating = true;
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneBuildIndex);
    }

    private IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
        _isNavigating = false;
    }

    private void CreateFadeOverlay()
    {
        VisualElement root = _uiDocument.rootVisualElement;

        _fadeOverlay = new VisualElement { name = "NavigationFadeOverlay" };
        _fadeOverlay.style.position = Position.Absolute;
        _fadeOverlay.style.left = 0;
        _fadeOverlay.style.right = 0;
        _fadeOverlay.style.top = 0;
        _fadeOverlay.style.bottom = 0;
        _fadeOverlay.style.backgroundColor = fadeColor;
        _fadeOverlay.style.opacity = 0f;
        _fadeOverlay.pickingMode = PickingMode.Ignore;

        root.Add(_fadeOverlay);
    }

    private IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        _fadeOverlay.style.display = DisplayStyle.Flex;
        _fadeOverlay.pickingMode = PickingMode.Position;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            _fadeOverlay.style.opacity = Mathf.Lerp(fromAlpha, toAlpha, eased);
            yield return null;
        }

        _fadeOverlay.style.opacity = toAlpha;

        if (Mathf.Approximately(toAlpha, 0f))
        {
            _fadeOverlay.pickingMode = PickingMode.Ignore;
            _fadeOverlay.style.display = DisplayStyle.None;
        }
    }
}