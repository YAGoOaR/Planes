using UnityEngine;

public class Aerofoil : MonoBehaviour
{
    const float STALL_COEF = 70f;
    const float DRAG_COEF = 0.0005f;
    const float STAB_COEF = 0.8f;
    const float EFFECTIVE_ANGLE_OFFSET = -0.04f;
    public bool lift = false;
    public float liftForceCoef = 0.1f;
    public int upside = 1;
    Rigidbody2D rb;
    Vector2 rotationVector;
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
        float dang = (-Vector2.SignedAngle(rotationVector, vel)) / 180 * Mathf.PI - EFFECTIVE_ANGLE_OFFSET;

        float drag = Mathf.Abs(Mathf.Sin(dang)) * vel.magnitude * DRAG_COEF;

        float stall = 1f;

        if (vel.magnitude < STALL_COEF) { stall = vel.magnitude / STALL_COEF; };

        stabilisation = Mathf.Abs(Mathf.Cos(dang)) * stall * STAB_COEF;

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
