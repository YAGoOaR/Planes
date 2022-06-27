using UnityEngine;

public class UnpackObjsToRoot : MonoBehaviour
{
    [SerializeField] GameObject[] objsToUnpack;

    void Start()
    {
        Transform handler = GameHandler.Instance.enemyHolder;
        foreach (GameObject obj in objsToUnpack)
        {
            obj.transform.parent = handler;
        }
    }
}
