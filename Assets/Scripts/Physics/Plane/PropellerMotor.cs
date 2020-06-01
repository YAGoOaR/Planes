using UnityEngine;

public class PropellerMotor : MonoBehaviour
{
    const float force = 5.5f;
    Rigidbody2D propellerRB;
    Animator propellerAnimator;
    FixedJoint2D fixedJoint;
    public int throttle;
    public bool jointIsActive = true;

    void Start()
    {
        fixedJoint = gameObject.GetComponent<FixedJoint2D>();
        propellerAnimator = gameObject.GetComponent<Animator>();
        propellerRB = gameObject.GetComponent<Rigidbody2D>();
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
            propellerAnimator.SetFloat("Velocity", Mathf.Sqrt(propellerRB.velocity.magnitude * throttle) + 5f);
            float ang = (gameObject.transform.rotation.eulerAngles.z - 180) / 180 * Mathf.PI;
            Vector2 v = new Vector2(Mathf.Cos(ang) * force * throttle, Mathf.Sin(ang) * force * throttle);
            propellerRB.AddForce(v);
        }
        else
        {
            propellerAnimator.enabled = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag != "water" || propellerRB.velocity.magnitude < 10 || !jointIsActive) return;
        throttle = 0;
        fixedJoint.breakForce = 0;
    }

}
