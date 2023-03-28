using UnityEngine;

public class Player : MonoBehaviour
{
    void Awake()
    {
        GameManager.Instance.Player = gameObject;
    }

    public void DeathMessage()
    {
        GameManager.Instance.messageBox.ShowMessage("You died. Press \"R\" to restart", true);
    }
}
