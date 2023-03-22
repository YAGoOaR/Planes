using UnityEngine;

public class AddToFreezeList : MonoBehaviour
{
    void Start()
    {
        GameHandler.Instance.AddObjectsToFreeze(gameObject);
    }
    public void OnDestroy()
    {
        GameHandler.Instance.RemoveObjectToFreeze(gameObject);
    }
}
