using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartSmoker : MonoBehaviour
{
    [SerializeField] GameObject smallSmoke;

    void Start()
    {
        Health health = GetComponent<Health>();
        PlanePartManager partManager = GetComponent<PlanePartManager>();

        health.OnDeath.AddListener(() =>
        {
            foreach (PlanePart part in partManager.Parts)
            {
                Instantiate(smallSmoke, part.transform);
            }
        });
       
    }
}
