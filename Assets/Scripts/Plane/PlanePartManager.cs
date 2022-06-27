using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanePartManager : MonoBehaviour
{
    public PlanePart[] Parts
    {
        get { return parts; }
    }
    PlanePart[] parts;

    void Awake()
    {
        parts = transform.parent.GetComponentsInChildren<PlanePart>(true);
    }

    public PlanePart getPart(string name)
    {
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].PartName == name)
            {
                return parts[i];
            }
        }
        return null;
    }
}
