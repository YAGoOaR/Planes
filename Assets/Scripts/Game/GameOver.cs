﻿using UnityEngine;
using UnityEngine.UI;

public class GameOver : Game
{
    bool gameIsOver = false;

    Text text;

    void Start()
    {
        text = GetComponent<Text>();
    }

    override public void gameOver(string reason)
    {
        if (gameIsOver) return;
        gameIsOver = true;
        text.enabled = true;
        if (reason != "") text.text += ":\n" + reason;
        base.gameOver(reason);
    }
}
