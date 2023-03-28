using UnityEngine;
using UnityEngine.UI;

public class PlaneInfoDisplayer : MonoBehaviour
{
    InfoText info;
    private Text text;

    const float SPEED_MULTIPLIER = 6;

    public struct InfoText
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
        "Gear: " + BoolToSwitch(!info.gear) + "\n" +
        "Brakes: " + BoolToSwitch(!info.brakes) + "\n" +
        "Flaps: " + BoolToSwitch(info.flaps) + "\n";
    }

    static string BoolToSwitch(bool value)
    {
        return value ? "on" : "off";
    }

    public void SetInfo(InfoText info)
    {
        this.info = info;
    }
}
