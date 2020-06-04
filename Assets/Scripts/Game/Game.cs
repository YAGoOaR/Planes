using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    const float WAIT_BEFORE_EXIT = 5;
    public static Game instance;

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
        Timers.customFunc callback = () =>
        {
            quitGame();
        };
        Timers.TimeoutTimer exitTimer = new Timers.TimeoutTimer(WAIT_BEFORE_EXIT, callback);
    }

    void Awake()
    {
        instance = this;
    }
}
