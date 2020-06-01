using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    const int maxHP = 3;
    int HP = maxHP;
    float buoyancy = 500;

    public void hit()
    {
        Debug.Log("hit");
        string[] edges = { "edge1", "edge2" };
        if (HP < 1)
        {
            Buoyancy buoyancyComponent = transform.Find(edges[Random.Range(0, edges.Length - 1)]).GetComponent<Buoyancy>();
            buoyancyComponent.volume = 10;
            return;
        }
        HP--;
        foreach (string edge in edges)
        {
            transform.Find(edge).GetComponent<Buoyancy>().volume -= Mathf.Max(0.5f, Random.value) * buoyancy / 3;
        }
    }
}
