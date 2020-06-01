using UnityEngine;

public class explosion : MonoBehaviour
{
    private bool isDropped = false;
    private float timer = 0;
    public float maxTime = 0f;

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
        if (timer > maxTime)
        {
            GetComponent<Animator>().SetBool("detonated", true);
            GetComponent<PointEffector2D>().enabled = true;
            if (collision.gameObject.tag == "Destructable" && collision.gameObject.GetComponent<Destructable>())
            {
                collision.gameObject.GetComponent<Destructable>().hit();
            }
        }
    }
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag != "water" || timer < maxTime) return;
        GameObject water = collider.transform.parent.gameObject;
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        Vector2 pos = transform.position;
        float velocity = rigidbody.velocity.magnitude;
        float mass = rigidbody.mass;
        water.GetComponent<Water>().Splash(pos.x, mass * 500);
        OnCollisionEnter2D(null);
    }
}
