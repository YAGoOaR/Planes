using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
//    [SerializeField] Vector2 parallaxeffectMultiplier;
    private Transform camTransform;
    private Vector3 lastCameraPos;
    private float TextureUnitSizeX;

    void Start()
    {
        camTransform = Camera.main.transform;
        lastCameraPos = camTransform.position;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        TextureUnitSizeX = texture.width / sprite.pixelsPerUnit;
    }

    void LateUpdate()
    {
//        Debug.Log(camTransform.position.x - transform.position.x);
//        Vector3 deltaMovement = camTransform.position - lastCameraPos;
//        transform.position +=new Vector3(deltaMovement.x * ParallaxEffectMultiplier);
        if (Mathf.Abs(camTransform.position.x - transform.position.x) >= TextureUnitSizeX) {
            float offset = (transform.position.x - camTransform.position.x) % TextureUnitSizeX;
            transform.position = new Vector3(camTransform.position.x + offset, transform.position.y);

        }
    }
}
