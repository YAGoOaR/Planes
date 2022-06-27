using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBay : MonoBehaviour
{
    Transform projectileHolder;
    Timers.CooldownTimer bombTimer;
    PlaneBehaviour plane;

    public int BombCount
    {
        get { return bombCount; }
        set { bombCount = value; }
    }

    [SerializeField] int bombCount = 2;

    public Queue<GameObject> Bombs
    {
        get { return bombs; }
    }
    Queue<GameObject> bombs;

    void Awake()
    {
        bombs = new Queue<GameObject>();
        bombTimer = new Timers.CooldownTimer(0.5f);
        projectileHolder = GameHandler.Instance.projectileHolder;
    }

    private void Start()
    {
        plane = transform.parent.GetComponent<PlaneBehaviour>();
        reloadBombs();
    }

    void Update()
    {
        
    }

    public void throwBomb()
    {
        if (!plane.checkNormalState()) return;
        if (bombs.Count > 0)
        {
            if (!bombTimer.check())
            {
                return;
            }
            bombTimer.reset();

            GameObject bomb = bombs.Dequeue();
            //Disconnecting bomb
            bomb.GetComponent<FixedJoint2D>().breakForce = 0;
        }
    }

    //Creating a bomb GameObject
    public void AddBomb()
    {
        GameObject bmb = Instantiate(GameAssets.Instance.Bomb, transform.position, Quaternion.identity, projectileHolder);
        FixedJoint2D joint = bmb.GetComponent<FixedJoint2D>();
        joint.connectedAnchor = transform.localPosition;
        joint.connectedBody = plane.GetComponent<Rigidbody2D>();
        bombs.Enqueue(bmb);
    }

    //Hide bomb textures(during animation)
    public void setBombsActive(bool active)
    {
        foreach (GameObject bomb in bombs)
        {
            bomb.GetComponent<SpriteRenderer>().enabled = active;
        }
    }

    void reloadBombs()
    {
        for (int a = 0; a < bombCount; a++)
        {
            AddBomb();
        }
    }

}
