using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    [SerializeField] Text vehicleInfo;

    public static UIManager Instance { get => instance; }
    public Text VehicleInfo { get => vehicleInfo; }

    public void Awake()
    {
        instance = this;
    }
}
