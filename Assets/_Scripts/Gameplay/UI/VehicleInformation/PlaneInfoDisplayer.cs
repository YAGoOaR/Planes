using UnityEngine;

public class PlaneInfoDisplayer : MonoBehaviour
{
    const float SPEED_MULTIPLIER = 6;

    [SerializeField] AeroPlane plane;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] BombBay bombBay;
    [SerializeField] Gun gun;
    [SerializeField] PropellerEngine propellerMotor;

    UIManager uIManager;

    void Start()
    {
        uIManager = UIManager.Instance;
    }

    static string BoolToText(bool value) => value ? "on" : "off";

    void Update()
    {
        uIManager.VehicleInfo.text = (
            $"Throttle: {propellerMotor.Throttle}\n" +
            $"Speed: {Mathf.Floor(rb.velocity.magnitude * SPEED_MULTIPLIER)}\n" +
            $"ALT: {plane.transform.position.y:.}\n" +
            $"Ammo: {(gun != null ? gun.Bullets : 0)}\n" +
            $"Bombs: {(bombBay != null ? bombBay.BombCount : 0)}\n" +
            $"Gear: {BoolToText(!plane.GearUp)}\n" +
            $"Brakes: {BoolToText(plane.Brakes)}\n" +
            $"Flaps: {BoolToText(plane.Flaps)}\n"
        );
    }
}
