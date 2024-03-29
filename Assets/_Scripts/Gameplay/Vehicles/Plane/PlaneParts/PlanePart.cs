﻿using UnityEngine;
using UnityEngine.Events;

public class PlanePart : MonoBehaviour
{
    public UnityEvent OnBreak;
    private bool isBroken;
    private Sprite defaultSprite;
    [SerializeField] SpriteRenderer spriteRenderer;
    AnchoredJoint2D joint;
    public PlanePartManager.PartType partType = PlanePartManager.PartType.other;

    public string PartName
    {
        get { return transform.name; }
    }

    public virtual void Start()
    {
        joint = GetComponent<AnchoredJoint2D>();

        Transform hull = GetComponentInParent<Vehicle>().Hull;
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

    public bool GetConnection()
    {
        return joint;
    }
    public virtual void Hide(bool hide)
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
