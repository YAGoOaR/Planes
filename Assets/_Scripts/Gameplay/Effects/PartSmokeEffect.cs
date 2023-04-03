using UnityEngine;

public class PartSmokeEffect : MonoBehaviour
{
    [SerializeField] GameObject smallSmoke;
    [SerializeField] Health health;
    [SerializeField] PlanePartManager partManager;

    void Start()
    {
        health.OnDeath.AddListener(() =>
        {
            foreach (PlanePart part in partManager.Parts)
            {
                Instantiate(smallSmoke, part.transform);
            }
        });
       
    }
}
