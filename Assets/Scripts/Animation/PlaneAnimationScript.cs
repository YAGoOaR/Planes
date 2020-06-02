using UnityEngine;

public class PlaneAnimationScript : StateMachineBehaviour
{
    //Event after plane turn animation
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("turning", false);
    }
}
