using System.Collections.Generic;
using UnityEngine;

public class BombBay : MonoBehaviour
{
    Transform projectileHolder;
    Timers.CooldownTimer bombTimer;
    AeroPlane plane;

    public int BombCount
    {
        get { return bombCount; }
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
        projectileHolder = GameManager.Instance.projectileHolder;
        plane = transform.parent.GetComponent<AeroPlane>();
        ReloadBombs();
    }

    public void ThrowBomb()
    {
        if (!plane.CheckNormalState()) return;
        if (bombs.Count > 0)
        {
            if (!bombTimer.Check())
            {
                return;
            }
            bombTimer.Reset();

            GameObject bomb = bombs.Dequeue();
            //Disconnecting bomb
            bomb.GetComponent<FixedJoint2D>().breakForce = 0;
            bombCount = bombs.Count;
        }
    }

    //Creating a bomb GameObject
    public void AddBomb()
    {
        GameObject bmb = Instantiate(GameAssets.Instance.Bomb, transform.position, transform.rotation, projectileHolder);
        FixedJoint2D joint = bmb.GetComponent<FixedJoint2D>();
        joint.connectedAnchor = transform.localPosition;
        joint.connectedBody = plane.GetComponent<Rigidbody2D>();
        bombs.Enqueue(bmb);
    }

    //Hide bomb textures(during animation)
    public void SetBombsActive(bool active)
    {
        foreach (GameObject bomb in bombs)
        {
            bomb.GetComponent<SpriteRenderer>().enabled = active;
        }
    }

    void ReloadBombs()
    {
        for (int a = 0; a < bombCount; a++)
        {
            AddBomb();
        }
    }

    public void Cleanup()
    {
        foreach (GameObject bomb in bombs)
        {
            Destroy(bomb);
        }
    }
}
