using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardKick : MonoBehaviour
{

    public float force;

    void Start()
    {
        float rotation = (transform.rotation.eulerAngles.z) / 180 * Mathf.PI;
        gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector3(-Mathf.Cos(rotation), -Mathf.Sin(rotation)) * force);
    }


}
