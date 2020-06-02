using UnityEngine;

public class explosionAnim : StateMachineBehaviour
{
    //An event after bomb is exploded
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.Destroy(animator.gameObject);
    }
}
