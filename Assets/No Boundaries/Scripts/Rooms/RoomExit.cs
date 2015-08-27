using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomExit : MonoBehaviour
{
    public RoomEntryInfo TargetRoom;

    [HideInInspector]
    public string EntranceId;

    public void Exit(params RoomTransitioner[] transitioners)
    {
        StartCoroutine(AsyncExit(transitioners));
    }

    private IEnumerator AsyncExit(params RoomTransitioner[] transitioners)
    {
        GameObject currentRoom = GameObject.FindGameObjectWithTag("Room");
        Application.LoadLevelAdditive(TargetRoom.Scene);
        yield return null;
        GameObject.DestroyObject(currentRoom);
        foreach (GameObject roomContainer in GameObject.FindGameObjectsWithTag("Room"))
        {
            Debug.Log(roomContainer.name);
            Room room = roomContainer.GetComponent<Room>();
            if (room.EntryInfo == TargetRoom)
            {
                for (int i = 0; i < room.Entrances.Length; i++)
                {
                    if (room.Entrances[i].Id == EntranceId)
                    {
                        room.Entrances[i].OnEntered(transitioners);
                        break;
                    }
                }
                break;
            }
        }
    }
}
