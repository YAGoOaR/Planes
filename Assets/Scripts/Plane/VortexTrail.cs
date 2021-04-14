using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VortexTrail : PlanePart
{
    TrailRenderer trailRenderer;

    public override void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public override void hide(bool hide)
    {
        trailRenderer.enabled = !hide;
    }
}
