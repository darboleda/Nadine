using UnityEngine;
using System.Collections;

using Rewired;

public class NadineAcceptActions : StateMachineBehaviour
{

    public bool AllowDirectionChange = true;
    public bool ChangeAnimationDirection = true;
    public bool AllowSpeedChange = true;
    public bool AllowBurst = true;
    public bool AllowRoll = true;

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
        
        float physicsDirection = controller.Physics.Direction;
        float facingAngle = controller.Animator.FacingAngle;
        if (speed > 0)
        {
            physicsDirection = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            facingAngle = physicsDirection;
        }

        if (AllowSpeedChange)
        {
            controller.Physics.Speed = speed;
            controller.Animator.SetSpeed(speed);
        }

        if (AllowDirectionChange)
        {
            controller.Physics.Direction = physicsDirection;
            if (ChangeAnimationDirection)
            {
                controller.Animator.SetDirection(facingAngle);
            }
        }

        // TODO implement stamina plan
        // Only allow set number of rolls and/or bursts in a row.
        // Use state machine behavior to refill stamina on exit of recovery
        if (AllowBurst && controller.Input.GetBurstInput())
        {
            controller.Animator.StartShieldAttack(0);
        }
        else if (AllowRoll && controller.Input.GetRollInput())
        {
            controller.Roll(movement);
        }
    }
}
