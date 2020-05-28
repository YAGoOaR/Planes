using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;

    public Vector2 spawnPosition;
    public float startRotation;
    public bool startUpsideDown = false;
    private GameObject player;
    private PlaneBehaviour planeBehaviour;
    public infoText planeInfo;


    public struct infoText
    {
        public int throttle, bullets, bombs;
        public float speed;
        public bool gear;
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
