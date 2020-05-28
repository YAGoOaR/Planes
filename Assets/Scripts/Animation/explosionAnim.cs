using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explosionAnim : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.Destroy(animator.gameObject);
    }

}
