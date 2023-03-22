using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanePartManager : MonoBehaviour
{
    public enum PartType
    {
        aerofoil,
        gear,
        engine,
        elevator,
        flap,
        other,
        smoke,
    }

    public PlanePart[] Parts { get => allParts.ToArray(); }

    readonly List<PlanePart> allParts = new List<PlanePart>();
    readonly Dictionary<PartType, List<PlanePart>> parts = new Dictionary<PartType, List<PlanePart>>();

    void AddElem(PlanePart part)
    {
        List<PlanePart> list = parts[part.partType];
        list.Add(part);
        allParts.Add(part);
        part.OnBreak.AddListener(() => { list.Remove(part); allParts.Remove(part); });
    }

    void Awake()
    {
        foreach (PartType key in Enum.GetValues(typeof(PartType)))
        {
            parts.Add(key, new List<PlanePart>());
        }

        foreach(PlanePart part in transform.parent.GetComponentsInChildren<PlanePart>(true))
        {
            AddElem(part);
        }
    }

    public PlanePart GetPart(PartType partType)
    {
        return parts[partType][0];
    }

    public PlanePart[] GetParts(PartType partType)
    {
        return parts[partType].ToArray();
    }

    public List<PlanePart> GetAll()
    {
        List<PlanePart> allParts = new List<PlanePart>();
        foreach (List<PlanePart> val in parts.Values)
        {
            allParts.AddRange(val);
        }

        return allParts;
    }
}
