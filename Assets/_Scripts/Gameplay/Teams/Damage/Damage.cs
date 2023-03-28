using UnityEngine;
using UnityEngine.Events;

public class Damage : MonoBehaviour
{
    [SerializeField] int damageAmount = 1;
    [SerializeField] float velocityThreshold = 5f;
    [SerializeField] bool damageOnCollision = true;
    [SerializeField] bool damageOnTrigger = false;
    [SerializeField] bool useTeamId = true;
    [SerializeField] Teams.Team team = Teams.Team.Enemies;
    public UnityEvent OnDamage = new();

    private void ApplyDmg(Health health)
    {
        if (useTeamId && team == health.team) return;
        health.Damage(damageAmount);
        OnDamage.Invoke();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!damageOnCollision) return;
        Health health;
        if (collision.gameObject.TryGetComponent(out health) && collision.relativeVelocity.magnitude >= velocityThreshold)
        {
            ApplyDmg(health);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!damageOnTrigger) return;
        Health health;
        if (collision.gameObject.TryGetComponent(out health))
        {
            ApplyDmg(health);
        }
    }
}
