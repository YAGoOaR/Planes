using UnityEngine;

//A script that detects objects that collide water
public class Water : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D hit)
    {
        if (hit.GetComponent<Rigidbody2D>() != null && hit.tag != "Map")
        {
            transform.parent.GetComponent<WaterManager>().Splash(transform.position.x, hit.GetComponent<Rigidbody2D>().velocity.y * hit.GetComponent<Rigidbody2D>().mass);
        }
    }
}
