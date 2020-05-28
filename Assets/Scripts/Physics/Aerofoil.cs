using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aerofoil : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 fv;
    private float ang;
    private Vector2 rotationVector;
    private float coef = 1f;
    public bool lift = false;
    public float liftCoef = 0.1f;
    public int upside = 1;
    private float stallCoef = 70f;
    private float dragCoef = 0.001f;
    private float coefCoef = 0.7f;
    private float effectiveAngleOffset = -0.04f;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        ang = (transform.rotation.eulerAngles.z) / 180 * Mathf.PI;
        rotationVector = new Vector2(-Mathf.Cos(ang), -Mathf.Sin(ang));
        Vector2 vel = rb.velocity;

        float vang = Vector2.SignedAngle(Vector2.left, vel) / 180 * Mathf.PI;
        float dang = (-Vector2.SignedAngle(rotationVector, vel)) / 180 * Mathf.PI - effectiveAngleOffset;

        float drag = Mathf.Abs(Mathf.Sin(dang)) * vel.magnitude * dragCoef;

        float stall = 1f;

        if (vel.magnitude < stallCoef) { stall = vel.magnitude / stallCoef; };

        coef = Mathf.Abs(Mathf.Cos(dang)) * stall * coefCoef;

        Vector2 stabForce = new Vector2(-Mathf.Cos(ang), -Mathf.Sin(ang)) * vel.magnitude;

        Vector2 liftForce;

        if (lift)
        {
            liftForce = new Vector2(-Mathf.Sin(ang), Mathf.Cos(ang)) * Mathf.Sqrt(vel.magnitude) * liftCoef * upside;
        }
        else liftForce = Vector2.zero;

        rb.velocity = (vel * (1 - coef) + coef * (stabForce + liftForce)) * (1 - drag);
    }
}
