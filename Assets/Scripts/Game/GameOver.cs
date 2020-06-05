using UnityEngine;
using UnityEngine.UI;

//Script that shows "Game over: ${reason}" on UI when the game is lost
public class GameOver : Game
{
    bool end = false;

    override public void gameOver(string reason)
    {
        if (end) {
            return;
        }
        end = true;
        base.gameOver(reason);
        Text text = GetComponent<Text>();
        text.enabled = true;
        if (reason != "") text.text += ":\n" + reason;
    }
}
