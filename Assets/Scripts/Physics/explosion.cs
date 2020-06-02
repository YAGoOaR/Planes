using UnityEngine;
//Script for a bomb
public class explosion : MonoBehaviour
{
    float EXPLOSION_FORCE_COEF = 500;
    private bool isDropped = false;
    private float timer = 0;
    public float minDetonationTime = 0f;

    public void OnJointBreak2D()
    {
        isDropped = true;
    }

    public void Update()
    {
        if (isDropped)
        {
            timer += Time.deltaTime;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (timer > minDetonationTime)
        {
            detonated();
            if (collision.gameObject.tag == "Destructable")
            {
                collision.gameObject.GetComponent<Destructable>().hit();
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag != "water" || timer < minDetonationTime) return;
        GameObject water = collider.transform.parent.gameObject;
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        Vector2 position = transform.position;
        float velocity = rigidbody.velocity.magnitude;
        float mass = rigidbody.mass;
        water.GetComponent<Water>().Splash(position.x, mass * EXPLOSION_FORCE_COEF);
        detonated();
    }

    void detonated()
    {
        GetComponent<Animator>().SetBool("detonated", true);
        GetComponent<PointEffector2D>().enabled = true;
    }
}
