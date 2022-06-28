using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] float spread = 5f;
    [SerializeField] float coolDown = 0.08f;
    [SerializeField] bool useAmmo = true;
    [SerializeField] int bullets = 250;
    [SerializeField] float bulletSpeed = 100;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float range = 30;

    public int Bullets { get => bullets; }
    public float Range { get => range; }
    public float BulletSpeed { get => bulletSpeed; }

    Timers.CooldownTimer shootingTimer;
    Transform projectileHolder;
    Rigidbody2D rb;

    void Start()
    {
        shootingTimer = new Timers.CooldownTimer(coolDown);
        projectileHolder = GameHandler.Instance.projectileHolder;
        rb = GetComponentInParent<Rigidbody2D>();
    }

    public void Shoot()
    {
        if (!shootingTimer.Check() || bullets <= 0) return;
        shootingTimer.Reset();
        float spread = (Random.value - 0.5f) * 2 * this.spread;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation, projectileHolder);
        bullet.GetComponent<Rigidbody2D>().velocity = rb.velocity - (Vector2)(Quaternion.Euler(0, 0, spread) * transform.right * bulletSpeed);
        if (useAmmo) bullets--;
    }
}
