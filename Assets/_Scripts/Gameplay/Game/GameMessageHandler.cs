using UnityEngine;
using UnityEngine.UI;
public class GameMessageHandler : MonoBehaviour
{
    Timers.Timeout timer;
    [SerializeField] float messageTime = 3;
    Text text;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    public void ShowMessage(string message, bool force = false)
    {
        if (!force && timer?.Check() == false) return;
        Debug.Log(message);
        timer?.Abort();

        if (text == null) return;
        text.text = message;
        text.enabled = true;
        timer = Timers.Delay(messageTime , () => { if (text != null) text.enabled = false; });
    }
}
