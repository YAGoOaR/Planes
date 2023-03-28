using UnityEngine;

public class RepairZone : MonoBehaviour
{
    [SerializeField] float repairTime = 7;
    [SerializeField] VehicleSpawner spawnPoint;
    public float RepairTime { get => repairTime; set => repairTime = value; }

    public void Respawn(Vehicle vehicle)
    {
        vehicle.OnCleanup.Invoke();
        GameObject prefab = vehicle.MyPrefab;
        Destroy(vehicle.gameObject);
        spawnPoint.SpawnObject(prefab);
    }

}
