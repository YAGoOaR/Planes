using UnityEngine;

//A script that activates attached game object after specified time
public class ActiveTimeout : MonoBehaviour
{
    public float timeout;

    void Awake()
    {
        gameObject.SetActive(false);
        Timers.timeout(timeout, () => { gameObject.SetActive(true); });
    }
}
