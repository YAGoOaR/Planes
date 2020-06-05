using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveTimeout : MonoBehaviour
{
    public float timeout;

    void Start()
    {
        gameObject.SetActive(false);
        Timers.timeout(timeout, () => { gameObject.SetActive(true); });
    }
}
