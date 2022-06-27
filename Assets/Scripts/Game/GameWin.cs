using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWin : MonoBehaviour
{
    void Start()
    {
        Teams teams = Teams.Instance;
        teams.OnRemove.AddListener(() =>
        {
            if (teams.GetTeamMemberCount(Teams.Team.Enemies) == 0)
            {
                GameHandler.Instance.GameWin("Mission success. Enemies have been defeated");
            }
        });

    }
}