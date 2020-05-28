using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets instance;

    void Awake()
    {
        instance = this;
    }

    public GameObject PlayerCam;
    public GameObject player;
    public GameObject[] chunks;
    public GameObject bomb;
    public Sprite turnSprite;
    public Sprite planeSprite;
    public Sprite gearSprite;
    public GameObject bullet;
    public GameObject[] clouds;

    public GameObject pickRandomCloud()
    {
        return clouds[Random.Range(0, clouds.Length)];
    }
    public GameObject GetChunk(int n)
    {
        if (n >= 0 && n < chunks.Length) return chunks[n];
        else return null;
    }
}
