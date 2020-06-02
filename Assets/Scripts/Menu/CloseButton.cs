using UnityEngine;

//A script for close button
public class CloseButton : MonoBehaviour
{
    public void close()
    {
        Object.Destroy(transform.parent.gameObject);
    }
}
