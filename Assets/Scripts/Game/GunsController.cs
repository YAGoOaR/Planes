using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunsController : MonoBehaviour
{
    Gun[] guns;
    public float MaxShootDistance { get => maxShootDistance; }
    float maxShootDistance = 0;
    public float MaxVelocity { get => maxVelocity; }
    float maxVelocity = 0;

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
        //Debug.Log($"{distance} {maxShootDistance}");
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
            gun.Shoot();
        }
    }

    public bool CanShoot(float distance)
    {
        //Debug.Log($"{ distance} { maxShootDistance} {distance < maxShootDistance}");
        return distance < maxShootDistance;
    }
}
