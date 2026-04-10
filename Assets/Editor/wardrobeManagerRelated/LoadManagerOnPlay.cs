#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor/play-from-scene helper: instantiates the wardrobe manager prefab when the active scene is not the main menu
/// so item JSON loads even if that scene was opened directly in the Editor.
/// </summary>
public static class LoadManagerOnPlay
{
    private const string WardrobeManagerPrefabPath = "Assets/Editor/wardrobeManagerRelated/WardrobeManager(Editor).prefab";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void LoadWardrobeManager()
    {
        if (GameConstants.MainMenuScene == SceneManager.GetActiveScene().name)
        {
            return;
        }

        Object wardrobeManagerPrefab = AssetDatabase.LoadAssetAtPath(WardrobeManagerPrefabPath, typeof(GameObject));
        if (wardrobeManagerPrefab != null)
        {
            Object.Instantiate(wardrobeManagerPrefab);
        }
        else
        {
            Debug.LogError("LoadManagerOnPlay: Wardrobe manager prefab not found at " + WardrobeManagerPrefabPath);
        }
    }
}
#endif
