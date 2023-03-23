using UnityEngine;
using UnityEngine.Events;

//Destroy the attached object after timeout
public class SelfDestroy : MonoBehaviour
{
    float curTime;
    [SerializeField] float maxTime;
    public float MaxTime { get => maxTime; set => maxTime = value; }

    public UnityEvent OnDeath = new();

    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > maxTime)
        {
            OnDeath.Invoke();
            Destroy(gameObject);
        }
    }
}
