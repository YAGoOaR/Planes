using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneInfoShow : MonoBehaviour
{
    infoText info;
    private Text text;

    //TODO: remove
    const float SPEED_MULTIPLIER = 6;

    public struct infoText
    {
        public int throttle, bullets, bombs;
        public float speed, altitude;
        public bool gear, brakes, flaps;
    }

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {

        text.text = "Throttle: " + info.throttle.ToString() + "\n" +
        "Speed: " + Mathf.Floor(info.speed * SPEED_MULTIPLIER).ToString() + "\n" +
        "ALT: " + Mathf.Floor(info.altitude).ToString() + "\n" +
        "Ammo: " + info.bullets + "\n" +
        "Bombs: " + info.bombs + "\n" +
        "Gear: " + boolToSwitch(!info.gear) + "\n" +
        "Brakes: " + boolToSwitch(!info.brakes) + "\n" +
        "Flaps: " + boolToSwitch(info.flaps) + "\n";
    }

    static string boolToSwitch(bool value)
    {
        return value ? "on" : "off";
    }

    public void SetInfo(infoText info)
    {
        this.info = info;
    }
}
