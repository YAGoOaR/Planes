using UnityEngine;
using UnityEngine.UIElements;

//A script that makes camera follow player
public class FollowPlayer : MonoBehaviour
{
    [SerializeField] float moveStep = 0.5f;
    [SerializeField] float offset = 10f;
    [SerializeField] Transform player;
    [SerializeField] float maxZoomOut = 30;

    Camera Cam;
    Transform camTransform;    
    Rigidbody2D rb;

    float size = 10;

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
        size = Mathf.Clamp(size * (1 - Input.GetAxis("Mouse ScrollWheel")), 5, 65);

        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, size, 0.03f);

        Vector3 playerPosition = player.position;
        float rotation = player.rotation.eulerAngles.z / 180 * Mathf.PI;

        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float mouseDelta = Vector3.Dot(Vector3.ClampMagnitude((Vector3)mouse - player.position, maxZoomOut) / maxZoomOut, -player.right);

        Vector3 rotationVector = new Vector2(-Mathf.Cos(rotation), -Mathf.Sin(rotation));
        Vector3 delta = camTransform.position - new Vector3(playerPosition.x, playerPosition.y, camTransform.position.z) - rotationVector * offset * mouseDelta;
        float distance = delta.magnitude;

        Vector3 move = delta.normalized * moveStep * (1 + 3 * distance) * Time.deltaTime * mouseDelta;
        if (move.magnitude > distance) move = delta;
        camTransform.position += (Vector3)rb.velocity * Time.deltaTime - move;
    }

}
