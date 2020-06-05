using UnityEngine;

//Destroy the attached object after timeout
public class SelfDestroy : MonoBehaviour
{
    float curTime;
    [SerializeField]
    float maxTime;

    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > maxTime)
        {
            Object.Destroy(gameObject);
        }
    }
}
