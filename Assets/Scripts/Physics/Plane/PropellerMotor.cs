using UnityEngine;
//A script that controls plane motor
public class PropellerMotor : MonoBehaviour
{
    const float VELOCITY_OFFSET = 5;
    public float force = 5.5f;
    Rigidbody2D propellerRigidbody;
    Animator propellerAnimator;
    FixedJoint2D fixedJoint;
    public int throttle;
    public bool jointIsActive = true;

    void Start()
    {
        fixedJoint = gameObject.GetComponent<FixedJoint2D>();
        propellerAnimator = gameObject.GetComponent<Animator>();
        propellerRigidbody = gameObject.GetComponent<Rigidbody2D>();
    }

    void OnJointBreak2D()
    {
        jointIsActive = false;
    }

    void FixedUpdate()
    {
        if (jointIsActive)
        {
            propellerAnimator.SetFloat("Throttle", throttle);
            propellerAnimator.SetFloat("Velocity", Mathf.Sqrt(propellerRigidbody.velocity.magnitude * throttle) + VELOCITY_OFFSET);
            float ang = (gameObject.transform.rotation.eulerAngles.z - 180) / 180 * Mathf.PI;
            Vector2 v = new Vector2(Mathf.Cos(ang) * force * throttle, Mathf.Sin(ang) * force * throttle);
            propellerRigidbody.AddForce(v);
        }
        else
        {
            propellerAnimator.enabled = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag != "water" || propellerRigidbody.velocity.magnitude < 10 || !jointIsActive) return;
        throttle = 0;
        fixedJoint.breakForce = 0;
    }
}
