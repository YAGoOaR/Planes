using UnityEngine;
using UnityEngine.SceneManagement;

//Script that handles main functions in game
public class Game : MonoBehaviour
{
    const float WAIT_BEFORE_EXIT = 10;
    bool gameIsOver;
    bool gameIsWon;
    public void QuitGame()
    {
        SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void GameOver(string reason)
    {
        if (gameIsOver || gameIsWon) return;
        Debug.Log(reason);
        gameIsOver = true;
        Timers.Delay(WAIT_BEFORE_EXIT, QuitGame);
    }

    public void GameWin(string reason)
    {
        if (gameIsWon || gameIsOver) return;
        gameIsWon = true;
        Timers.Delay(WAIT_BEFORE_EXIT, QuitGame);
    }
}
