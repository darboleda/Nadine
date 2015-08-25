using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(SpecialDirection))]
public class SpecialDirectionDrawer : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float height = base.GetPropertyHeight(property, label);

        switch(DrawType(ConsumeRect(height, false, ref position), property, true))
        {
        case SpecialDirection.DirectionType.Constant:
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel += 1;
            DrawDirection(ConsumeRect(height, false, ref position), property, true);
            EditorGUI.indentLevel = indentLevel;
            break;
        }
    }

    private Rect ConsumeRect(float value, bool horizontal, ref Rect source)
    {
        Rect rect = new Rect(source);
        if (horizontal)
        {
            source.x += value;
            source.width = rect.width - value;
            rect.width = value;
            return rect;
        }
        else
        {
            source.y += value;
            source.height = rect.height - value;
            rect.height = value;
            return rect;
        }
    }

    private SpecialDirection.DirectionType DrawType(Rect position, SerializedProperty property, bool showLabel)
    {
        SerializedProperty type = property.FindPropertyRelative("Type");

        if (showLabel)
        {
            type.enumValueIndex = (int)(object)EditorGUI.EnumPopup(position, "Direction Type", (SpecialDirection.DirectionType)type.enumValueIndex);
        }
        else
        {
            type.enumValueIndex = (int)(object)EditorGUI.EnumPopup(position, (SpecialDirection.DirectionType)type.enumValueIndex);
        }
        return (SpecialDirection.DirectionType)type.enumValueIndex;
    }

    private void DrawDirection(Rect position, SerializedProperty property, bool showLabel)
    {
        SerializedProperty direction = property.FindPropertyRelative("Direction");

        if (showLabel)
        {
            EditorGUI.PropertyField(position, direction, new GUIContent("Direction"), true);
        }
        else
        {
            EditorGUI.PropertyField(position, direction, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        switch ((SpecialDirection.DirectionType)property.FindPropertyRelative("Type").enumValueIndex)
        {
        case SpecialDirection.DirectionType.Constant:
            return base.GetPropertyHeight(property, label) * 2;

        default:
            return base.GetPropertyHeight(property, label);
        }
    }
}
