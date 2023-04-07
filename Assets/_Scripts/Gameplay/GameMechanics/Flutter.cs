using UnityEngine;
using Random = UnityEngine.Random;

public class Flutter : MonoBehaviour
{
    [SerializeField] float maxOffset = 0.6f;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] FixedJoint2D wingJoint;

    [SerializeField] float flutterStartSpeed = 450;
    [SerializeField] float flutterBreakSpeed = 500;
    const float toWorldMetricCoef = 1f / 6f;

    Vector3 startAnchor;

    Vector3 RandVec () => new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);

    private void Start()
    {
        startAnchor = wingJoint.anchor;
    }

    void FixedUpdate()
    {
        if (wingJoint == null) return;
        float speed = rb.velocity.magnitude / toWorldMetricCoef;

        if (speed >= flutterStartSpeed)
        {
            float s = flutterStartSpeed;
            float b = flutterBreakSpeed;

            float flutterCoef = (Mathf.Min(speed, b) - s) / (b - s);
            wingJoint.anchor = startAnchor + Vector3.Project(RandVec(), rb.transform.up) * maxOffset * flutterCoef;
        } else
        {
            wingJoint.anchor = startAnchor;
        }

        if (speed > flutterBreakSpeed)
        {
            wingJoint.breakForce = 0;
            this.enabled = false;
        }
    }
}
