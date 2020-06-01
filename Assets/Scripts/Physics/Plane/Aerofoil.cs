using UnityEngine;

public class Aerofoil : MonoBehaviour
{
    const float stallCoef = 70f;
    const float dragCoef = 0.0005f;
    const float stabCoef = 0.8f;
    const float effectiveAngleOffset = -0.04f;

    Rigidbody2D rb;
    Vector2 rotationVector;

    public bool lift = false;
    public float liftForceCoef = 0.1f;
    public int upside = 1;

    float angle;
    float stabilisation = 1f;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        angle = (transform.rotation.eulerAngles.z) / 180 * Mathf.PI;
        rotationVector = new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle));
        Vector2 vel = rb.velocity;

        float vang = Vector2.SignedAngle(Vector2.left, vel) / 180 * Mathf.PI;
        float dang = (-Vector2.SignedAngle(rotationVector, vel)) / 180 * Mathf.PI - effectiveAngleOffset;

        float drag = Mathf.Abs(Mathf.Sin(dang)) * vel.magnitude * dragCoef;

        float stall = 1f;

        if (vel.magnitude < stallCoef) { stall = vel.magnitude / stallCoef; };

        stabilisation = Mathf.Abs(Mathf.Cos(dang)) * stall * stabCoef;

        Vector2 stabForce = new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle)) * vel.magnitude;

        Vector2 liftForce;

        if (lift)
        {
            liftForce = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * Mathf.Sqrt(vel.magnitude) * liftForceCoef * upside;
        }
        else liftForce = Vector2.zero;

        rb.velocity = (vel * (1 - stabilisation) + stabilisation * (stabForce + liftForce)) * (1 - drag);
    }
}
