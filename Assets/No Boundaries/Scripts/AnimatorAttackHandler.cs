using UnityEngine;
using System.Collections;

public class AnimatorAttackHandler : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NadineController controller = animator.GetComponentInParent<NadineController>();
        if (controller == null) return;
        controller.BeginAttack();
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NadineController controller = animator.GetComponentInParent<NadineController>();
        if (controller == null) return;
        controller.EndAttack();
    }
}
