using UnityEngine;

public class PlaneInfoGatherer : MonoBehaviour
{
    PlaneBehaviour plane;
    PlaneInfoDisplayer infoShow;
    Rigidbody2D rb;
    PropellerMotor propellerMotor;
    BombBay bombBay;
    Gun gun;

    void Start()
    {
        plane = GetComponent<PlaneBehaviour>();
        rb = GetComponent<Rigidbody2D>();
        infoShow = GameManager.Instance.planeinfoShow;
        propellerMotor = transform.parent.GetComponentInChildren<PropellerMotor>();
        bombBay = GetComponentInChildren<BombBay>();
        gun = GetComponentInChildren<Gun>();
    }

    void Update()
    {
        PlaneInfoDisplayer.InfoText infoText = new PlaneInfoDisplayer.InfoText {
            throttle = propellerMotor.Throttle,
            bullets = gun.Bullets,
            altitude = transform.position.y,
            bombs = bombBay.BombCount,
            speed = rb.velocity.magnitude,
            gear = plane.GearUp,
            brakes = !plane.Brakes,
            flaps = plane.Flaps,
        };
        infoShow.SetInfo(infoText);
    }
}
