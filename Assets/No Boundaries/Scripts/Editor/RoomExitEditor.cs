using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(RoomExit))]
public class RoomExitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RoomExit exit = target as RoomExit;
        if (exit.TargetRoom == null) return;

        int index = -1;
        for (int i = 0; i < exit.TargetRoom.Entrances.Length; i++)
        {
            if (exit.TargetRoom.Entrances[i] == exit.EntranceId)
            {
                index = i;
                break;
            }
        }

        EditorGUI.BeginChangeCheck();
        index = EditorGUILayout.Popup("Entrance ID", index, exit.TargetRoom.Entrances);

        if (EditorGUI.EndChangeCheck())
        {
            if (index < 0 || index >= exit.TargetRoom.Entrances.Length)
            {
                exit.EntranceId = string.Empty;
            }
            else
            {
                exit.EntranceId = exit.TargetRoom.Entrances[index];
            }
        }
    }
}
