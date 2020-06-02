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

    string boolToSwitch(bool value)
    {
        if (value) return "on";
        else return "off";
    }

    void LateUpdate()
    {
        infoText.text = "Throttle: " + GameHandler.instance.planeInfo.throttle.ToString() + "\n" +
        "Speed: " + Mathf.Floor(GameHandler.instance.planeInfo.speed * SPEED_MULTIPLIER).ToString() + "\n" +
        "Ammo: " + GameHandler.instance.planeInfo.bullets + "\n" +
        "Bombs: " + GameHandler.instance.planeInfo.bombs + "\n" +
        "Gear: " + boolToSwitch(!GameHandler.instance.planeInfo.gear) + "\n";
    }
}
