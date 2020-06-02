using UnityEngine;

//Destroy the attached object after timeout
public class SelfDestroy : MonoBehaviour
{
    float curTime = 0;
    public float maxTime;

    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > maxTime) Object.Destroy(gameObject);
    }
}
