using UnityEngine;
using UnityEngine.Events;

//Any physical plane part
public class PlanePart : MonoBehaviour
{
    public UnityEvent OnBreak;
    private bool isBroken;
    private Sprite defaultSprite;
    [SerializeField] SpriteRenderer spriteRenderer;
    AnchoredJoint2D joint;

    public string PartName
    {
        get { return transform.name; }
    }

    public virtual void Start()
    {
        joint = GetComponent<AnchoredJoint2D>();

        GameObject hull = transform.parent.GetComponent<Hull>().hull;
        joint.connectedBody = hull.GetComponent<Rigidbody2D>();

        if (spriteRenderer == null) TryGetComponent(out spriteRenderer);
        if (spriteRenderer == null) return;
        defaultSprite = spriteRenderer.sprite;
    }

    public bool IsBroken
    {
        get { return isBroken; }
        set { isBroken = value; }
    }

    public bool getConnection()
    {
        return joint;
    }
    public virtual void hide(bool hide)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sprite = hide ? GameAssets.Instance.EmptyTexture : defaultSprite;
    }

    private void OnJointBreak2D(Joint2D joint)
    {
        isBroken = true;
        OnBreak.Invoke();
    }

    public void Break()
    {
        if (joint == null) return;
        joint.breakForce = 0;
    }
}
