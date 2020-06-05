using UnityEngine;
using UnityEngine.UI;

//Show info about plane to UI
public class Info : MonoBehaviour
{
    const float SPEED_MULTIPLIER = 6;
    private Text infoText;

    void Awake()
    {
        infoText = transform.Find("InfoText").GetComponent<Text>();
    }

    static string boolToSwitch(bool value)
    {
        if (value)
        {
            return "on";
        }
        else
        {
            return "off";
        }
    }

    void LateUpdate()
    {
        GameHandler.infoText info = GameHandler.Instance.PlaneInfo;
        infoText.text = "Throttle: " + info.throttle.ToString() + "\n" +
        "Speed: " + Mathf.Floor(info.speed * SPEED_MULTIPLIER).ToString() + "\n" +
        "ALT: " + Mathf.Floor(info.altitude).ToString() + "\n" +
        "Ammo: " + info.bullets + "\n" +
        "Bombs: " + info.bombs + "\n" +
        "Gear: " + boolToSwitch(!info.gear) + "\n" +
        "Brakes: " + boolToSwitch(!info.brakes) + "\n";
    }
}
