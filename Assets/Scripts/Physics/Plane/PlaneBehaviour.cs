using System.Collections.Generic;
using UnityEngine;

public class PlaneBehaviour : MonoBehaviour
{
    public AeroPlane plane;

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
    PlanePart elevator;
    PlanePart gear;
    PlanePart propeller;
    PlanePart flap;
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
    string[] partNames = { "gear", "wing", "flap", "tail", "elevator", "propeller" };
    Timers.CooldownTimer turnTimer;
    Timers.CooldownTimer throttleTimer;
    Timers.CooldownTimer shootingTimer;
    Aerofoil[] aerofoilList;
    public Queue<GameObject> bombs = new Queue<GameObject>();

    // Find part in the plane
    GameObject findPart(string name)
    {
        return transform.Find(name).gameObject;
    }

    //the plane
    public class AeroPlane
    {
        //health points of a plane
        public int HP = 4;
        public string[] partNames;
        public PlanePart[] parts;

        public AeroPlane(string[] partNames, GameObject[] gameObjects)
        {
            this.partNames = partNames;
            int count = partNames.Length;
            parts = new PlanePart[count];
            for (int i = 0; i < count; i++)
            {
                parts[i] = new PlanePart(partNames[i], gameObjects[i]);
            }
        }

        public PlanePart getPart(string partName)
        {
            for (int i = 0; i < partName.Length; i++)
            {
                if (partNames[i] == partName) return parts[i];
            }
            return null;
        }
    }

    //Any physical plane part
    public class PlanePart
    {
        public string name;
        public bool isBroken = false;
        public GameObject gameObject { private set; get; }
        public bool isConnected
        {
            get
            {
                if (this.gameObject.GetComponent<Joint2D>()) return true;
                else return false;
            }
        }

        public PlanePart(string name, GameObject gameObject)
        {
            this.name = name;
            this.gameObject = gameObject;
        }

        public T GetComponent<T>()
        {
            return gameObject.GetComponent<T>();
        }
    }

    //Called once when this object initializes
    void Start()
    {
        addAllComponents();
        planeSetup();
    }

    //Called once per frame
    void Update()
    {
        controls();
        updateInfo();
        updateControls();
    }

    //Set Unity components
    void addAllComponents()
    {
        GameObject[] partObjects = new GameObject[partNames.Length];
        for (int i = 0; i < partNames.Length; i++)
        {
            partObjects[i] = findPart(partNames[i]);
        }
        plane = new AeroPlane(partNames, partObjects);

        elevator = plane.getPart("elevator");
        gear = plane.getPart("gear");
        propeller = plane.getPart("propeller");
        flap = plane.getPart("flap");

        planeAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        planerb = GetComponent<Rigidbody2D>();
        hinge = GetComponent<HingeJoint2D>();
        aerofoilList = gameObject.GetComponentsInChildren<Aerofoil>();

        gearCtrl = gear.GetComponent<GearController>();
        propellerMotor = propeller.GetComponent<PropellerMotor>();
        gearAnimator = gear.GetComponent<Animator>();
        flapJoint = flap.GetComponent<HingeJoint2D>();

        throttleTimer = new Timers.CooldownTimer(0.01f);
        turnTimer = new Timers.CooldownTimer(1f);
        shootingTimer = new Timers.CooldownTimer(0.1f);
    }

    //Set variables
    void planeSetup()
    {
        reloadBombs();

        flapAngle = flapJoint.limits.min;
        hingemotor.maxMotorTorque = 100;
        flapMotor.maxMotorTorque = FLAP_MAX_TORQUE;
        flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;

        if (startInOtherHeading)
        {
            forceTurnBack();
        }
    }

    void updateControls()
    {
        //applying throttle to propeller
        propellerMotor.throttle = throttle;
        //Plane pitch
        if (!elevator.isBroken)
        {
            hingemotor.motorSpeed = (pitch * 15 - hinge.jointAngle) * JOINT_FORCE;
            hinge.motor = hingemotor;
        }
        //Changing controls if animation puts plane upside down
        if (upsideDown)
        {
            JointMotor2D reverse = new JointMotor2D();
            reverse.motorSpeed = -flapMotor.motorSpeed;
            reverse.maxMotorTorque = FLAP_MAX_TORQUE;
            flapJoint.motor = reverse;
        }
        else flapJoint.motor = flapMotor;
    }

