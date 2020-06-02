using UnityEngine;

public class StartKick : MonoBehaviour
{
    public Vector2 velocity;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }
}
