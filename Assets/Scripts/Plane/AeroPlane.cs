using System.Collections.Generic;
using UnityEngine;

public class AeroPlane : MonoBehaviour
{
    public string[] partNames = { "gear", "wing", "flap", "tail", "elevator", "propeller" };
    [HideInInspector]
    public PlanePart[] parts;
    public int HP = 4;
    public int bullets = 150;
    public int bombCount = 1;
    public Queue<GameObject> bombs = new Queue<GameObject>();

    void Awake()
    {
        int count = partNames.Length;
        parts = new PlanePart[count];
        for (int i = 0; i < count; i++)
        {
            parts[i] = transform.Find(partNames[i]).gameObject.AddComponent<PlanePart>();
        }
    }

    public PlanePart getPart(string name)
    {
        for (int i = 0; i < partNames.Length; i++)
        {
            if (partNames[i] == name) return parts[i];
        }
        return null;
    }
}
