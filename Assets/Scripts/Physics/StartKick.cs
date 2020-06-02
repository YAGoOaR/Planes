using UnityEngine;

//Kick attached object after initialization
public class StartKick : MonoBehaviour
{
    public Vector2 velocity;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }
}
