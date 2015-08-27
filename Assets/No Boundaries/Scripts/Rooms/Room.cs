using UnityEngine;
using System.Collections;

public class Room : MonoBehaviour
{
    public RoomEntryInfo EntryInfo;
    public RoomEntrance[] Entrances;
    public RoomExit[] Exits;

    public void Awake()
    {
        gameObject.tag = "Room";
    }
}
