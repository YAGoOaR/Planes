using System;
using UnityEngine;
//A script that controls plane motor
public class PropellerMotor : MonoBehaviour
{
    const float VELOCITY_OFFSET = 5;

    Rigidbody2D propellerRigidbody;
    Animator propellerAnimator;
    FixedJoint2D fixedJoint;

    [SerializeField] float force = 5.5f;
    [SerializeField] float zeroForceSpeed = 10;
    [SerializeField] float maxForceSpeed = 8;

    int throttle;
    bool jointIsActive = true;

    public int Throttle
    {
        get { return throttle; }
        set { throttle = value; }
    }

    public bool JointIsActive
    {
        get { return jointIsActive; }
    }

    void Start()
    {
        fixedJoint = GetComponent<FixedJoint2D>();
        propellerAnimator = GetComponent<Animator>();
        propellerRigidbody = GetComponent<Rigidbody2D>();
    }

    void OnJointBreak2D()
    {
        jointIsActive = false;
        PlanePart part = GetComponent<PlanePart>();
        part.IsBroken = true;
    }

    float getMotorForce(float speed)
    {
        return speed < maxForceSpeed ? 1 
            : speed > zeroForceSpeed ? 0 
            : .5f + .5f * Mathf.Cos((speed - maxForceSpeed) / (zeroForceSpeed - maxForceSpeed) * Mathf.PI);
    }

    void FixedUpdate()
    {
        if (jointIsActive)
        {
            float vel = propellerRigidbody.velocity.magnitude;
            propellerAnimator.SetFloat("Throttle", throttle);
            propellerAnimator.SetFloat("Velocity", Mathf.Sqrt(vel * throttle) + VELOCITY_OFFSET);

            Vector2 v = -transform.right * force * getMotorForce(vel) * throttle;
            propellerRigidbody.AddForce(v);
        }
        else
        {
            propellerAnimator.enabled = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag != "water" || propellerRigidbody.velocity.magnitude < 10 || !jointIsActive)
        {
            return;
        }
        throttle = 0;
        fixedJoint.breakForce = 0;
    }
}
