using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [SerializeField] Teams.Team team = Teams.Team.Enemies;
    [SerializeField] float spawnTime = 10;
    [SerializeField] int enemiesToSpawn = 2;
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] bool spawnOnStart = true;
    [SerializeField] Transform homePoint;
    [SerializeField] float placeObstructionRefreshTime = 1f;
    Timers.Interval timer;
    GameHandler gameHandler;

    bool busy = false;

    public int EnemiesToSpawn { get => enemiesToSpawn; }

    void Start()
    {
        Teams.Instance.AddSpawner(team, this);
        gameHandler = GameHandler.Instance;

        if (spawnOnStart)
        {
            if (enemiesToSpawn <= 0) return;
            enemiesToSpawn--;
            SpawnObject();
        }

        timer = Timers.SetInterval(spawnTime, () => {
            if (enemiesToSpawn <= 0) { timer?.Clear(); return; };
            enemiesToSpawn--;
            SpawnObject();
        });

    }

    public void SpawnObject() => SpawnObject(objectToSpawn);
    public void SpawnObject(GameObject objectToSpawn)
    {
        void KeepTrying()
        {
            if (busy)
            {
                busy = false;
                Timers.Delay(placeObstructionRefreshTime, () => KeepTrying());
            }
            else SpawnImmediately(objectToSpawn);
        }
        KeepTrying();
    }

    void SpawnImmediately(GameObject objectToSpawn)
    {
        GameObject obj = Instantiate(objectToSpawn, transform.position, objectToSpawn.transform.rotation, gameHandler.enemyHolder);
        Vehicle vehicle = obj.GetComponent<Vehicle>();
        vehicle.MyPrefab = objectToSpawn;
        vehicle.Home = homePoint != null ? homePoint : transform;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        busy = true;
    }
}