    //Information about plane that will be taken by UI 
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
        // Shooting
        if (Input.GetMouseButton(0) && bullets > 0) shoot();
        // Turning plane flaps
        if (Input.GetKeyDown(KeyCode.F)) switchFlaps();
        // Checking time cooldown to adjust throttle
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
        //Checking if turning animation is ended
        if (turnTimer.check())
        {
            //Performing a half of barrel roll
            if (Input.GetKey(KeyCode.Q) && planerb.velocity.magnitude > 10f && gearCtrl.isGearUp) turn();
            //Performing simple turn
            if (Input.GetKey(KeyCode.E) && planerb.velocity.magnitude > 5f && gearCtrl.isGearUp) turnBack();
            //Landing gear
            if (Input.GetKey(KeyCode.G)) switchGear();
            //Throwing a bomb
            if (Input.GetKeyDown(KeyCode.Space)) throwBomb();
            //Controlling elevator
            if (Input.GetKey(KeyCode.A)) pitch = -1;
            else if (Input.GetKey(KeyCode.D)) pitch = 1;
            else pitch = 0;
        }
        else pitch = 0;
        if (throttle > 100) throttle = 100;
        else if (throttle < 0) throttle = 0;
    }

    // A half of barrel roll
    public void turn()
    {
        turnOver();
        planeAnimator.SetBool("turning", true);
        turnTimer.reset();
    }

    //Place a plane upside down
    public void turnOver()
    {
        gunOffsetAngle = -gunOffsetAngle;
        Vector3 scale = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        for (int i = 0; i < aerofoilList.Length; i++)
        {
            aerofoilList[i].upside = -aerofoilList[i].upside;
        }
        upsideDown = !upsideDown;
        turnFlaps();
    }

    // throw a bomb from the plane
    public void throwBomb()
    {
        if (bombs.Count > 0)
        {
            GameObject bomb = bombs.Dequeue();
            bomb.GetComponent<FixedJoint2D>().breakForce = 0;
        }
    }

    //Landing gear switch
    public void switchGear()
    {
        if (planerb.velocity.magnitude > 5f) gearCtrl.switchGear(!gearCtrl.isGearUp);
    }

    //Turn the gear on/off
    public void switchGear(bool on)
    {
        if (planerb.velocity.magnitude > 5f) gearCtrl.switchGear(!on);
    }

    //Shoot bullets
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

    //Update if part breaks
    public void OnJointBreak2D(Joint2D thejoint)
    {
        if (thejoint.connectedBody.gameObject.transform.name == "elevator") elevator.isBroken = true;
    }

    //Adding bomb GameObjects
    public void AddBomb()
    {
        GameObject bmb = GameObject.Instantiate(GameAssets.instance.bomb, bombOffset + transform.position, Quaternion.identity);
        FixedJoint2D joint = bmb.GetComponent<FixedJoint2D>();
        joint.connectedAnchor = bombOffset;
        joint.connectedBody = GetComponent<Rigidbody2D>();
        bombs.Enqueue(bmb);
    }

    //Switching plane flaps physics after animation
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

    //Turn plane 180 degrees
    public void rotate()
    {
        gameObject.transform.Rotate(0, 0, 180);
    }

    //Simple turn animation
    public void turnBack()
    {
        turnOver();
        planeAnimator.SetBool("turningBack", true);
        turnTimer.reset();
    }

    //Turn off aerofoil(is needed during animation)
    public void switchAerofoilActive()
    {
        foreach (Aerofoil aerofoil in aerofoilList)
        {
            aerofoil.enabled = !aerofoil.enabled;
        }
    }

    //Hide bomb textures(is needed during animation)
    public void switchBombsActive()
    {
        foreach (GameObject bomb in bombs)
        {
            bomb.GetComponent<SpriteRenderer>().enabled = !bomb.GetComponent<SpriteRenderer>().enabled;
        }
    }

    //If plane collides something(during animation)
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTurningBack && collision.gameObject.layer != 8) broke();
        if (collision.gameObject.tag == "bullet" && collision.relativeVelocity.magnitude > 5f) plane.HP--;
        if (plane.HP < 1)
        {
            fullDestruction();
        }
    }

    //If plane collides a trigger(during animation)
    public void OnTriggerEnter2D()
    {
        if (isTurningBack) broke();
    }

    //Break a plane
    void broke()
    {
        planeAnimator.enabled = planerb.isKinematic = planerb.freezeRotation = false;
        if (!propellerMotor.jointIsActive) return;
        propeller.GetComponent<FixedJoint2D>().breakForce = 0;
    }

    //Totally break a plane
    void fullDestruction()
    {
        broke();
        Joint2D[] joints = GetComponents<Joint2D>();
        foreach (Joint2D joint in joints)
        {
            joint.breakForce = 0;
        }
    }

    //Perform a turn without animation
    void forceTurnBack()
    {
        rotate();
        turnOver();
    }

    //Turning plane flaps
    void switchFlaps()
    {
        flaps = !flaps;
        if (flaps) flapMotor.motorSpeed = FLAP_MOTOR_SPEED;
        else flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;
    }

    //Switching on/off
    public void switchFlaps(bool on)
    {
        flaps = on;
        if (flaps) flapMotor.motorSpeed = FLAP_MOTOR_SPEED;
        else flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;
    }

    //Reload
    void reloadBombs()
    {
        for (int a = 0; a < bombCount; a++)
        {
            AddBomb();
        }
    }
}
