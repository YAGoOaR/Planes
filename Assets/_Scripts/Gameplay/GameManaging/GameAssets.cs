using UnityEngine;

//Here are the main assets of the game
public class GameAssets : MonoBehaviour
{
    static GameAssets instance;

    [SerializeField] GameObject bomb;
    [SerializeField] PhysicsMaterial2D wheelMaterial;
    [SerializeField] PhysicsMaterial2D wheelBrakeMaterial;
    [SerializeField] GameObject[] chunks;
    [SerializeField] Sprite emptyTexture;
    [SerializeField] GameObject[] clouds;

    public static GameAssets Instance
    {
        get { return instance; }
    }

    public GameObject Bomb
    {
        get { return bomb; }
    }

    public PhysicsMaterial2D WheelMaterial
    {
        get { return wheelMaterial; }
    }

    public PhysicsMaterial2D WheelBrakeMaterial
    {
        get { return wheelBrakeMaterial; }
    }

    public Sprite EmptyTexture
    {
        get { return emptyTexture; }
    }

    [SerializeField] GameObject explosionEffect;
    public GameObject ExplosionEffect
    {
        get { return explosionEffect; }
    }

    void Awake()
    {
        instance = this;
    }

    public GameObject PickRandomCloud()
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
