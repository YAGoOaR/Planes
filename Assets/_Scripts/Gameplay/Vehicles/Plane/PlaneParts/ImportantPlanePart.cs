using UnityEngine;

public class ImportantPlanePart : MonoBehaviour
{
    void Start()
    {
        Health health = transform.parent.GetComponent<Hull>().hull.GetComponent<Health>();
        GetComponent<PlanePart>().OnBreak.AddListener(() => health.Kill());
    }
}
