using UnityEngine;

public class PropellerMotor : MonoBehaviour
{
    const float force = 5.3f;

    Rigidbody2D propellerRB;
    Animator propellerAnimator;
    FixedJoint2D fixedJoint;

    public int num { get; private set; }
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


    public void throttle(int n)
    {
        if (jointIsActive)
        {
            num += n;

            if (num > 100) num = 100;
            else if (num < 0) num = 0;
        }
    }
    public void SetThrottle(int n)
    {
        num = n;
    }

    void FixedUpdate()
    {
        if (jointIsActive)
        {
            propellerAnimator.SetFloat("Throttle", num);
            propellerAnimator.SetFloat("Velocity", Mathf.Sqrt(propellerRB.velocity.magnitude * num) + 5f);
            float ang = (gameObject.transform.rotation.eulerAngles.z - 180) / 180 * Mathf.PI;
            Vector2 v = new Vector2(Mathf.Cos(ang) * force * num, Mathf.Sin(ang) * force * num);
            propellerRB.AddForce(v);
        }
        else
        {
            propellerAnimator.enabled = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag != "water" || propellerRB.velocity.magnitude < 10 || !jointIsActive) return;
        num = 0;
        fixedJoint.breakForce = 0;
    }

}
