using UnityEngine;

//A script that kicks attached gameobject into its heading
public class ForwardKick : MonoBehaviour
{
    [SerializeField]
    float force = 1;

    void Start()
    {
        float rotation = (transform.rotation.eulerAngles.z) / 180 * Mathf.PI;
        gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector3(-Mathf.Cos(rotation), -Mathf.Sin(rotation)) * force);
    }
}
