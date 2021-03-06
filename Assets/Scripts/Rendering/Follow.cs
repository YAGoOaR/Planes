﻿using UnityEngine;

//A script that makes camera follow player
public class Follow : MonoBehaviour
{
    readonly Vector3 startPosition = new Vector3(0, 0, 21);
    [SerializeField]
    float moveStep = 1f;
    [SerializeField]
    float offset = 10f;
    Camera Cam;
    Transform camTransform;
    Transform background;

    //Called instantly after initialization
    void Awake()
    {
        GameObject cameraObject = Instantiate(GameAssets.Instance.PlayerCam);
        camTransform = cameraObject.transform;
        camTransform.position = new Vector3(transform.position.x, transform.position.y, camTransform.position.z);
        background = camTransform.Find("background");
        Cam = cameraObject.GetComponent<Camera>();
    }

    //Called after "Awake"
    void Start()
    {
        background.transform.localPosition = startPosition;
    }

    //Called once per frame
    void Update()
    {
        float size = Cam.orthographicSize * (1 - Input.GetAxis("Mouse ScrollWheel"));
        if (size > 100)
        {
            size = 100;
        }
        else if (size < 2f)
        {
            size = 2;
        }

        Cam.orthographicSize = size;
        background.transform.localScale = new Vector3(size / 2, size / 4, 1);
        Vector3 playerPosition = transform.position;
        float rotation = transform.rotation.eulerAngles.z / 180 * Mathf.PI;
        Vector3 rotationVector = new Vector2(-Mathf.Cos(rotation), -Mathf.Sin(rotation));
        Vector3 delta = camTransform.position - new Vector3(playerPosition.x, playerPosition.y, camTransform.position.z) - rotationVector * offset;
        float distance = delta.magnitude;

        Vector3 move = delta.normalized * moveStep * (1 + 3 * distance) * Time.deltaTime;
        if (move.magnitude > distance)
        {
            move = delta;
        }
        camTransform.position -= move;
    }

}
