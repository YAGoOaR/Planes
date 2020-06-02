using UnityEngine;
using System.Collections;

public class WaterDetector : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D hit)
    {
        if (hit.GetComponent<Rigidbody2D>() != null && hit.tag != "Map")
        {
            transform.parent.GetComponent<Water>().Splash(transform.position.x, hit.GetComponent<Rigidbody2D>().velocity.y * hit.GetComponent<Rigidbody2D>().mass);
        }
    }
}
