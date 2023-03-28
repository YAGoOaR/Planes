using UnityEngine;

public class Freezeable : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.AddObjectsToFreeze(gameObject);
    }
    public void OnDestroy()
    {
        GameManager.Instance.RemoveObjectToFreeze(gameObject);
    }
}
