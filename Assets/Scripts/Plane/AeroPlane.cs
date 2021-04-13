using System.Collections.Generic;
using UnityEngine;

public class AeroPlane : MonoBehaviour
{
    [SerializeField]
    private string[] partNames = { "gear", "wing", "flap", "tail", "elevator", "propeller" };
    private PlanePart[] parts;
    [SerializeField]
    private int hp = 4;
    [SerializeField]
    private int bullets = 150;
    [SerializeField]
    private int bombCount = 1;
    Queue<GameObject> bombs;
    
    public PlanePart[] Parts
    {
        get { return parts; }
    }

    public int HP
    {
        get { return hp; }
    }

    public int Bullets
    {
        get { return bullets; }
        set { bullets = value; }
    }

    public int BombCount
    {
        get { return bombCount; }
        set { bombCount = value; }
    }

    public Queue<GameObject> Bombs
    {
        get { return bombs; }
    }

    //Called after plane initialization
    void Awake()
    {
        bombs = new Queue<GameObject>();
        int count = partNames.Length;
        parts = new PlanePart[count];
        for (int i = 0; i < count; i++)
        {
            parts[i] = transform.Find(partNames[i]).gameObject.AddComponent<PlanePart>();
        }
    }

    public PlanePart getPart(string name)
    {
        for (int i = 0; i < partNames.Length; i++)
        {
            if (partNames[i] == name)
            {
                return parts[i];
            }
        }
        return null;
    }

    public void Damage()
    {
        hp--;
    }
}
