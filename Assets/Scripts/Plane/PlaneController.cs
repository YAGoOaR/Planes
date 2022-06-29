using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public float GunRange { get => guns[0].Range; }
    public float GunBulletVelocity { get => guns[0].BulletSpeed; }

    public bool GearUp { get => plane.GearUp; }
    public bool HasBombs { get => bombBay.BombCount > 0; }

    public bool HasAmmo { get => guns[0].Bullets > 0; }

    public Transform BombBay { get => bombBay.transform; }

    PlaneBehaviour plane;
    Rigidbody2D rb;

    BombBay bombBay;
    Health health;
    Gun[] guns;

    float P = 10;
    float D = 0.05f;

    void Start()
    {
        plane = GetComponent<PlaneBehaviour>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        bombBay = GetComponentInChildren<BombBay>();
        guns = GetComponentsInChildren<Gun>();
    }

    Vector2 lastHdg;
    public void SetHeading(Vector3 hdg, bool aimVelocity = true)
    {
        Vector2 flightDir = aimVelocity ? rb.velocity.normalized : -(Vector2)transform.right;
        float angleDiff = Vector2.SignedAngle(flightDir, hdg) * Mathf.Deg2Rad;
        float hdgRate = Vector2.SignedAngle(lastHdg, hdg) * Mathf.Deg2Rad;

        float rotationRate = rb.angularVelocity - hdgRate;
        float pitch = angleDiff * P - rotationRate * D;
        SetPitch(-pitch);
        lastHdg = hdg;
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
        SetHeading(target - transform.position);
    }

    //static Func<Vector3, Vector3, Vector3> Reject = (a, b) => a - Vector3.Project(a, b);
    //static Func<Vector3, Vector3, float, Vector3> ReflectWithCoef = (a, b, k) => Vector3.Project(a, b) - k * Reject(a, b);
    //float coef = 0.2f;
    //public void SetHeadingIncreasedSens(Vector3 hdg)
    //{
    //    Vector3 res = ReflectWithCoef(rb.velocity, hdg, coef);
    //    movementTargetDir = Vector2.Angle(hdg, res) > 60 ? hdg : res;
    //}

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
