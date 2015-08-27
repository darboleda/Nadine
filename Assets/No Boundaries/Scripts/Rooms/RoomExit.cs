using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomExit : MonoBehaviour
{
    public RoomEntryInfo TargetRoom;
    public float TransitionTime = 1f;

    [HideInInspector]
    public string EntranceId;

    public void Exit()
    {
        if (enabled)
        {
            StartCoroutine(AsyncExit(TransitionTime));
        }
    }

    private IEnumerator AsyncExit(float cameraTransitionTime)
    {
        GameObject currentRoom = GameObject.FindGameObjectWithTag("Room");
        Application.LoadLevelAdditive(TargetRoom.Scene);
        yield return null;

        List<Room> rooms = new List<Room>();
        foreach (GameObject roomContainer in GameObject.FindGameObjectsWithTag("Room"))
        {
            Room room = roomContainer.GetComponent<Room>();
            rooms.Add(room);
        }

        foreach (Room room in rooms)
        {
            foreach (RoomExit exit in room.Exits)
            {
                exit.enabled = false;
            }
        }

        foreach (Room room in rooms)
        {
            if (room.EntryInfo == TargetRoom)
            {
                for (int i = 0; i < room.Entrances.Length; i++)
                {
                    if (room.Entrances[i].Id == EntranceId)
                    {
                        room.transform.position =  room.transform.position + (transform.position - room.Entrances[i].transform.position);
                        yield return room.Entrances[i].Transition(cameraTransitionTime);
                        break;
                    }
                }
                break;
            }
        }

        foreach (Room room in rooms)
        {
            foreach (RoomExit exit in room.Exits)
            {
                exit.enabled = true;
            }
        }

        if (currentRoom != null)
        {
            GameObject.DestroyObject(currentRoom);
        }
    }
}
