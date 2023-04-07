using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    const float effectTime = 5;

    [SerializeField] GameObject effect;

    public void Explode()
    {
        GameObject explosion = Instantiate(effect != null ? effect : GameAssets.Instance.ExplosionEffect, transform.position, transform.rotation);
        explosion.AddComponent<SelfDestroy>().MaxTime = effectTime;
    }
}
