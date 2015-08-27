using UnityEngine;
using System.Collections;

public class RoomEntrance : MonoBehaviour
{
    public string Id;
    public CameraConstrainer StartingConstraint;

    public virtual void OnEntered(RoomTransitioner[] transitioners)
    {
        foreach (RoomTransitioner transitioner in transitioners)
        {
            transitioner.transform.position = transform.position;
        }

        StartingConstraint.Activate(Camera.main);
        StartingConstraint.Constrain();
        Camera.main.GetComponent<CameraController>().Jump();
    }
}
