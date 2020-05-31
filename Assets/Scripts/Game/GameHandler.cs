using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    const float freezeDistance = 235;

    public List<GameObject> objectsToFreeze;
    public static GameHandler instance;
    public Vector2 spawnPosition;
    public float startRotation;
    public bool startUpsideDown = false;
    public infoText planeInfo;

    Transform cameraTransform;
    GameObject player;
    PlaneBehaviour planeBehaviour;

    public struct infoText
    {
        public int throttle, bullets, bombs;
        public float speed;
        public bool gear;
        public void Set(int throttle, int bullets, int bombs, float speed, bool gear)
        {
            this.throttle = throttle;
            this.bullets = bullets;
            this.bombs = bombs;
            this.speed = speed;
            this.gear = gear;
        }
    }

    void Awake()
    {
        instance = this;

        player = GameObject.Instantiate(GameAssets.instance.player);
        player.AddComponent<Follow>();
        planeBehaviour = player.GetComponent<PlaneBehaviour>();
        player.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 0);

        if (startUpsideDown)
        {
            planeBehaviour.turnOver();
        }
    }

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        foreach (GameObject obj in objectsToFreeze)
        {
            obj.SetActive(Mathf.Abs(cameraTransform.position.x - obj.transform.position.x) < freezeDistance);
        }
    }

    public Vector3 GetPlayerPosition()
    {
        return player.transform.position;
    }

    public float GetPlayerRotation()
    {
        return player.transform.eulerAngles.z / 180 * Mathf.PI;
    }

    public static void quitGame()
    {
        SceneManager.LoadScene(0);
    }

    public static void restartGame()
    {
        SceneManager.LoadScene(1);
    }
}
