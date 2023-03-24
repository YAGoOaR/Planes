using UnityEngine;

public class Player : MonoBehaviour
{
    void Awake()
    {
        GameHandler.Instance.Player = gameObject;
    }

    public void DeathMessage()
    {
        GameHandler.Instance.messageBox.ShowMessage("You died. Press \"R\" to restart", true);
    }
}
