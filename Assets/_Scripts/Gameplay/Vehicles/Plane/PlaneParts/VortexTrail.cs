using UnityEngine;

public class VortexTrail : PlanePart
{
    TrailRenderer trailRenderer;

    public override void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public override void Hide(bool hide)
    {
        trailRenderer.enabled = !hide;
    }
}
