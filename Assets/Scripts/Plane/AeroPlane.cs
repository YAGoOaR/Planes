using System.Collections.Generic;
using UnityEngine;

public class AeroPlane : MonoBehaviour
{
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
    void Start()
    {
        bombs = new Queue<GameObject>();
        parts = transform.GetComponentsInChildren<PlanePart>(true);
    }

    public PlanePart getPart(string name)
    {
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].PartName == name)
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
