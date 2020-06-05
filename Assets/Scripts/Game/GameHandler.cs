using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Game))]
//Main script of the game
public class GameHandler : MonoBehaviour
{
    const float FREEZE_DISTANCE = 235;

    public static GameHandler instance;

    Transform cameraTransform;
    List<GameObject> objectsToFreeze = new List<GameObject>();
    [SerializeField]
    Vector2 spawnPosition;
    [SerializeField]
    bool startInOtherHeading;
    public infoText planeInfo;
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

    //Called instantly after initialization
    public void Awake()
    {
        instance = this;
        player = Object.Instantiate(GameAssets.Instance.Player, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);
        player.AddComponent<Follow>();
        player.GetComponent<PlaneBehaviour>().startInOtherHeading = startInOtherHeading;
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
