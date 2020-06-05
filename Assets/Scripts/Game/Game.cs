using UnityEngine;
using UnityEngine.SceneManagement;

//Script that handles main funcrions in game
public class Game : MonoBehaviour
{
    const float WAIT_BEFORE_EXIT = 10;
    bool gameIsOver;

    static Game instance;
    public static Game Instance
    {
        get { return Game.instance; }
    }

    public static void quitGame()
    {
        SceneManager.LoadScene(0);
    }

    public static void restartGame()
    {
        SceneManager.LoadScene(1);
    }

    public virtual void gameOver(string reason)
    {
        if (gameIsOver)
        {
            return;
        }
        gameIsOver = true;
        Timers.customFunc callback = () =>
        {
            quitGame();
        };
        Timers.timeout(WAIT_BEFORE_EXIT, callback);
    }

    void Awake()
    {
        instance = this;
    }
}
