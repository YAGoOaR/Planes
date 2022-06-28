using UnityEngine;

public class Smoker : MonoBehaviour
{
    Health health;
    [SerializeField] GameObject smoke;

    [SerializeField] ParticleSystemRenderer particleSystemRenderer;
    [SerializeField] Material lightSmoke;
    [SerializeField] Material hardSmoke;

    [SerializeField] float lightThreshold = 0.5f;
    [SerializeField] float hardThreshold = 0.25f;

    void Start()
    {
        health = GetComponent<Health>();
        health.OnDamage.AddListener(OnDamage);
    }

    void OnDamage()
    {
        float live = (float)health.HP / health.MaxHP;
        particleSystemRenderer.material = live <= hardThreshold ? hardSmoke : lightSmoke;
        smoke.SetActive(live <= lightThreshold);
    }
}
