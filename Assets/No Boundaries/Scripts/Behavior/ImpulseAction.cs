using UnityEngine;
using System.Collections;

public class ImpulseAction : StateMachineBehaviour
{
    public string DirectionParameter;
    public string SpeedMultiplierParameter;

    public float Speed;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NadineControllerV2 controller = animator.GetComponentInParent<NadineControllerV2>();
        if (controller == null) return;

        controller.Physics.SetVelocity(animator.GetFloat(DirectionParameter), animator.GetFloat(SpeedMultiplierParameter) * Speed);
    }
}
