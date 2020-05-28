using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Info : MonoBehaviour
{

    private Text infoText;

    void Awake()
    {
        infoText = transform.Find("InfoText").GetComponent<Text>();
    }

    string boolToSwitch(bool value) {
        if (value) return "on";
        else return "off";
    }

    void LateUpdate()
    {
        infoText.text = "Throttle: " + GameHandler.i.planeInfo.throttle.ToString() + "\n" + 
        "Speed: " +   Mathf.Floor(GameHandler.i.planeInfo.speed * 6).ToString() + "\n" + 
        "Ammo: " + GameHandler.i.planeInfo.bullets + "\n" +
        "Bombs: " + GameHandler.i.planeInfo.bombs + "\n" +
        "Gear: " + boolToSwitch(GameHandler.i.planeInfo.gear) + "\n" 
        ;
    }
}
