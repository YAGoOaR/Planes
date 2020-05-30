using UnityEngine;

public class AddToFreezeList : MonoBehaviour
{
    
    void Start()
    {
        GameHandler.instance.objectsToFreeze.Add(gameObject);
    }
}
