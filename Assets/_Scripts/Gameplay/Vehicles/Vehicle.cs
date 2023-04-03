using UnityEngine;
using UnityEngine.Events;

public class Vehicle : MonoBehaviour
{
    [SerializeField] Transform hull;
    [SerializeField] Rigidbody2D rb;
    GameObject myPrefab;
    Transform home;

    public UnityEvent OnCleanup = new();

    private Transform startPoint;
    public Transform StartPoint { get => startPoint; set => startPoint = value; }
    public GameObject MyPrefab { get => myPrefab; set => myPrefab = value; }
    public Rigidbody2D Rb { get => rb; set => rb = value; }
    public Transform Home { get => home; set => home = value; }
    public Transform Hull { get => hull; set => hull = value; }
}
