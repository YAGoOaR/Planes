using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerMotor : MonoBehaviour
{

    public int num { get; private set; }
    private Rigidbody2D propellerRB;
    private float force = 5f;
    private Animator propellerAnimator;
    private bool jointIsActive = true;
    private FixedJoint2D fixedJoint;

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

}
