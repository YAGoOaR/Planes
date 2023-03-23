using UnityEngine;

public class DropoutObjects : MonoBehaviour
{
    [SerializeField] GameObject[] objsToUnpack;
    [SerializeField] Transform holder = null;

    public void Drop()
    {
        foreach (GameObject obj in objsToUnpack)
        {
            obj.transform.parent = holder;
        }
    }

    public void DropAndDestroy(float time=1f)
    {
        foreach (GameObject obj in objsToUnpack)
        {
            obj.transform.parent = holder;
            obj.AddComponent<SelfDestroy>().MaxTime = time;
        }
    }
}
