using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleDialogue))]
public class SimpleDialogueEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SimpleDialogue dialogue = target as SimpleDialogue;
        foreach (SimpleDialogue.Blob blob in dialogue.Dialogue)
        {
            blob.Dialogue = dialogue;
        }
    }
}
