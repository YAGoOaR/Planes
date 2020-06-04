using System.Collections.Generic;
using UnityEngine;

//A script for destructable objects
public class Destructable : MonoBehaviour
{
    const float DESTROYED_OBJECT_VOLUME = 10;
    const float MIN_RANDOM_COEFFICIENT = 0.5f;
    const int maxHP = 3;
    int HP = maxHP;
    float buoyancy = 500;

    //if bomb hits this object
    public void hit()
    {
        string[] edges = { "edge1", "edge2" };
        if (HP < 1)
        {
            Buoyancy buoyancyComponent = transform.Find(edges[Random.Range(0, edges.Length - 1)]).GetComponent<Buoyancy>();
            buoyancyComponent.volume = DESTROYED_OBJECT_VOLUME;
            if (gameObject.name == "AirCarrier")
            {
                Timers.TimeoutTimer timer = new Timers.TimeoutTimer(0, () => { Game.instance.gameOver("air carrier is destroyed"); });
            }
            return;
        }
        HP--;
        foreach (string edge in edges)
        {
            transform.Find(edge).GetComponent<Buoyancy>().volume -= Mathf.Max(MIN_RANDOM_COEFFICIENT, Random.value) * buoyancy / maxHP;
        }
    }
}
