using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public float GunRange { get => guns[0].Range; }
    public bool GearUp { get => plane.GearUp; }
    public bool HasBombs { get => bombBay.BombCount > 0; }

    public bool HasAmmo { get => guns[0].Bullets > 0; }

    public Transform BombBay { get => bombBay.transform; }

    PlaneBehaviour plane;
    Rigidbody2D rb;

    Vector3 movementTargetDir;

    BombBay bombBay;
    Health health;
    Gun[] guns;

    float P = 3;
    float D = 0f;

    void Start()
    {
        plane = GetComponent<PlaneBehaviour>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        bombBay = GetComponentInChildren<BombBay>();
        guns = GetComponentsInChildren<Gun>();
    }

    void FixedUpdate()
    {
        maintainHeading();
    }

    Vector2 prevRotation;
    void maintainHeading()
    {
        Vector3 flightDir = -transform.right;
        float angleDiff = Vector2.SignedAngle(flightDir, movementTargetDir) * Mathf.Deg2Rad;

        float rotationRate = rb.angularVelocity * Mathf.Deg2Rad;
        float pitch = angleDiff * P + rotationRate * D;

        SetPitch(-pitch);
        prevRotation = flightDir;
    }

    public void Shoot()
    {
        foreach (Gun gun in guns)
        {
            gun.Shoot();
        }
    }

    public void SetFlaps(bool on)
    {
        plane.SetFlaps(on);
    }
    public void SwitchFlaps()
    {
        plane.SetFlaps(!plane.Flaps);
    }

    public void SwitchBrakes()
    {
        plane.SetBrakes(!plane.Brakes);
    }

    public void IncreaseThrottle()
    {
        plane.AdjustThrottle(1);
    }

    public void DecreaseThrottle()
    {
        plane.AdjustThrottle(-1);
    }

    public void SetTarget(Vector3 target)
    {
        movementTargetDir = target - transform.position;
    }
    public void SetHeading(Vector3 hdg)
    {
        movementTargetDir = hdg;
    }

    public void Roll()
    {
        plane.Turn();
    }

    public void TurnBack()
    {
        plane.TurnBack();
    }

    public void SwitchGear()
    {
        plane.SwitchGear();
    }

    public void ThrowBomb()
    {
        bombBay.ThrowBomb();
    }

    public void SetPitch(float pitch)
    {
        plane.SetPitch(pitch);
    }
    public void SetThrottle(int throttle)
    {
        plane.SetThrottle(throttle);
    }
    public void SetGear(bool on)
    {
        plane.SetGear(on);
    }
    public void SetBrakes(bool on)
    {
        plane.SetBrakes(on);
    }
}
