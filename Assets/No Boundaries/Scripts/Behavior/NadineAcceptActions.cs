using UnityEngine;
using System.Collections;

using Rewired;

public class NadineAcceptActions : StateMachineBehaviour
{

    private NadineControllerV2 controller;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.GetComponent<NadineControllerV2>();
    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller == null)
        {
            return;
        }
        
        Vector3 movement = controller.Input.GetMovementAxes();
        float speed = (movement.sqrMagnitude < 0.001f ? 0 :
                       movement.sqrMagnitude > 1 ? 1 :
                       movement.magnitude);
        
        float direction = controller.Physics.Direction;
        if (speed > 0)
        {
            direction = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
        }
        
        controller.Physics.Speed = speed;
        controller.Physics.Direction = direction;
        
        if (controller.Input.GetBurstInput())
        {
            controller.Animator.StartShieldAttack(0);
        }
        
        controller.Animator.SetDirection(direction);
        controller.Animator.SetSpeed(speed);
    }
}
