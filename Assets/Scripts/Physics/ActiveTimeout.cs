using UnityEngine;

//A script that activates attached game object after specified time
public class ActiveTimeout : MonoBehaviour
{
    [SerializeField]
    float timeout;

    void Awake()
    {
        gameObject.SetActive(false);
        Timers.Delay(timeout, () => { gameObject.SetActive(true); });
    }
}
