using UnityEngine;

public class CloseButton : MonoBehaviour
{
    public void close() {
        Object.Destroy(transform.parent.gameObject);
    }
}
