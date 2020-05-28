using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerMotor : MonoBehaviour
{

    public int num { get; private set; }
    private Rigidbody2D rb;
    private float force = 5f;
    private Animator propellerAnimator;
    private bool joint = true;
    private FixedJoint2D fixedJoint;

    void Start()
    {
        fixedJoint = gameObject.GetComponent<FixedJoint2D>();
        propellerAnimator = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void OnJointBreak2D()
    {
        joint = false;
    }


    public void throttle(int n)
    {
        if (joint)
        {
            num += n;

            if (num > 100) num = 100;
            else if (num < 0) num = 0;
        }

        else
        {
        }
    }
    public void SetThrottle(int n)
    {
        num = n;
    }

    void FixedUpdate()
    {
        if (joint)
        {
            propellerAnimator.SetFloat("Throttle", num);
            propellerAnimator.SetFloat("Velocity", Mathf.Sqrt(rb.velocity.magnitude * num) + 5f);
            float ang = (gameObject.transform.rotation.eulerAngles.z - 180) / 180 * Mathf.PI;
            Vector2 v = new Vector2(Mathf.Cos(ang) * force * num, Mathf.Sin(ang) * force * num);
            rb.AddForce(v);
        }
        else
        {
            propellerAnimator.enabled = false;
        }
    }

}
