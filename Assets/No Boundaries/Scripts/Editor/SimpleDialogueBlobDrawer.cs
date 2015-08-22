using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomPropertyDrawer(typeof(SimpleDialogue.Blob), true)]
public class SimpleDialogueBlobDrawer : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        float standardHeight = base.GetPropertyHeight(property, label);

        Rect leftoverPosition = new Rect(position);

        SerializedProperty speakerProperty = property.FindPropertyRelative("DisplayedSpeaker");
        SerializedProperty messageProperty = property.FindPropertyRelative("Message");

        property.isExpanded = EditorGUI.Foldout(
            CalculateRect(standardHeight, ref leftoverPosition),
            property.isExpanded,
            FoldoutLabel(speakerProperty.stringValue, messageProperty.stringValue));

        if (property.isExpanded)
        {
            SerializedProperty clearProperty = property.FindPropertyRelative("Clear");
            clearProperty.enumValueIndex = (int)(object)EditorGUI.EnumPopup(
                CalculateRect(standardHeight, ref leftoverPosition), 
                "Clear Rule", (SimpleDialogue.ClearType)(clearProperty.enumValueIndex));

            SerializedProperty speakerId = property.FindPropertyRelative("SpeakerId");

            SerializedProperty dialogue = property.FindPropertyRelative("Dialogue");
            if (dialogue.objectReferenceValue != null && ((SimpleDialogue)dialogue.objectReferenceValue).SpeakerIds != null)
            {

                speakerId.intValue = EditorGUI.Popup(
                    CalculateRect(standardHeight, ref leftoverPosition),
                    speakerId.intValue,
                    ((SimpleDialogue)dialogue.objectReferenceValue).SpeakerIds.Ids);
            }

            speakerProperty.stringValue = EditorGUI.TextField(
                CalculateRect(standardHeight, ref leftoverPosition), 
                "Displayed Speaker", speakerProperty.stringValue);

            messageProperty.stringValue = EditorGUI.TextArea(
                CalculateRect(standardHeight * 4, ref leftoverPosition),
                messageProperty.stringValue);

            SerializedProperty continueProperty = property.FindPropertyRelative("Continue");
            continueProperty.enumValueIndex = (int)(object)EditorGUI.EnumPopup(
                CalculateRect(standardHeight, ref leftoverPosition),
                "Continue Rule", (SimpleDialogue.ContinueType)(continueProperty.enumValueIndex));

            switch ((SimpleDialogue.ContinueType)continueProperty.enumValueIndex)
            {
            case SimpleDialogue.ContinueType.Event:
                SerializedProperty eventProperty = property.FindPropertyRelative("EventName");
                eventProperty.stringValue = EditorGUI.TextField(
                    CalculateRect(standardHeight, ref leftoverPosition),
                    "Event Name",
                    eventProperty.stringValue);
                break;

            case SimpleDialogue.ContinueType.Time:
                SerializedProperty timeProperty = property.FindPropertyRelative("Time");
                timeProperty.floatValue = EditorGUI.FloatField(
                    CalculateRect(standardHeight, ref leftoverPosition),
                    "Time to wait",
                    timeProperty.floatValue);
                break;

            case SimpleDialogue.ContinueType.Option:
                SerializedProperty options = property.FindPropertyRelative("Options");
                EditorGUI.PropertyField(leftoverPosition, options, true);
                break;

            default:
                break;
            }
        }
    }

    private string FoldoutLabel(string speaker, string message)
    {
        int index = message.IndexOf("\n");

        switch (speaker.Trim())
        {
        case "":
            if (index >= 0 && index < 48)
            {
                message = string.Format("{0}...", message.Substring(0, index));
            }
            else if (message.Length > 50)
            {
                message = string.Format("{0}...", message.Substring(0, 48));
            }
            return string.Format("\"{0}\"", message);

        default:
            if (speaker.Length > 15)
            {
                speaker = string.Format("{0}...", speaker.Substring(0, 13));
            }

            if (index >= 0 && index + speaker.Length < 48)
            {
                message = string.Format("{0}...", message.Substring(0, index));
            }
            else if (message.Length + speaker.Length > 50)
            {
                message = string.Format("{0}...", message.Substring(0, 48 - speaker.Length));
            }
            return string.Format("{0}: \"{1}\"", speaker, message);
        }
    }

    private Rect CalculateRect(float height, ref Rect input)
    {
        Rect rect = new Rect(input);
        rect.height = height;

        input.yMin += height;
        input.height -= height;

        return rect;
    }


    private const int BasicHeight = 9;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return base.GetPropertyHeight(property, label);
        }

        float extraLines = 0;
        switch ((SimpleDialogue.ContinueType)property.FindPropertyRelative("Continue").enumValueIndex)
        {
        case SimpleDialogue.ContinueType.Event:
        case SimpleDialogue.ContinueType.Time:
            extraLines++;
            break;

        case SimpleDialogue.ContinueType.Option:
            SerializedProperty options = property.FindPropertyRelative("Options");

            extraLines += 1;
            if (options.isExpanded)
            {
                extraLines += options.arraySize * 1.125f + 1.5f;
            }
            break;
        }

        return base.GetPropertyHeight(property, label) * (BasicHeight + extraLines);
    }
}
