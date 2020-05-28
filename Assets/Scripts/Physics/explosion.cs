using System.Collections;
using System.Collections.Generic;
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
            gameObject.GetComponent<Animator>().SetBool("detonated", true);
            gameObject.GetComponent<PointEffector2D>().enabled = true;
        }
    }
}
