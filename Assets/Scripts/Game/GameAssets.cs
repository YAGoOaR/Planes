using UnityEngine;

//Here are the main assets of the game
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
    public PhysicsMaterial2D wheelMaterial;
    public PhysicsMaterial2D wheelBrakeMaterial;

    public GameObject pickRandomCloud()
    {
        return clouds[Random.Range(0, clouds.Length)];
    }
    public GameObject GetChunk(int i)
    {
        if (i >= 0 && i < chunks.Length) return chunks[i];
        else return null;
    }
}
