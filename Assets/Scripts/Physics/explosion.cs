﻿using UnityEngine;

//Script for a bomb
public class explosion : MonoBehaviour
{
    const float EXPLOSION_FORCE_COEF = 400;
    bool isDropped;
    float timer;
    [SerializeField]
    float minDetonationTime;

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
            detonated();
            if (collision.gameObject.tag == "Destructable")
            {
                collision.gameObject.GetComponent<Destructable>().hit();
            }
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
        detonated();
    }

    //start explosion animation
    void detonated()
    {
        GetComponent<Animator>().SetBool("detonated", true);
        GetComponent<PointEffector2D>().enabled = true;
    }
}
