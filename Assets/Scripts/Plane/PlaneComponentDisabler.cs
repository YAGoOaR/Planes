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

    public void SetPartsActive(bool active)
    {
        foreach (PlanePart part in partManager.Parts)
        {
            if (part.PartName != "propeller")
            {
                part.Hide(!active);
            }
        }
        bombBay.SetBombsActive(active);
    }
}
