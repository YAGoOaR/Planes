using System.Collections.Generic;
using UnityEngine;

public class PlaneBehaviour : MonoBehaviour
{
    const float shootingAccuracy = 2;
    const float gunOffset = 1.6f;
    const float jointForce = 20f;
    const float flapMotorSpeed = 20;
    const float flapMaxTorque = 1000;
    const int bombCount = 1;
    Vector3 bombOffset = new Vector3(0, -0.5f, 0);

    public bool isPlayer = true;
    bool upsideDown, flaps = false;

    [HideInInspector]
    public bool isTurningBack;
    [HideInInspector]
    public float pitch = 0;
    [HideInInspector]
    public int throttle = 0;

    float flapAngle = 30;
    float gunOffsetAngle = -0.2f;
    int bullets = 150;

    Rigidbody2D planerb;
    JointMotor2D hingemotor = new JointMotor2D();
    JointMotor2D flapMotor = new JointMotor2D();
    HingeJoint2D flapJoint;
    HingeJoint2D hinge;
    GearController gearCtrl;
    PropellerMotor propellerMotor;
    Animator gearAnimator, planeAnimator;
    SpriteRenderer spriteRenderer;
    PlanePart wing;
    PlanePart tail;
    PlanePart propeller;
    PlanePart gear;
    PlanePart elevator;
    PlanePart flap;
    CooldownTimer turnTimer;
    CooldownTimer throttleTimer;
    CooldownTimer shootingTimer;
    Aerofoil[] Alist;
    Queue<GameObject> bombs = new Queue<GameObject>();

    PlanePart createPart(string name)
    {
        return new PlanePart(transform.Find(name).gameObject);
    }

    class PlanePart
    {
        public bool isBroken;
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

    void Start()
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


        planeAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        planerb = GetComponent<Rigidbody2D>();
        hinge = GetComponent<HingeJoint2D>();
        Alist = GetComponentsInChildren<Aerofoil>();

        gearCtrl = gear.gameObject.GetComponent<GearController>();
        propellerMotor = propeller.gameObject.GetComponent<PropellerMotor>();
        gearAnimator = gear.gameObject.GetComponent<Animator>();
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
        GameHandler.infoText info = GameHandler.instance.planeInfo;
        info.Set(propellerMotor.throttle, bullets, bombs.Count, planerb.velocity.magnitude, gearCtrl.isGearUp);
        GameHandler.instance.planeInfo = info;
    }

    void controls()
    {
        if (!isPlayer) return;
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
                throttle++;
                throttleTimer.reset();
            }
            if (Input.GetKey(KeyCode.S))
            {
                throttle--;
                throttleTimer.reset();
            }
        }

        if (turnTimer.check())
        {
            if (Input.GetKey(KeyCode.Q) && planerb.velocity.magnitude > 10f && gearCtrl.isGearUp) turn();
            if (Input.GetKey(KeyCode.E) && planerb.velocity.magnitude > 5f && gearCtrl.isGearUp) turnBack();
            if (Input.GetKey(KeyCode.G)) switchGear();
            if (Input.GetKeyDown(KeyCode.Space)) throwBomb();
            if (Input.GetKey(KeyCode.A)) pitch = -1;
            else if (Input.GetKey(KeyCode.D)) pitch = 1;
            else pitch = 0;
        }
        else pitch = 0;

        if (!elevator.isBroken)
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

        if (throttle > 100) throttle = 100;
        else if (throttle < 0) throttle = 0;
        propellerMotor.throttle = throttle;
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
        float accuracy = (Random.value - .5f) * shootingAccuracy;
        float rotation = transform.rotation.eulerAngles.z / 180 * Mathf.PI;
        Vector3 gunPos = new Vector2(-Mathf.Cos(rotation + gunOffsetAngle), -Mathf.Sin(rotation + gunOffsetAngle)) * gunOffset;
        GameObject bullet = Instantiate(GameAssets.instance.bullet, gunPos + transform.position, transform.rotation, transform);
        bullet.transform.Rotate(new Vector3(0, 0, accuracy));
        bullet.GetComponent<Rigidbody2D>().AddForce(planerb.velocity);
        bullets--;
        shootingTimer.reset();
    }

    public void OnJointBreak2D(Joint2D thejoint)
    {
        if (thejoint.connectedBody.gameObject.transform.name == "elevator") elevator.isBroken = true;
    }

    public void AddBomb()
    {
        GameObject bmb = GameObject.Instantiate(GameAssets.instance.bomb);
        bmb.transform.position += bombOffset;
        FixedJoint2D joint = bmb.GetComponent<FixedJoint2D>();
        joint.connectedAnchor = bombOffset;
        joint.connectedBody = GetComponent<Rigidbody2D>();
        bombs.Enqueue(bmb);
    }

    void turnFlaps()
    {
        JointAngleLimits2D angle = new JointAngleLimits2D();
        angle.max = 0;
        if (upsideDown)
        {
            angle.min = -flapAngle;
        }
        else
        {
            angle.min = flapAngle;
        }
        flapJoint.limits = angle;

    }

    public void rotate()
    {
        gameObject.transform.Rotate(0, 0, 180);
    }

    public void turnBack()
    {
        turnOver();
        planeAnimator.SetBool("turningBack", true);
        turnTimer.reset();
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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTurningBack && collision.gameObject.layer != 8) broke();
    }

    public void OnTriggerEnter2D()
    {
        if (isTurningBack) broke();
    }

    private void broke()
    {
        planeAnimator.enabled = planerb.isKinematic = planerb.freezeRotation = false;
        if (!propellerMotor.jointIsActive) return;
        propeller.gameObject.GetComponent<FixedJoint2D>().breakForce = 0;
    }
}
