using UnityEngine;

//Any physical plane part
public class PlanePart : MonoBehaviour
{
    private bool isBroken;
    public bool IsBroken
    {
        get { return isBroken; }
        set { isBroken = value; }
    }
    public bool getConnection()
    {
        return GetComponent<Joint2D>();
    }
}
