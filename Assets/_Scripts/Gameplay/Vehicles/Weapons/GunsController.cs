using UnityEngine;

public class GunsController : MonoBehaviour
{
    public float MaxShootDistance { get => maxShootDistance; }
    float maxShootDistance = 0;
    public float MaxVelocity { get => maxVelocity; }
    public bool Full { get => full; set => full = value; }

    public float GunRange { get => guns[0].Range; }
    public float GunBulletVelocity { get => guns[0].BulletSpeed; }
    public bool HasAmmo { get => guns[0].Bullets > 0; }

    Gun[] guns;
    float maxVelocity = 0;
    bool full = true;

    void Awake()
    {
        guns = GetComponentsInChildren<Gun>();
        foreach (Gun gun in guns)
        {
            if (gun.Range > maxShootDistance) maxShootDistance = gun.Range;
            if (gun.BulletSpeed > maxVelocity) maxVelocity = gun.BulletSpeed;
        }
    }

    public void TryShoot(float distance)
    {
        foreach (Gun gun in guns)
        {
            if (distance < gun.Range) {
                gun.Shoot();
            } 
        }
    }

    public void Shoot()
    {
        foreach (Gun gun in guns)
        {
            bool success = gun.Shoot();
            if (full) full = !success;
        }
    }

    public bool CanShoot(float distance)
    {
        return distance < maxShootDistance;
    }

    public void Reload()
    {
        foreach (Gun gun in guns)
        {
            gun.Reload();
            full = true;
        }
    }
}
