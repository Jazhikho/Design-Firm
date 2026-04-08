#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class loadManagerOnPlay
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void loadWardrobeManager()
    {
        Object wardrobeManagerPrefab = AssetDatabase.LoadAssetAtPath("Assets/Editor/wardrobeManagerRelated/WardrobeManager(Editor).prefab", typeof(GameObject));
        GameObject.Instantiate(wardrobeManagerPrefab);
    }
}

#endif