using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Teams.Team team = Teams.Team.Enemies;
    [SerializeField] float spawnTime = 10;
    [SerializeField] int enemiesToSpawn = 2;
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] bool spawnOnStart = true;
    Timers.Interval timer;

    public int EnemiesToSpawn { get => enemiesToSpawn; }

    void Start()
    {
        Teams.Instance.AddSpawner(team, this);
        GameHandler gameHandler = GameHandler.Instance;
        transform.parent = gameHandler.enemyHolder;

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

    void SpawnObject()
    {
        Instantiate(objectToSpawn, transform, false);
    }
}
