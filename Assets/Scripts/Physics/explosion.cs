using UnityEngine;

//Script for a bomb
public class Explosion : MonoBehaviour
{
    const float EXPLOSION_FORCE_COEF = 400;
    bool isDropped;
    float timer;
    [SerializeField]
    float minDetonationTime;
    [SerializeField]
    GameObject explosionEffect;

    //Called when the bomb drops from plane
    public void OnJointBreak2D()
    {
        isDropped = true;
    }

    //Called each frame
    void Update()
    {
        if (isDropped)
        {
            timer += Time.deltaTime;
        }
    }

    //Called when bomb hits something
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (timer > minDetonationTime)
        {
            Detonated();
        }
    }

    //called when bomb hits a trigger(water, etc)
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag != "water" || timer < minDetonationTime)
        {
            return;
        }
        GameObject water = collider.transform.parent.gameObject;
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        Vector2 position = transform.position;
        float mass = rigidbody.mass;
        water.GetComponent<Water>().Splash(position.x, mass * EXPLOSION_FORCE_COEF);
        Detonated();
    }

    void Detonated()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation, GameHandler.Instance.projectileHolder);
        Destroy(gameObject);
    }
}
