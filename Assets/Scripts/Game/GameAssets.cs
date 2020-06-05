using UnityEngine;

//Here are the main assets of the game
public class GameAssets : MonoBehaviour
{
    static GameAssets instance;
    public static GameAssets Instance
    {
        get { return GameAssets.instance; }
    }

    void Awake()
    {
        instance = this;
    }

    [SerializeField]
    private GameObject playerCam;

    public GameObject PlayerCam
    {
        get { return playerCam; }
    }

    [SerializeField]
    public GameObject player;

    public GameObject Player
    {
        get { return player; }
    }

    [SerializeField]
    private GameObject bomb;

    public GameObject Bomb
    {
        get { return bomb; }
    }

    [SerializeField]
    private Sprite planeSprite;

    public Sprite PlaneSprite
    {
        get { return planeSprite; }
    }

    private Sprite gearSprite;

    [SerializeField]
    public Sprite GearSprite
    {
        get { return gearSprite; }
    }

    [SerializeField]
    public GameObject bullet;

    public GameObject Bullet
    {
        get { return bullet; }
    }

    [SerializeField]
    private GameObject[] clouds;

    public GameObject[] Clouds
    {
        get { return clouds; }
    }

    [SerializeField]
    private PhysicsMaterial2D wheelMaterial;

    public PhysicsMaterial2D WheelMaterial
    {
        get { return wheelMaterial; }
    }
    [SerializeField]
    private PhysicsMaterial2D wheelBrakeMaterial;

    public PhysicsMaterial2D WheelBrakeMaterial
    {
        get { return wheelBrakeMaterial; }
    }

    [SerializeField]
    private GameObject[] chunks;

    public GameObject[] Chunks
    {
        get { return chunks; }
    }

    public GameObject pickRandomCloud()
    {
        return clouds[Random.Range(0, clouds.Length)];
    }
    public GameObject GetChunk(int i)
    {
        if (i >= 0 && i < chunks.Length)
        {
            return chunks[i];
        }
        else
        {
            return null;
        }
    }
}
