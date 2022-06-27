using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] float spread = 5f;
    [SerializeField] float coolDown = 0.08f;
    [SerializeField] int bullets;
    [SerializeField] float bulletShootSpeed = 100;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float range = 30;
    public int Bullets { get => bullets; }
    public float Range { get => range; }

    Timers.CooldownTimer shootingTimer;
    Transform projectileHolder;
    Rigidbody2D rb;

    void Start()
    {
        shootingTimer = new Timers.CooldownTimer(coolDown);
        projectileHolder = GameHandler.Instance.projectileHolder;
        rb = transform.parent.GetComponent<Rigidbody2D>();
    }

    public void shoot()
    {
        if (!shootingTimer.check() || bullets <= 0) return;
        shootingTimer.reset();

        float spread = (Random.value - 0.5f) * 2 * this.spread;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation, projectileHolder);
        bullet.GetComponent<Rigidbody2D>().velocity = rb.velocity - (Vector2)(Quaternion.Euler(0, 0, spread) * transform.right * bulletShootSpeed);
        bullets--;
    }
}
