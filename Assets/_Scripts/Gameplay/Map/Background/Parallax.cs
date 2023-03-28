using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private bool foreground = false;
    [SerializeField] private bool foregroundFollowPlayer = true;
    [SerializeField] private float foregroundCoef = 1;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool moveVertical = false;

    private Transform trackTransform;

    private SpriteRenderer spriteRenderer;

    private Vector2 prevCamPos;
    Vector3 offset;

    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        trackTransform = foreground && foregroundFollowPlayer ? GameManager.Instance.Player.transform : cameraTransform;
        offset = transform.position;
        prevCamPos = cameraTransform.position;
    }

    Vector3 ModuloBackroundPos(Vector3 a, Vector2 b) {
        return new Vector3(a.x % b.x, 0, 0);
    }

    Vector3 HorModuloBackroundPos(Vector3 a, Vector2 b)
    {
        return new Vector3(a.x % b.x, moveVertical ? a.y % b.y : a.y, a.z);
    }

    void LateUpdate()
    {
        if (trackTransform == null) return;

        Vector3 camPos = cameraTransform.position;

        if (foreground)
        {
            Vector3 delta = (Vector2)camPos - prevCamPos;
            transform.Translate(-delta * foregroundCoef);
        }

        Vector2 spriteUnitSize = spriteRenderer.sprite.bounds.extents*2;
        transform.position = offset + camPos - HorModuloBackroundPos(camPos, spriteUnitSize) + (foreground ? ModuloBackroundPos(transform.position, spriteUnitSize) : Vector3.zero);
        
        prevCamPos = camPos;
    }
}
