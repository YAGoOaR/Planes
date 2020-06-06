using UnityEngine;

//Here are the main assets of the game
public class GameAssets : MonoBehaviour
{
    static GameAssets instance;
    public static GameAssets Instance
    {
        get { return GameAssets.instance; }
    }

    public static void setInstance(GameAssets gameAssets)
    {
        instance = gameAssets;
    }

    //Called after game assets initialization
    void Awake()
    {
        setInstance(this);
    }

    // Game object prefabs

    [SerializeField]
    GameObject playerCam;
    public GameObject PlayerCam
    {
        get { return playerCam; }
    }

    [SerializeField]
    GameObject player;
    public GameObject Player
    {
        get { return player; }
    }

    [SerializeField]
    GameObject bomb;
    public GameObject Bomb
    {
        get { return bomb; }
    }

    [SerializeField]
    GameObject bullet;
    public GameObject Bullet
    {
        get { return bullet; }
    }


    [SerializeField]
    GameObject[] clouds;
    public GameObject[] Clouds
    {
        get { return clouds; }
    }

    [SerializeField]
    PhysicsMaterial2D wheelMaterial;
    public PhysicsMaterial2D WheelMaterial
    {
        get { return wheelMaterial; }
    }

    [SerializeField]
    PhysicsMaterial2D wheelBrakeMaterial;
    public PhysicsMaterial2D WheelBrakeMaterial
    {
        get { return wheelBrakeMaterial; }
    }

    [SerializeField]
    GameObject[] chunks;
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
