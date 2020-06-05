using UnityEngine;

//A script that makes the attached object to float
public class Buoyancy : MonoBehaviour
{
    const float FORCE_COEFFICIENT = 8;
    public float volume = 10;
    Collider2D waterCollider;
    int connections;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag != "water")
        {
            return;
        }
        waterCollider = collider;
        connections++;
    }

    void OnTriggerExit2D()
    {
        connections--;
    }

    void FixedUpdate()
    {
        if (connections < 0)
        {
            connections = 0;
        }

        if (connections <= 0 || !waterCollider)
        {
            return;
        }
        Rigidbody2D thisRigidbody = gameObject.GetComponent<Rigidbody2D>();
        GameObject waterObject = waterCollider.gameObject;
        float position = gameObject.transform.position.y;
        float waterLevel = waterObject.transform.position.y + (waterObject.transform.localScale.y) / 2;
        float forceY = FORCE_COEFFICIENT * volume * (waterLevel - position);
        thisRigidbody.AddForce(new Vector2(0, forceY));
    }
}
