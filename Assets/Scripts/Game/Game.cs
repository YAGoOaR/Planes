using UnityEngine;
using UnityEngine.SceneManagement;

//Script that handles main functions in game
public class Game : MonoBehaviour
{
    const float WAIT_BEFORE_EXIT = 10;
    bool gameIsOver;
    bool gameIsWon;
    public void quitGame()
    {
        SceneManager.LoadScene(0);
    }

    public void restartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void gameOver(string reason)
    {
        if (gameIsOver || gameIsWon) return;
        Debug.Log(reason);
        gameIsOver = true;
        Timers.delay(WAIT_BEFORE_EXIT, quitGame);
    }

    public void gameWin(string reason)
    {
        if (gameIsWon || gameIsOver) return;
        Debug.Log(reason);
        gameIsWon = true;
        Timers.delay(WAIT_BEFORE_EXIT, quitGame);
    }
}
