﻿using UnityEngine;
//A scripts that changes plane physics when it's performing a turn
public class OnTurnBackExit : StateMachineBehaviour
{
    const float MAX_VELOCITY_COEF = 0.3f;
    const float MIN_ANGLE_COEF = 0.9f;
    const float VELOCITY_OFFSET = -0.3f;
    PlaneBehaviour planeBehaviour;
    Rigidbody2D planeRigidbody;
    Vector3 velocity;
    Collider2D planeCollider;
    float timer;

    //Called on start of animation
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
        planeBehaviour = animator.GetComponent<PlaneBehaviour>();
        planeCollider = planeBehaviour.GetComponent<Collider2D>();
        planeRigidbody = planeBehaviour.GetComponent<Rigidbody2D>();
        Transform planeTransform = planeBehaviour.transform;
        float velocityCoefficient = Mathf.Min(1 / planeRigidbody.velocity.magnitude * 10, MAX_VELOCITY_COEF);
        float angleCoefficient = Mathf.Max(-Mathf.Sin(planeTransform.rotation.eulerAngles.z / 180 * Mathf.PI) + 1, MIN_ANGLE_COEF);
        animator.SetFloat("speedMultiplier", velocityCoefficient * angleCoefficient);
        planeBehaviour.IsTurningBack = true;
        planeRigidbody.isKinematic = true;
        planeRigidbody.freezeRotation = true;
        velocity = new Vector3(planeRigidbody.velocity.x, planeRigidbody.velocity.y, 0);
        planeBehaviour.switchAerofoilActive();
        planeBehaviour.switchBombsActive();
    }

    //Called each frame of animation
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        planeRigidbody.velocity = velocity * Mathf.Cos(Mathf.Max(Mathf.PI * timer + VELOCITY_OFFSET, 0));
        timer += Time.deltaTime * stateInfo.speedMultiplier;
    }

    //Called at the end of animation
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        planeBehaviour.GetComponent<Rigidbody2D>().isKinematic = false;
        planeRigidbody.freezeRotation = false;
        animator.SetBool("turningBack", false);
        planeBehaviour.switchAerofoilActive();
        planeBehaviour.switchBombsActive();
        planeBehaviour.IsTurningBack = false;
    }
}
