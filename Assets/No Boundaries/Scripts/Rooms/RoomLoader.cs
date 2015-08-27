using UnityEngine;
using System.Collections;

public class RoomLoader : MonoBehaviour
{
    public void Start()
    {
        GetComponent<RoomExit>().Exit();
    }
}
