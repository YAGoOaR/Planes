using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Game))]
//Main script of the game
public class GameHandler : MonoBehaviour
{
    const float FREEZE_DISTANCE = 235;

    private static GameHandler instance;
    public static GameHandler Instance
    {
        get { return GameHandler.instance; }
    }

    Transform cameraTransform;
    List<GameObject> objectsToFreeze;
    [SerializeField]
    Vector2 spawnPosition;

    infoText planeInfo;
    public infoText PlaneInfo
    {
        get { return planeInfo; }
        set { planeInfo = value; }
    }

    GameObject player;
    public GameObject Player
    {
        get { return player; }
    }

    //info that will be showed on UI
    public struct infoText
    {
        public readonly int throttle, bullets, bombs;
        public readonly float speed, altitude;
        public readonly bool gear, brakes;
        public infoText(int throttle, int bullets, int bombs, float altitude, float speed, bool gear, bool brakes)
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

    public static void setInstance(GameHandler gameHandler) {
        instance = gameHandler;
    }

    //Called instantly after initialization
    public void Awake()
    {
        setInstance(this);
        objectsToFreeze = new List<GameObject>();
        player = Object.Instantiate(GameAssets.Instance.Player, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);
        player.AddComponent<Follow>();
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

    static void controls()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Game.quitGame();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Game.restartGame();
        }
    }

    public void addObjectsToFreeze(GameObject gameobject)
    {
        objectsToFreeze.Add(gameobject);
    }
    public void removeObjectToFreeze(GameObject gameobject)
    {
        objectsToFreeze.Add(gameobject);
    }
}
