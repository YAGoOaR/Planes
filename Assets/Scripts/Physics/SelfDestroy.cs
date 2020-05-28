using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    private float curTime = 0;
    public float maxTime;

    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > maxTime) Object.Destroy(gameObject);
    }
}
