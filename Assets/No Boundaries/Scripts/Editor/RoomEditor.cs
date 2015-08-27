using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Room room = target as Room;
        room.gameObject.tag = "Room";

        if (GUILayout.Button("Update Entrances and Exits"))
        {
            room.Exits = room.GetComponentsInChildren<RoomExit>();
            room.Entrances = room.GetComponentsInChildren<RoomEntrance>();
            if (room.EntryInfo != null)
            {
                room.EntryInfo.Entrances = room.Entrances.Select(x => x.Id).ToArray();
            }
        }
    }
}
