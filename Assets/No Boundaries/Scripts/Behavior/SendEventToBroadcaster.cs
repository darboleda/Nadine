using UnityEngine;
using System.Collections;

public class SendEventToBroadcaster : StateMachineBehaviour
{
    public string StateEnteredEvent;
    public string StateExitedEvent;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EventRebroadcaster broadcaster = animator.GetComponentInParent<EventRebroadcaster>();
        if (broadcaster == null) return;

        broadcaster.BroadcastEvent(StateEnteredEvent);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EventRebroadcaster broadcaster = animator.GetComponentInParent<EventRebroadcaster>();
        if (broadcaster == null) return;
        broadcaster.BroadcastEvent(StateExitedEvent);
    }
}
