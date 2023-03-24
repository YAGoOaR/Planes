using UnityEngine;

public class Freezeable : MonoBehaviour
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
