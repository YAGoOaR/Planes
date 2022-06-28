using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Teams : MonoBehaviour
{
    static Teams instance;
    public static Teams Instance { get => instance; }

    public UnityEvent OnRemove = new UnityEvent();

    public enum Team
    {
        Allies,
        Enemies,
    }

    readonly Dictionary<Team, List<Health>> teams = new Dictionary<Team, List<Health>>();
    readonly Dictionary<Team, List<EnemySpawner>> spawners = new Dictionary<Team, List<EnemySpawner>>();

    void Awake()
    {
        instance = this;

        foreach (Team team in Enum.GetValues(typeof(Team)))
        {
            teams.Add(team, new List<Health>());
            spawners.Add(team, new List<EnemySpawner>());
        }

        Health[] entities = FindObjectsOfType<Health>();     
    }

    public void AddObject(Health health)
    {
        teams[health.team].Add(health);
    }
    public void RemoveObject(Health health)
    {
        teams[health.team].Remove(health);
        OnRemove.Invoke();
    }

    int GetTeamSpawnableCount(Team team)
    {
        int count = 0;

        foreach (EnemySpawner spawner in spawners[team])
        {
            count += spawner.EnemiesToSpawn;
        }
        return count;
    }

    public int GetTeamMemberCount(Team team, bool includeSpawners)
    {
        return teams[team].Count + (includeSpawners ? GetTeamSpawnableCount(team) : 0);
    }

    public Health FindClosestToMe(Team team, Vector3 me)
    {
        Health closest = null;
        float closestDist = Mathf.Infinity;

        foreach(Health health in teams[team])
        {
            float dist = (health.transform.position - me).magnitude;
            if (dist < closestDist)
            {
                closest = health;
                closestDist = dist;
            }
        }

        return closest;
    }

    public Health FindBiggest(Team team)
    {
        Health biggest = null;
        float biggestHP = 0;

        foreach (Health health in teams[team])
        {
            float hp = health.MaxHP;
            if (hp > biggestHP)
            {
                biggest = health;
                biggestHP = hp;
            }
        }

        return biggest;
    }

    public void AddSpawner(Team team, EnemySpawner spawner)
    {
        spawners[team].Add(spawner);
    }
}
