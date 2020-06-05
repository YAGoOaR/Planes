using UnityEngine;
using UnityEngine.UI;

//Script that shows "Game over: ${reason}" on UI when the game is lost
public class GameOver : Game
{
    bool gameIsOver = false;

    Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    override public void gameOver(string reason)
    {
        Debug.Log("over");
        if (gameIsOver) return;
        base.gameOver(reason);
        gameIsOver = true;
        text.enabled = true;
        if (reason != "") text.text += ":\n" + reason;
    }
}
