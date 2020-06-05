using UnityEngine;

public class AddToFreezeList : MonoBehaviour
{
    void Start()
    {
        GameHandler.instance.addObjectsToFreeze(gameObject);
    }
    public void OnDestroy()
    {
        GameHandler.instance.removeObjectToFreeze(gameObject);
    }
}
