using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Game))]
//Main script of the game
public class GameHandler : MonoBehaviour
{
    const float FREEZE_DISTANCE = 270;

    public Transform projectileHolder;
    public Transform chunkHolder;
    public Transform enemyHolder;
    public PlaneInfoShow planeinfoShow;
    public GameMessageHandler messageBox;

    private static GameHandler instance;
    public static GameHandler Instance { get => instance; }

    Game game;

    Transform cameraTransform;
    List<GameObject> objectsToFreeze;
    [SerializeField]
    Vector2 spawnPosition;

    GameObject player;
    public GameObject Player { get => player; set => player = value; }

    public static void SetInstance(GameHandler gameHandler) {
        instance = gameHandler;
    }

    //Called instantly after initialization
    public void Awake()
    {
        SetInstance(this);
        objectsToFreeze = new List<GameObject>();
        game = GetComponent<Game>();
    }

    public void GameOver(string msg) 
    {
        messageBox.ShowMessage(msg, true);
        game.GameOver(msg);
    }

    public void GameWin(string msg)
    {
        messageBox.ShowMessage(msg, true);
        game.GameWin(msg);
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
            if (gameobject == null) continue;
            gameobject.SetActive(Mathf.Abs(cameraTransform.position.x - gameobject.transform.position.x) < FREEZE_DISTANCE);
        }
        Controls();
    }

    static void Controls()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            instance.game.QuitGame();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            instance.game.RestartGame();
        }
    }

    public void AddObjectsToFreeze(GameObject gameobject)
    {
        objectsToFreeze.Add(gameobject);
    }
    public void RemoveObjectToFreeze(GameObject gameobject)
    {
        objectsToFreeze.Add(gameobject);
    }
}
