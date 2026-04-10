#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

    /// <summary>
    /// Loads wardrobeManager in a scene where it does not exist.
    /// </summary>
public class loadManagerOnPlay
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void loadWardrobeManager()
    {
        if (GameConstants.MainMenuScene != SceneManager.GetActiveScene().name)
        {
            Object wardrobeManagerPrefab = AssetDatabase.LoadAssetAtPath("Assets/Editor/wardrobeManagerRelated/WardrobeManager(Editor).prefab", typeof(GameObject));
            if (wardrobeManagerPrefab != null)
            {
                GameObject.Instantiate(wardrobeManagerPrefab);
            }
            else
            {
                Debug.LogError("LoadManagerOnPlay: There is no Manager to Load");
            }
        }
    }
}

#endif