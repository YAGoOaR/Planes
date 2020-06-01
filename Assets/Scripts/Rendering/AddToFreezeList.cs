using UnityEngine;

public class AddToFreezeList : MonoBehaviour
{
    
    void Start()
    {
        GameHandler.instance.objectsToFreeze.Add(gameObject);
    }
    void OnDestroy() {
        GameHandler.instance.objectsToFreeze.Remove(gameObject);
    }
}
