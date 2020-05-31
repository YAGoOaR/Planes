using UnityEngine;

public class GearIsSwitched : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<GearController>().gearSwitched(); 
    }
}
