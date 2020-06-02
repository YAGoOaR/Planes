using UnityEngine;

//Plane aerofoil behaviour
public class Aerofoil : MonoBehaviour
{
    const float STALL_COEF = 70f;
    const float DRAG_COEF = 0.0005f;
    const float STAB_COEF = 0.8f;
    const float EFFECTIVE_ANGLE_OFFSET = -0.04f;
    public bool lift = false;
    public float LIFT_FORCE_COEF = 0.1f;
    public int upside = 1;
    Rigidbody2D rigidBody;
    Vector2 rotationVector;
    float rotationAngle;
    float stabilization = 1f;

    //Called once when this object initializes
    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
    }

    //Called once per frame
    //Physics of an aerofoil
    void FixedUpdate()
    {
        rotationAngle = (transform.rotation.eulerAngles.z) / 180 * Mathf.PI;
        rotationVector = new Vector2(-Mathf.Cos(rotationAngle), -Mathf.Sin(rotationAngle));
        Vector2 velocity = rigidBody.velocity;
        float velocityAngle = Vector2.SignedAngle(Vector2.left, velocity) / 180 * Mathf.PI;
        //angle between velocity and heading 
        float deltaAngle = (-Vector2.SignedAngle(rotationVector, velocity)) / 180 * Mathf.PI - EFFECTIVE_ANGLE_OFFSET;
        //drag applied to the aerofoil
        float drag = Mathf.Abs(Mathf.Sin(deltaAngle)) * velocity.magnitude * DRAG_COEF;
        //Stall coefficient
        float stall = 1f;
        //Stalling when low speed
        if (velocity.magnitude < STALL_COEF) stall = velocity.magnitude / STALL_COEF;
        //Stabilization force coefficient
        stabilization = Mathf.Abs(Mathf.Cos(deltaAngle)) * stall * STAB_COEF;
        //Stabilization force
        Vector2 stabForce = new Vector2(-Mathf.Cos(rotationAngle), -Mathf.Sin(rotationAngle)) * velocity.magnitude;
        //Lifting force
        Vector2 liftForce;
        if (lift)
        {
            liftForce = new Vector2(-Mathf.Sin(rotationAngle), Mathf.Cos(rotationAngle)) * Mathf.Sqrt(velocity.magnitude) * LIFT_FORCE_COEF * upside;
        }
        else liftForce = Vector2.zero;
        //Applying velocity to rigidbody
        rigidBody.velocity = (velocity * (1 - stabilization) + stabilization * (stabForce + liftForce)) * (1 - drag);
    }
}
