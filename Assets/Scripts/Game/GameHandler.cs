using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    const float FREEZE_DISTANCE = 235;

    public List<GameObject> objectsToFreeze;
    public static GameHandler instance;
    public Vector2 spawnPosition;
    public infoText planeInfo;

    Transform cameraTransform;
    GameObject player;
    PlaneBehaviour planeBehaviour;

    public bool startInOtherHeading = false;

    //info that will be showed on UI
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

    //Called instantly after initialization
    void Awake()
    {
        instance = this;

        player = GameObject.Instantiate(GameAssets.instance.player, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);
        player.AddComponent<Follow>();
        planeBehaviour = player.GetComponent<PlaneBehaviour>();
        planeBehaviour.startInOtherHeading = startInOtherHeading;
    }

    //Called after "Awake"
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    //Called once per frame
    void Update()
    {
        //freeze objects if they are too far from camera
        foreach (GameObject gameobject in objectsToFreeze)
        {
            gameobject.SetActive(Mathf.Abs(cameraTransform.position.x - gameobject.transform.position.x) < FREEZE_DISTANCE);
        }
        controls();
    }

    //Exit or restart the game
    void controls()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) GameHandler.quitGame();
        if (Input.GetKeyDown(KeyCode.R)) GameHandler.restartGame();
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
