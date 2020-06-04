using UnityEngine;
using UnityEngine.SceneManagement;

//A script for buttons in menu
public class Button1 : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowControls()
    {
        transform.parent.Find("controls").gameObject.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
