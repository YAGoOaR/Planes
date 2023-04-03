using UnityEngine;

//A script that makes camera follow player
public class FollowPlayer : MonoBehaviour
{
    [SerializeField] float moveStep = 0.5f;
    [SerializeField] float offset = 10f;
    [SerializeField] Transform player;
    Camera Cam;
    Transform camTransform;    
    Rigidbody2D rb;

    //Called instantly after initialization
    void Awake()
    {
        GameObject cameraObject = Camera.main.gameObject;
        camTransform = cameraObject.transform;
        camTransform.position = new Vector3(player.position.x, player.position.y, camTransform.position.z);
        Cam = cameraObject.GetComponent<Camera>();
        rb = player.GetComponent<Rigidbody2D>();
        Cam.orthographicSize = 10;
    }

    //Called once per frame
    void LateUpdate()
    {
        float size = Mathf.Clamp(Cam.orthographicSize * (1 - Input.GetAxis("Mouse ScrollWheel")), 5, 65);
        Cam.orthographicSize = size;
        Vector3 playerPosition = player.position;
        float rotation = player.rotation.eulerAngles.z / 180 * Mathf.PI;
        Vector3 rotationVector = new Vector2(-Mathf.Cos(rotation), -Mathf.Sin(rotation));
        Vector3 delta = camTransform.position - new Vector3(playerPosition.x, playerPosition.y, camTransform.position.z) - rotationVector * offset;
        float distance = delta.magnitude;

        Vector3 move = delta.normalized * moveStep * (1 + 3 * distance) * Time.deltaTime;
        if (move.magnitude > distance) move = delta;
        camTransform.position += (Vector3)rb.velocity * Time.deltaTime - move;
    }

}
