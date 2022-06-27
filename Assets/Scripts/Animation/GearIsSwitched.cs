using UnityEngine;

public class GearIsSwitched : StateMachineBehaviour
{
    //an event after gear animation is ended
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.transform.parent.GetComponent<GearController>().gearSwitched();
    }
}
