using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Game))]
//Main script of the game
public class GameHandler : MonoBehaviour
{
    const float FREEZE_DISTANCE = 235;

    public List<GameObject> objectsToFreeze;
    public static GameHandler instance;
    public Vector2 spawnPosition;
    public infoText planeInfo;

    Transform cameraTransform;
    [HideInInspector]
    public GameObject player;
    PlaneBehaviour planeBehaviour;

    public bool startInOtherHeading = false;

    //info that will be showed on UI
    public struct infoText
    {
        public int throttle, bullets, bombs;
        public float speed, altitude;
        public bool gear, brakes;
        public void Set(int throttle, int bullets, int bombs, float altitude, float speed, bool gear, bool brakes)
        {
            this.throttle = throttle;
            this.bullets = bullets;
            this.bombs = bombs;
            this.speed = speed;
            this.gear = gear;
            this.brakes = brakes;
            this.altitude = altitude;
        }
    }

    //Called instantly after initialization
    public void Awake()
    {
        instance = this;
        player = Object.Instantiate(GameAssets.instance.player, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);
        player.AddComponent<Follow>();
        planeBehaviour = player.GetComponent<PlaneBehaviour>();
        planeBehaviour.startInOtherHeading = startInOtherHeading;
    }

    //Called after "Awake"
    public void Start()
    {
        cameraTransform = Camera.main.transform;

    }

    //Called once per frame
    public void Update()
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
        if (Input.GetKeyDown(KeyCode.Escape)) Game.quitGame();
        if (Input.GetKeyDown(KeyCode.R)) Game.restartGame();
    }
}
