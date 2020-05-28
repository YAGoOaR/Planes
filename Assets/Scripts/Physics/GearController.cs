using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearController : MonoBehaviour
{
    public bool gearUp { get; private set; }
    private Animator gearAnimator;

    void Start()
    {
        gearUp = false;
        gearAnimator = gameObject.GetComponent<Animator>();
    }

    public void switchGear()
    {
        gearAnimator.SetBool("gearUp", !gearUp);
    }
    public void gearSwitched()
    {
        gearUp = !gearUp;
        gameObject.GetComponent<CircleCollider2D>().enabled = gearUp;
    }
}