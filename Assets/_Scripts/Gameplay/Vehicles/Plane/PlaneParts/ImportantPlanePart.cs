using UnityEngine;

public class ImportantPlanePart : MonoBehaviour
{
    void Start()
    {
        Health health = GetComponentInParent<Health>();
        GetComponent<PlanePart>().OnBreak.AddListener(() => health.Kill());
    }
}
