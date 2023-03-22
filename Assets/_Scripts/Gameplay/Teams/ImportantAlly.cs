using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportantAlly : MonoBehaviour
{
    [SerializeField] string destroyMessage = "Ally destroyed";
    [SerializeField] float gameOverDelayTime = 10;

    void Start()
    {
        GetComponent<Health>().OnDeath.AddListener(() => Timers.Delay(gameOverDelayTime, () => { GameHandler.Instance.GameOver(destroyMessage); }));
    }
}
