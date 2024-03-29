﻿using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Game))]
//Main script of the game
public class GameManager : MonoBehaviour
{
    const float FREEZE_DISTANCE = 1400;

    public Transform projectileHolder;
    public Transform chunkHolder;
    public Transform enemyHolder;
    public GameMessageHandler messageBox;

    private static GameManager instance;
    public static GameManager Instance { get => instance; }

    Game game;

    Transform cameraTransform;
    List<GameObject> objectsToFreeze;
    [SerializeField] Vector2 spawnPosition;

    Transform player;
    public Transform Player { get => player; set => player = value; }

    public void Awake()
    {
        instance = this;
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

    public void Start()
    {
        cameraTransform = Camera.main.transform;
    }

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
