using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] int damageAmount = 1;
    [SerializeField] float velocityThreshold = 5f;
    [SerializeField] bool damageOnCollision = true;
    [SerializeField] bool damageOnTrigger = false;
    [SerializeField] bool useTeamId = true;
    [SerializeField] Teams.Team team = Teams.Team.Enemies;

    void applyDmg(Health health)
    {
        if (useTeamId) health.Damage(damageAmount, team); else health.Damage(damageAmount);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!damageOnCollision) return;
        Health health;
        if (collision.gameObject.TryGetComponent(out health) && collision.relativeVelocity.magnitude >= velocityThreshold)
        {
            applyDmg(health);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!damageOnTrigger) return;
        Health health;
        if (collision.gameObject.TryGetComponent(out health))
        {
            applyDmg(health);
        }
    }
}
