using UnityEngine;
using UnityEngine.UI;

public class GameOver : Game
{
    bool gameIsOver = false;
    override public void gameOver(string reason)
    {
        base.gameOver(reason);
        if (gameIsOver) return;
        gameIsOver = true;
        Text text = GetComponent<Text>();
        text.enabled = true;
        if (reason != "") text.text += ":\n" + reason;
    }
}
