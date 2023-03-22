using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneInfo : MonoBehaviour
{
    PlaneBehaviour plane;
    PlaneInfoShow infoShow;
    Rigidbody2D rb;
    PropellerMotor propellerMotor;
    BombBay bombBay;
    Gun gun;

    void Start()
    {
        plane = GetComponent<PlaneBehaviour>();
        rb = GetComponent<Rigidbody2D>();
        infoShow = GameHandler.Instance.planeinfoShow;
        propellerMotor = transform.parent.GetComponentInChildren<PropellerMotor>();
        bombBay = GetComponentInChildren<BombBay>();
        gun = GetComponentInChildren<Gun>();
    }

    void Update()
    {
        PlaneInfoShow.InfoText infoText = new PlaneInfoShow.InfoText {
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
