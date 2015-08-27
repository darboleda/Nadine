using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using System.IO;
using System.Linq;

[CustomEditor(typeof(DefinedRooms))]
public class DefinedRoomsEditor : Editor
{
    private ReorderableList list;
    public void OnEnable()
    {
        list = new UnityEditorInternal.ReorderableList(serializedObject, serializedObject.FindProperty("Scenes"), true, false, true, true);
        list.drawElementCallback = (position, index, isActive, isFocused) => {
            position.y += 2;
            position.height -= 5;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(position, element);
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Set Scenes"))
        {
            EditorBuildSettings.scenes = ((DefinedRooms)target).Scenes
                .Where(x => x != null).Select(x => AssetDatabase.GetAssetPath(x))
                .Where(x => Path.GetExtension(x).ToLower() == ".unity").Select(x => new EditorBuildSettingsScene(x, true))
                .ToArray();
        }
    }
}

[CustomPropertyDrawer(typeof(SceneAttribute))]
public class SceneFileDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = (property.objectReferenceValue != null ? new GUIContent(property.objectReferenceValue.name) : label);
        Object picked = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(DefaultAsset), false);
        string path = AssetDatabase.GetAssetPath(picked);
        if (Path.GetExtension(path).ToLower() != ".unity") picked = null;
        property.objectReferenceValue = picked;
    }
}
