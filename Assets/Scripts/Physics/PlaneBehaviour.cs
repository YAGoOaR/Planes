using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneBehaviour : MonoBehaviour
{

    const float accuracy = 2;
    const float gunOffset = 1.6f;
    const float jointForce = 20f;
    const float flapMotorSpeed = 20;
    const float flapMaxTorque = 1000;
    const int bombCount = 1;

    bool isPlayer = true;
    bool isTurningBack, upsideDown, flaps, elevatorBroken = false;

    private float pitch = 0;
    float flapAngle = 30;
    float gunOffsetAngle = -0.2f;

    int bullets = 150;


    Rigidbody2D planerb;

    JointMotor2D hingemotor = new JointMotor2D();
    JointMotor2D flapMotor = new JointMotor2D();

    GearController gearCtrl;

    PropellerMotor propellerMotor;

    HingeJoint2D flapJoint, hinge;

    Animator gearAnimator, planeAnimator;

    SpriteRenderer spriteRenderer;

    PlanePart wing, tail, propeller, gear, elevator, flap;

    CooldownTimer turnTimer, throttleTimer, shootingTimer;

    Aerofoil[] Alist;
    Queue<GameObject> bombs = new Queue<GameObject>();

    PlanePart createPart(string name)
    {
        return new PlanePart(transform.Find(name).gameObject);
    }


    class PlanePart
    {
        public bool isBroken { private set; get; }
        public GameObject gameObject { private set; get; }
        public bool isConnected
        {
            get
            {
                if (this.gameObject.GetComponent<Joint2D>()) return true;
                else return false;
            }
        }

        public PlanePart(GameObject obj)
        {
            isBroken = false;
            this.gameObject = obj;
        }
    }

    class CooldownTimer
    {
        static List<CooldownTimer> timers = new List<CooldownTimer>();
        private float curTime;
        private float maxTime;

        public CooldownTimer(float max)
        {
            timers.Add(this);
            maxTime = max;
            curTime = max;
        }

        public static void refresh()
        {
            foreach (CooldownTimer timer in timers)
            {
                timer.curTime += Time.deltaTime;
            }
        }

        public bool check()
        {
            return curTime > maxTime;
        }
        public void reset()
        {
            curTime = 0f;
        }
    }

    void Awake()
    {
        for (int a = 0; a < bombCount; a++)
        {
            AddBomb();
        }

        gear = createPart("gear");
        wing = createPart("wing");
        flap = createPart("flap");
        tail = createPart("tail");
        elevator = createPart("elevator");
        propeller = createPart("propeller");

        gearCtrl = gear.gameObject.GetComponent<GearController>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        planerb = gameObject.GetComponent<Rigidbody2D>();
        hinge = gameObject.GetComponent<HingeJoint2D>();
        propellerMotor = propeller.gameObject.GetComponent<PropellerMotor>();

        planeAnimator = gameObject.GetComponent<Animator>();
        gearAnimator = gear.gameObject.GetComponent<Animator>();

        Alist = gameObject.GetComponentsInChildren<Aerofoil>();
        flapJoint = flap.gameObject.GetComponent<HingeJoint2D>();

        flapAngle = flapJoint.limits.min;
        throttleTimer = new CooldownTimer(0.01f);
        turnTimer = new CooldownTimer(1f);
        shootingTimer = new CooldownTimer(0.1f);

        hingemotor.maxMotorTorque = 100;
        flapMotor.maxMotorTorque = flapMaxTorque;
        flapMotor.motorSpeed = -flapMotorSpeed;
    }

    void Update()
    {
        CooldownTimer.refresh();
        controls();
        updateInfo();

    }

    void updateInfo()
    {

        GameHandler.infoText info = GameHandler.i.planeInfo;
        info.bombs = bombs.Count;
        info.speed = planerb.velocity.magnitude;
        info.throttle = propellerMotor.num;
        info.gear = gearCtrl.gearUp;
        info.bullets = bullets;
        GameHandler.i.planeInfo = info;
    }

    void controls()
    {
        if (!isPlayer)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) GameHandler.quitGame();
        if (Input.GetKeyDown(KeyCode.R)) GameHandler.restartGame();
        if (Input.GetMouseButton(0) && bullets > 0) shoot();
        if (Input.GetKeyDown(KeyCode.F))
        {
            flaps = !flaps;
            if (flaps) flapMotor.motorSpeed = flapMotorSpeed;
            else flapMotor.motorSpeed = -flapMotorSpeed;
        };
        if (throttleTimer.check())
        {

            if (Input.GetKey(KeyCode.W))
            {
                throttle(1);
                throttleTimer.reset();
            }
            if (Input.GetKey(KeyCode.S))
            {
                throttle(-1);
                throttleTimer.reset();

            }
        }

        if (turnTimer.check())
        {
            if (Input.GetKey(KeyCode.Q))
            {
                if (planerb.velocity.magnitude > 5f && gearCtrl.gearUp)
                {
                    turn();
                }
            }
            if (Input.GetKey(KeyCode.E))
            {
                if (planerb.velocity.magnitude > 5f && gearCtrl.gearUp)
                {
                    turnBack();
                }
            }
            if (Input.GetKey(KeyCode.G))
            {
                switchGear();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                throwBomb();
            }

            if (Input.GetKey(KeyCode.A))
            {
                pitch = -1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                pitch = 1;
            }
            else pitch = 0;
        }
        else
        {
            pitch = 0;
        }

        if (!elevatorBroken)
        {
            hingemotor.motorSpeed = (pitch * 15 - hinge.jointAngle) * jointForce;
            hinge.motor = hingemotor;
        }
        if (upsideDown)
        {
            JointMotor2D reverse = new JointMotor2D();
            reverse.motorSpeed = -flapMotor.motorSpeed;
            reverse.maxMotorTorque = flapMaxTorque;
            flapJoint.motor = reverse;
        }
        else flapJoint.motor = flapMotor;

    }

    public void turn()
    {
        turnOver();
        planeAnimator.SetBool("turning", true);
        turnTimer.reset();
    }

    public void turnOver()
    {
        gunOffsetAngle = -gunOffsetAngle;
        Vector3 scale = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        for (int i = 0; i < Alist.Length; i++)
        {
            Alist[i].upside = -Alist[i].upside;
        }
        upsideDown = !upsideDown;
        turnFlaps();
    }

    public void setThrottle(int throttle)
    {
        propellerMotor.SetThrottle(throttle);
    }

    public void throwBomb()
    {
        if (bombs.Count > 0)
        {
            GameObject bomb = bombs.Dequeue();
            bomb.GetComponent<FixedJoint2D>().breakForce = 0;
        }
    }

    public void switchGear()
    {
        if (planerb.velocity.magnitude > 5f) gearCtrl.switchGear();
    }

    public void shoot()
    {
        if (!shootingTimer.check())
        {
            return;
        }
        float acc = (Random.value - .5f) * accuracy;
        float rotation = transform.rotation.eulerAngles.z / 180 * Mathf.PI;
        Vector3 gunPos = new Vector2(-Mathf.Cos(rotation + gunOffsetAngle), -Mathf.Sin(rotation + gunOffsetAngle)) * gunOffset;
        GameObject bullet = Instantiate(GameAssets.i.bullet, gunPos + transform.position, transform.rotation);
        bullet.transform.Rotate(new Vector3(0, 0, acc));
        bullet.GetComponent<Rigidbody2D>().AddForce(planerb.velocity);
        bullets--;
        shootingTimer.reset();
    }

    public void OnJointBreak2D(Joint2D thejoint)
    {
        if (thejoint.connectedBody.gameObject.transform.name == "elevator") elevatorBroken = true;
    }

    public void AddBomb()
    {
        GameObject bmb = GameObject.Instantiate(GameAssets.i.bomb);
        bmb.transform.position += new Vector3(0, -0.5f, 0);
        bmb.GetComponent<FixedJoint2D>().connectedAnchor = new Vector3(0, -0.5f, 0);
        bmb.GetComponent<FixedJoint2D>().connectedBody = gameObject.GetComponent<Rigidbody2D>();
        bombs.Enqueue(bmb);
        bmb.transform.localScale = new Vector3(0.6f, 0.6f, 0);
    }
    void turnFlaps()
    {
        JointAngleLimits2D angle = new JointAngleLimits2D();
        if (upsideDown)
        {
            angle.max = 0;
            angle.min = -flapAngle;
        }
        else
        {
            angle.max = 0;
            angle.min = flapAngle;
        }
        flapJoint.limits = angle;

    }

    public void turnBack()
    {
        turnOver();
        planeAnimator.SetBool("turningBack", true);
        turnTimer.reset();
    }

    public void rotate()
    {
        gameObject.transform.Rotate(0, 0, 180);
    }

    public void switchAerofoil()
    {
        foreach (Aerofoil aerofoil in Alist)
        {
            aerofoil.enabled = !aerofoil.enabled;
        }
    }

    public void switchBmb()
    {
        foreach (GameObject bomb in bombs)
        {
            bomb.GetComponent<SpriteRenderer>().enabled = !bomb.GetComponent<SpriteRenderer>().enabled;
        }
    }

    public void OnCollisionEnter2D()
    {
        if (isTurningBack) broke();
    }

    public void OnTriggerEnter2D()
    {
        if (isTurningBack) broke();
    }

    private void broke()
    {
        planeAnimator.enabled = false;
        planerb.isKinematic = false;
        planerb.freezeRotation = false;
        propeller.gameObject.GetComponent<FixedJoint2D>().enabled = false;
    }

    public void setTurningBack(bool set)
    {
        isTurningBack = set;
    }

    public void throttle(int n)
    {
        propellerMotor.throttle(n);
    }
}
