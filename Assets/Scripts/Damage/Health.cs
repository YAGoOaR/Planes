using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent OnDeath;
    public UnityEvent OnDamage;
    public Teams.Team team = Teams.Team.Enemies;

    public bool Dead { get => dead; }
    bool dead = false;
    [SerializeField]
    int hp = 4;
    [SerializeField]
    int maxhp = 4;

    public int HP { get => hp; }
    public int maxHP { get => maxhp; }

    private void Start()
    {
        Teams.Instance.AddObject(this);
    }

    public void Damage(int damageAmount, Teams.Team team)
    {
        if (this.team == team) return;
        Damage(damageAmount);
    }

    public void Damage(int damageAmount)
    {
        hp -= damageAmount;
        OnDamage.Invoke();
        if (hp <= 0 && !dead) {
            dead = true;
            OnDeath.Invoke();
        }
    }

    public void Kill()
    {
        Damage(HP);
    }

    private void OnDestroy()
    {
        Teams.Instance.RemoveObject(this);
    }
}
