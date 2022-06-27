using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneComponentDisabler : MonoBehaviour
{
    BombBay bombBay;
    PlanePartManager partManager;

    // Start is called before the first frame update
    void Start()
    {
        partManager = GetComponent<PlanePartManager>();
        bombBay = GetComponentInChildren<BombBay>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setPartsActive(bool active)
    {
        foreach (PlanePart part in partManager.Parts)
        {
            if (part.PartName != "propeller")
            {
                part.hide(!active);
            }
        }
        bombBay.setBombsActive(active);
    }
}
