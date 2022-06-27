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
    Dictionary<Team, List<Health>> teams = new Dictionary<Team, List<Health>>();

    void Awake()
    {
        instance = this;

        foreach (Team team in Enum.GetValues(typeof(Team)))
        {
            teams.Add(team, new List<Health>());
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

    public int GetTeamMemberCount(Team team)
    {
        return teams[team].Count;
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
}
