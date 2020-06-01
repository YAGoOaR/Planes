using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    const int maxHP = 3;
    int HP = maxHP;
    float buoyancy = 500;

    public void hit () {
        if (HP < 1) {
            transform.Find("edge2").GetComponent<Buoyancy>().V = 10;
            return;
        }
        HP--;
        transform.Find("edge2").GetComponent<Buoyancy>().V -= Mathf.Max(0.5f, Random.value) * buoyancy / 3;
        transform.Find("edge1").GetComponent<Buoyancy>().V -= Mathf.Max(0.5f, Random.value) * buoyancy / 3;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
