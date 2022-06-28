using UnityEngine;

public class ExplosionAnim : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.Destroy(animator.gameObject);
    }
}
