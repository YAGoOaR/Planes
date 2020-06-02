using UnityEngine;

public class OnTurnBackExit : StateMachineBehaviour
{
    const float MAX_VELOCITY_COEF = 0.3f;
    const float MAX_ANGLE_COEF = 0.7f;
    const float VELOCITY_OFFSET = -0.3f;
    PlaneBehaviour planeBehaviour;
    Rigidbody2D planeRigidbody;
    Transform planeTransform;
    Vector3 velocity;
    Collider2D planeCollider;
    float timer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
        planeBehaviour = animator.GetComponent<PlaneBehaviour>();
        planeCollider = planeBehaviour.GetComponent<Collider2D>();
        planeRigidbody = planeBehaviour.GetComponent<Rigidbody2D>();
        planeTransform = planeBehaviour.transform;
        float velocityCoefficient = Mathf.Min(1 / planeRigidbody.velocity.magnitude * 10, MAX_VELOCITY_COEF);
        float angleCoefficient = Mathf.Max(-Mathf.Sin(planeTransform.rotation.eulerAngles.z / 180 * Mathf.PI) + 1, MAX_ANGLE_COEF);
        animator.SetFloat("speedMultiplier", velocityCoefficient * angleCoefficient);
        planeBehaviour.isTurningBack = true;
        planeRigidbody.isKinematic = true;
        planeRigidbody.freezeRotation = true;
        velocity = new Vector3(planeRigidbody.velocity.x, planeRigidbody.velocity.y, 0);
        planeBehaviour.switchAerofoilActive();
        planeBehaviour.switchBombsActive();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        planeRigidbody.velocity = velocity * Mathf.Cos(Mathf.Max(Mathf.PI * timer + VELOCITY_OFFSET, 0));
        timer += Time.deltaTime * stateInfo.speedMultiplier;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        planeBehaviour.GetComponent<Rigidbody2D>().isKinematic = false;
        planeRigidbody.freezeRotation = false;
        animator.SetBool("turningBack", false);
        planeBehaviour.switchAerofoilActive();
        planeBehaviour.switchBombsActive();
        planeBehaviour.isTurningBack = false;
    }

}
