using UnityEngine;
using UnityEditor;

public static class ScriptableObjectUtility
{
    
    /// <summary>
    /// Create new asset from <see cref="ScriptableObject"/> type with unique name at
    /// selected folder in project window. Asset creation can be cancelled by pressing
    /// escape key when asset is initially being named.
    /// </summary>
    /// <typeparam name="T">Type of scriptable object.</typeparam>
    public static void CreateAsset<T>() where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        ProjectWindowUtil.CreateAsset(asset, "New " + typeof(T).Name + ".asset");
    }

    [MenuItem("Assets/Create/State Map")]
    public static void CreateStateMap() {
        ScriptableObjectUtility.CreateAsset<StateMap>();
    }

    [MenuItem("Assets/Create/Update Map")]
    public static void CreateUpdateMap() {
        ScriptableObjectUtility.CreateAsset<UpdateMap>();
    }

    [MenuItem("Assets/Create/Collectibles/Collectible Info")]
    public static void CreateCollectibleInfo() {
        ScriptableObjectUtility.CreateAsset<CollectibleInfo>();
    }

    [MenuItem("Assets/Create/Collectibles/Countable Info")]
    public static void CreateCountableInfo() {
        ScriptableObjectUtility.CreateAsset<CountableInfo>();
    }

    [MenuItem("Assets/Create/Dialogue/Simple Dialogue")]
    public static void CreateSimpleDialogue() {
        ScriptableObjectUtility.CreateAsset<SimpleDialogue>();
    }

    [MenuItem("Assets/Create/Dialogue/Speakers")]
    public static void CreateDialogueSpeakers() {
        ScriptableObjectUtility.CreateAsset<DialogueSpeakers>();
    }
}
