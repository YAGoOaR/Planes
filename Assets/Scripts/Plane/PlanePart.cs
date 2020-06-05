using UnityEngine;

//Any physical plane part
public class PlanePart : MonoBehaviour
{
    public bool isBroken;
    public bool isConnected
    {
        get
        {
            return !!GetComponent<Joint2D>();
        }
    }
}
