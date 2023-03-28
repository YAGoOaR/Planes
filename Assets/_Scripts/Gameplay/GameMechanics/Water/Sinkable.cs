using UnityEngine;

public class Sinkable : MonoBehaviour
{
    Timers.CooldownTimer timer = new(max:1f);
    [SerializeField] Health health;
    [SerializeField] int damagePerSec = 1;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out Water _)) return;
        if (timer.Check())
        {
            health.Damage(damagePerSec);
            timer.Reset();
        }
    }
}
