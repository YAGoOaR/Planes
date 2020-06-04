using UnityEngine;

//A script for close button
public class CloseButton : MonoBehaviour
{
    public void close()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
