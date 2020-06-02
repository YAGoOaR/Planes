using System.Collections.Generic;
using UnityEngine;

public class PlaneBehaviour : MonoBehaviour
{
    int HP = 4;

    public bool startInOtherHeading = false;
    const float SHOOTING_ACCURACY = 2;
    const float GUN_OFFSET = 1.6f;
    const float JOINT_FORCE = 20f;
    const float FLAP_MOTOR_SPEED = 20;
    const float FLAP_MAX_TORQUE = 1000;
    public int bombCount = 1;
    Vector3 bombOffset = new Vector3(0, -0.5f, 0);

    public bool isPlayer = true;
    [HideInInspector]
    public bool upsideDown = false;
    [HideInInspector]
    public bool flaps = false;
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
    [HideInInspector]
    public GearController gearCtrl;
    PropellerMotor propellerMotor;
    Animator gearAnimator, planeAnimator;
    SpriteRenderer spriteRenderer;
    PlanePart wing;
    PlanePart tail;
    PlanePart propeller;
    PlanePart gear;
    PlanePart elevator;
    PlanePart flap;
    Timers.CooldownTimer turnTimer;
    Timers.CooldownTimer throttleTimer;
    Timers.CooldownTimer shootingTimer;
    Aerofoil[] Alist;
    public Queue<GameObject> bombs = new Queue<GameObject>();

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
        Alist = gameObject.GetComponentsInChildren<Aerofoil>();

        gearCtrl = gear.gameObject.GetComponent<GearController>();
        propellerMotor = propeller.gameObject.GetComponent<PropellerMotor>();
        gearAnimator = gear.gameObject.GetComponent<Animator>();
        flapJoint = flap.gameObject.GetComponent<HingeJoint2D>();

        flapAngle = flapJoint.limits.min;
        throttleTimer = new Timers.CooldownTimer(0.01f);
        turnTimer = new Timers.CooldownTimer(1f);
        shootingTimer = new Timers.CooldownTimer(0.1f);

        hingemotor.maxMotorTorque = 100;
        flapMotor.maxMotorTorque = FLAP_MAX_TORQUE;
        flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;

        if (startInOtherHeading)
        {
            forceTurnBack();
        }
    }

    void Update()
    {
        controls();
        updateInfo();
        updateControls();
    }

    void updateControls()
    {
        propellerMotor.throttle = throttle;
        if (!elevator.isBroken)
        {
            hingemotor.motorSpeed = (pitch * 15 - hinge.jointAngle) * JOINT_FORCE;
            hinge.motor = hingemotor;
        }
        if (upsideDown)
        {
            JointMotor2D reverse = new JointMotor2D();
            reverse.motorSpeed = -flapMotor.motorSpeed;
            reverse.maxMotorTorque = FLAP_MAX_TORQUE;
            flapJoint.motor = reverse;
        }
        else flapJoint.motor = flapMotor;
    }

    void updateInfo()
    {
        if (!isPlayer) return;
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

        if (Input.GetKeyDown(KeyCode.F)) switchFlaps();
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
        if (throttle > 100) throttle = 100;
        else if (throttle < 0) throttle = 0;
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
        if (planerb.velocity.magnitude > 5f) gearCtrl.switchGear(!gearCtrl.isGearUp);
    }

    public void switchGear(bool on)
    {
        if (planerb.velocity.magnitude > 5f) gearCtrl.switchGear(!on);
    }

    public void shoot()
    {
        if (!shootingTimer.check()) return;
        float accuracy = (Random.value - .5f) * SHOOTING_ACCURACY;
        float rotation = transform.rotation.eulerAngles.z / 180 * Mathf.PI;
        Vector3 gunPos = new Vector2(-Mathf.Cos(rotation + gunOffsetAngle), -Mathf.Sin(rotation + gunOffsetAngle)) * GUN_OFFSET;
        GameObject bullet = Instantiate(GameAssets.instance.bullet, gunPos + transform.position, transform.rotation);
        bullet.transform.Rotate(new Vector3(0, 0, accuracy));
        bullet.GetComponent<Rigidbody2D>().velocity = planerb.velocity;
        bullets--;
        shootingTimer.reset();
    }

    public void OnJointBreak2D(Joint2D thejoint)
    {
        if (thejoint.connectedBody.gameObject.transform.name == "elevator") elevator.isBroken = true;
    }

    public void AddBomb()
    {
        GameObject bmb = GameObject.Instantiate(GameAssets.instance.bomb, bombOffset + transform.position, Quaternion.identity);
        FixedJoint2D joint = bmb.GetComponent<FixedJoint2D>();
        joint.connectedAnchor = bombOffset;
        joint.connectedBody = GetComponent<Rigidbody2D>();
        bombs.Enqueue(bmb);
    }

    public void turnFlaps()
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

    public void switchAerofoilActive()
    {
        foreach (Aerofoil aerofoil in Alist)
        {
            aerofoil.enabled = !aerofoil.enabled;
        }
    }

    public void switchBombsActive()
    {
        foreach (GameObject bomb in bombs)
        {
            bomb.GetComponent<SpriteRenderer>().enabled = !bomb.GetComponent<SpriteRenderer>().enabled;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTurningBack && collision.gameObject.layer != 8) broke();
        if (collision.gameObject.tag == "bullet" && collision.relativeVelocity.magnitude > 5f) HP--;
        if (HP < 1)
        {
            fullDestruction();
        }
    }

    public void OnTriggerEnter2D()
    {
        if (isTurningBack) broke();
    }

    void broke()
    {
        planeAnimator.enabled = planerb.isKinematic = planerb.freezeRotation = false;
        if (!propellerMotor.jointIsActive) return;
        propeller.gameObject.GetComponent<FixedJoint2D>().breakForce = 0;
    }
    void fullDestruction()
    {
        broke();
        Joint2D[] joints = GetComponents<Joint2D>();
        foreach (Joint2D joint in joints)
        {
            joint.breakForce = 0;
        }
    }
    void forceTurnBack()
    {
        rotate();
        turnOver();
    }
    void switchFlaps()
    {
        flaps = !flaps;
        if (flaps) flapMotor.motorSpeed = FLAP_MOTOR_SPEED;
        else flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;
    }
    public void switchFlaps(bool on)
    {
        flaps = on;
        if (flaps) flapMotor.motorSpeed = FLAP_MOTOR_SPEED;
        else flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;
    }
}
