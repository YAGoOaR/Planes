using UnityEngine;

//Kick attached object after initialization
public class StartKick : MonoBehaviour
{
    [SerializeField]
    Vector2 velocity = Vector2.zero;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }
}
