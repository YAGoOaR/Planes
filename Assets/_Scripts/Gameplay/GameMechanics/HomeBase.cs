using UnityEngine;

public class HomeBase : MonoBehaviour
{
    [SerializeField] string destroyMessage = "Home base have been destroyed";
    [SerializeField] float gameOverDelayTime = 10;

    void Start()
    {
        GetComponent<Health>().OnDeath.AddListener(() => Timers.Delay(gameOverDelayTime, () => { GameManager.Instance.GameOver(destroyMessage); }));
    }
}
