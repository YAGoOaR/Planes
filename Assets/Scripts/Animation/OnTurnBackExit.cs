﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTurnBackExit : StateMachineBehaviour
{
    PlaneBehaviour pb;
    Rigidbody2D prb;
    Transform ptr;
    Vector3 velocity;
    Collider2D planeCollider;
    float timer;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
        pb = animator.gameObject.GetComponent<PlaneBehaviour>();
        planeCollider = pb.gameObject.GetComponent<Collider2D>();
        prb = pb.gameObject.GetComponent<Rigidbody2D>();
        ptr = pb.gameObject.transform;
        animator.SetFloat("speedMultiplier", Mathf.Min(1 / prb.velocity.magnitude * 10, 0.6f) * Mathf.Max(-Mathf.Sin(ptr.rotation.eulerAngles.z / 180 * Mathf.PI) + 1, 0.7f));
        pb.setTurningBack(true);
        prb.isKinematic = true;
        prb.freezeRotation = true;
        velocity = new Vector3(prb.velocity.x, prb.velocity.y, 0);
        pb.switchAerofoil();
        pb.switchBmb();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        prb.velocity = velocity * Mathf.Cos(Mathf.Max(Mathf.PI * timer - 0.3f, 0));
        timer += Time.deltaTime * stateInfo.speedMultiplier;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        pb.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        prb.freezeRotation = false;
        animator.SetBool("turningBack", false);
        pb.switchAerofoil();
        pb.switchBmb();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}