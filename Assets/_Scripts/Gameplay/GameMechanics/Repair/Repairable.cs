using UnityEngine;

public class Repairable : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] GunsController guns;
    [SerializeField] Vehicle vehicle;
    [SerializeField] float maxVelocity = 5f;

    bool repairing = false;
    bool NeedRepair { get => !repairing && (!guns.Full || !health.FullHP); }
    bool CanRepair { get => vehicle.Rb.velocity.magnitude < maxVelocity; }

    Timers.Timeout timer;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (NeedRepair && collision.gameObject.TryGetComponent(out RepairZone zone) && CanRepair)
        {
            repairing = true;
            Debug.Log("Repairing!");
            timer = Timers.Timeout.SetTimeout(zone.RepairTime, () => {
                guns.Reload();
                health.Heal();
                repairing = false;
                zone.Respawn(vehicle);
                Debug.Log("Repaired!");
            });
        };
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (timer != null && !timer.Check() && collision.gameObject.TryGetComponent(out RepairZone zone))
        {
            Debug.Log("Repair abort!");
            timer.Abort();
            timer = null;
            repairing = false;
        };
    }

    private void FixedUpdate()
    {
        if (timer != null && !timer.Check())
        {
            if (!CanRepair)
            {
                Debug.Log("Repair abort!");
                timer.Abort();
                timer = null;
                repairing = false;
            }
        };
    }
}
