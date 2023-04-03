using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public bool GearUp { get => plane.GearUp; }
    public bool HasBombs { get => bombBay.BombCount > 0; }

    public Transform BombBay { get => bombBay.transform; }
    public GunsController Guns { get => guns; set => guns = value; }

    AeroPlane plane;
    Rigidbody2D rb;

    BombBay bombBay;
    GunsController guns;

    float P = 10;
    float D = 0.05f;

    void Start()
    {
        plane = GetComponent<AeroPlane>();
        rb = GetComponent<Rigidbody2D>();
        bombBay = GetComponentInChildren<BombBay>();
        guns = GetComponent<GunsController>();
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

    public void Shoot() => guns.Shoot();

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
