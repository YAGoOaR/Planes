using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dieable : MonoBehaviour
{
    public Health health;
    public float destroyDelay = 5;

    void Start()
    {
        health.OnDeath.AddListener(() => Timers.Delay(destroyDelay, () => Destroy(gameObject)));
    }

}
