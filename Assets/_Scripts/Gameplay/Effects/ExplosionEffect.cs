using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    const float effectTime = 5;

    public void Explode()
    {
        GameObject explosion = Instantiate(GameAssets.Instance.ExplosionEffect, transform.position, transform.rotation);
        explosion.AddComponent<SelfDestroy>().MaxTime = effectTime;
    }
}
