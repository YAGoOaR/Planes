using UnityEngine;
using UnityEngine.SceneManagement;

public class Button1 : MonoBehaviour
{
    public GameObject controlsPrefab;

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowControls()
    {
        GameObject.Instantiate(controlsPrefab, transform.parent);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
