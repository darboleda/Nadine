using UnityEngine;
using System.Collections;

public class RoomEntrance : MonoBehaviour
{
    public string Id;
    public CameraConstrainer StartingConstraint;

    public Coroutine Transition(float transitionTime)
    {
        return StartCoroutine(DoTransition(transitionTime));
    }

    private IEnumerator DoTransition(float transitionTime)
    {
        StartingConstraint.Activate(Camera.main);
        StartingConstraint.Constrain();

        CameraController controller = Camera.main.GetComponent<CameraController>();
        controller.enabled = false;
        ClockMaster cm = ClockMaster.Find();
        Clock clock = null;
        if (cm != null)
        {
            clock = cm.GetClock("Game");
        }

        if (clock != null)
        {
            clock.Speed = 0;
        }

        MasterSpriteSorter.ForceUpdate();
        while (transitionTime > 0)
        {
            controller.Jump(Time.deltaTime / transitionTime);
            transitionTime -= Time.deltaTime;
            yield return null;
        }

        controller.Jump(1);
        controller.enabled = true;
        if (clock != null)
        {
            clock.Speed = 1;
        }
    }
}
