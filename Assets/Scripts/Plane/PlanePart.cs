using UnityEngine;

//Any physical plane part
public class PlanePart : MonoBehaviour
{
    private bool isBroken;
    private Sprite defaultSprite;
    private SpriteRenderer spriteRenderer;

    public string PartName
    {
        get { return transform.name; }
    }

    public virtual void Start()
    {
        if (!TryGetComponent<SpriteRenderer>(out spriteRenderer)) return;
        defaultSprite = spriteRenderer.sprite;
    }

    public bool IsBroken
    {
        get { return isBroken; }
        set { isBroken = value; }
    }
    public bool getConnection()
    {
        return GetComponent<Joint2D>();
    }
    public virtual void hide(bool hide)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sprite = hide ? GameAssets.Instance.EmptyTexture : defaultSprite;
    }
}
