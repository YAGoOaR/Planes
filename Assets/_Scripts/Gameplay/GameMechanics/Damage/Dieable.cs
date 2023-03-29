using UnityEngine;
using UnityEngine.Events;

public class Dieable : MonoBehaviour
{
    public UnityEvent OnDeath = new();
    public void Die(float time = 5)
    {
        SelfDestroy destroyer = gameObject.AddComponent<SelfDestroy>();
        destroyer.MaxTime = time;
        destroyer.OnDeath.AddListener(() => OnDeath.Invoke());
    }
}
