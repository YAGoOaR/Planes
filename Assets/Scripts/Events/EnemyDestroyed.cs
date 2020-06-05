using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestroyed : Game
{
    public override void gameOver(string reason)
    {
        base.gameOver(reason);
        GetComponent<AIPlane>().enemyDestroyed = true;
    }
}
