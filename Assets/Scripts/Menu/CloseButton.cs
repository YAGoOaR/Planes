using UnityEngine;

//A script for closing controls info screen
public class CloseButton : MonoBehaviour
{
    public void close()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
