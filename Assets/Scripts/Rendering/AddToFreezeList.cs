using UnityEngine;

public class AddToFreezeList : MonoBehaviour
{
    void Start()
    {
        GameHandler.Instance.addObjectsToFreeze(gameObject);
    }
    public void OnDestroy()
    {
        GameHandler.Instance.removeObjectToFreeze(gameObject);
    }
}
