using UnityEngine;
using UnityEngine.SceneManagement;

//A script for buttons in menu
public class Button1 : MonoBehaviour
{
    //Start button
    public static void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    //Controls Button
    public void ShowControls()
    {
        transform.parent.Find("controls").gameObject.SetActive(true);
    }

    //Exit button
    public static void ExitGame()
    {
        Application.Quit();
    }
}
