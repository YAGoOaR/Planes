using UnityEngine;

public class SinkableShip : MonoBehaviour
{
    const float DESTROYED_OBJECT_VOLUME = 10;
    const float HALF_DESTROYED_OBJECT_VOLUME = 100;
    const float buoyancy = 500;

    Health health;
    Buoyancy[] edges;

    private void Start()
    {
        health = GetComponent<Health>();
        edges = GetComponentsInChildren<Buoyancy>();
        health.OnDamage.AddListener(Hit);
    }

    //if bomb hits this object
    public void Hit()
    {

        if (health.HP < health.MaxHP / 2)
        {
            Buoyancy buoyancyComponent = edges[Random.Range(0, edges.Length - 1)];
            buoyancyComponent.Volume = HALF_DESTROYED_OBJECT_VOLUME;
        }
        if (health.HP < health.MaxHP / 4)
        {
            foreach (Buoyancy edge in edges)
            {
                edge.Volume = DESTROYED_OBJECT_VOLUME;
            }
        }
        foreach (Buoyancy edge in edges)
        {
            if (edge.Volume == HALF_DESTROYED_OBJECT_VOLUME) continue;
            edge.Volume = buoyancy * health.HP / health.MaxHP;
        }
    }
}
