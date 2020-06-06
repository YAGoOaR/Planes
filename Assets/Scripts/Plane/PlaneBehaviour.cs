using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AeroPlane))]
//A script that controls a plane
public class PlaneBehaviour : MonoBehaviour
{
    const float SHOOTING_ACCURACY = 2;
    const float JOINT_FORCE = 20f;
    const float FLAP_MOTOR_SPEED = 20;
    const float FLAP_MAX_TORQUE = 1000;
    const float PITCH_FORCE_COEF = 15;
    const float GEAR_UP_VELOCITY = 5;
    readonly Vector3 bombOffset = new Vector3(0, -0.5f, 0);

    AeroPlane plane;
    public AeroPlane Plane
    {
        get { return plane; }
    }

    GearController gearCtrl;
    public GearController GearCtrl
    {
        get { return gearCtrl; }
    }

    [SerializeField]
    bool isPlayer = true;
    public bool IsPlayer
    {
        get { return isPlayer; }
    }

    bool upsideDown;
    public bool UpsideDown
    {
        get { return upsideDown; }
    }

    bool isTurningBack;
    public bool IsTurningBack
    {
        get { return isTurningBack; }
        set { isTurningBack = value; }
    }

    float pitch;
    public float Pitch
    {
        get { return pitch; }
        set { pitch = value; }
    }

    int throttle;
    public int Throttle
    {
        get { return throttle; }
        set { throttle = value; }
    }

    [SerializeField]
    bool invertPitch;
    [SerializeField]
    bool startInOtherHeading;
    [SerializeField]
    float gunOffset = 1.6f;
    [SerializeField]
    float gunOffsetAngle = -0.2f;
    [SerializeField]
    float trimPitch;

    bool flaps;
    bool brakes;
    float flapAngle = 30;
    float maxPitch;
    float minPitch;
    PlanePart elevator;
    PlanePart gear;
    PlanePart propeller;
    PlanePart flap;
    Rigidbody2D planeRB;
    JointMotor2D hingemotor;
    JointMotor2D flapMotor;
    HingeJoint2D flapJoint;
    HingeJoint2D hinge;
    PropellerMotor propellerMotor;
    Animator planeAnimator;
    Timers.CooldownTimer turnTimer;
    Timers.CooldownTimer throttleTimer;
    Timers.CooldownTimer shootingTimer;
    Aerofoil[] aerofoilList;

    //Called once when this object initializes
    void Start()
    {
        defineVariables();
        defineComponents();
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
    void defineComponents()
    {
        planeAnimator = GetComponent<Animator>();
        planeRB = GetComponent<Rigidbody2D>();
        hinge = GetComponent<HingeJoint2D>();
        aerofoilList = gameObject.GetComponentsInChildren<Aerofoil>();
        gearCtrl = gear.GetComponent<GearController>();
        propellerMotor = propeller.GetComponent<PropellerMotor>();
        flapJoint = flap.GetComponent<HingeJoint2D>();
        flapMotor = new JointMotor2D();
        hingemotor = new JointMotor2D();
    }

    void defineVariables()
    {
        plane = GetComponent<AeroPlane>();
        elevator = plane.getPart("elevator");
        gear = plane.getPart("gear");
        propeller = plane.getPart("propeller");
        flap = plane.getPart("flap");
        throttleTimer = new Timers.CooldownTimer(0.01f);
        turnTimer = new Timers.CooldownTimer(1f);
        shootingTimer = new Timers.CooldownTimer(0.08f);
    }

    //Set up plane
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

        if (invertPitch)
        {
            maxPitch = -1;
        }
        else
        {
            maxPitch = 1;
        }
        minPitch = -maxPitch;
    }

    void updateControls()
    {
        //applying throttle to propeller
        propellerMotor.Throttle = throttle;
        //Plane pitch
        if (!elevator.IsBroken)
        {
            hingemotor.motorSpeed = (pitch * PITCH_FORCE_COEF - hinge.jointAngle) * JOINT_FORCE;
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
        else
        {
            flapJoint.motor = flapMotor;
        }
    }

    //Information about plane that will be taken by UI 
    void updateInfo()
    {
        //Send information only if it is player
        if (!isPlayer)
        {
            return;
        }
        GameHandler.infoText info = new GameHandler.infoText(
            propellerMotor.Throttle,
            plane.Bullets,
            plane.Bombs.Count,
            transform.position.y,
            planeRB.velocity.magnitude,
            gearCtrl.IsGearUp,
            !brakes);
        GameHandler.Instance.PlaneInfo = info;
    }

    //Player controls
    void controls()
    {
        //Control only player's plane 
        if (!isPlayer)
        {
            return;
        }
        // Shooting
        if (Input.GetMouseButton(0) && plane.Bullets > 0)
        {
            shoot();
        }
        // Turning plane flaps
        if (Input.GetKeyDown(KeyCode.F))
        {
            switchFlaps();
        }
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
            if (Input.GetKeyDown(KeyCode.B))
            {
                switchBrakes();
            }
            //Performing a half of barrel roll
            if (Input.GetKey(KeyCode.Q) && planeRB.velocity.magnitude > 10f && gearCtrl.IsGearUp)
            {
                turn();
            }
            //Performing simple turn
            if (Input.GetKey(KeyCode.E) && planeRB.velocity.magnitude > 5f && gearCtrl.IsGearUp)
            {
                if (Mathf.Cos(MathUtils.toRadian(planeRB.rotation) - MathUtils.Vector2ToAngle(planeRB.velocity)) < -0.95f)
                {
                    Debug.Log("turn");
                    turnBack();
                }

            }
            //Landing gear
            if (Input.GetKey(KeyCode.G))
            {
                switchGear();
            }
            //Throwing a bomb
            if (Input.GetKeyDown(KeyCode.Space))
            {
                throwBomb();
            }
            //Controlling elevator
            if (Input.GetKey(KeyCode.A))
            {
                pitch = minPitch;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                pitch = maxPitch;
            }
            else
            {
                pitch = trimPitch;
            }
        }
        else
        {
            pitch = trimPitch;
        }

        MathUtils.clamp(throttle, 0, 100);
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
        //Mirroring model
        gameObject.transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
        //Inverting aerofoil
        for (int i = 0; i < aerofoilList.Length; i++)
        {
            aerofoilList[i].Upside = -aerofoilList[i].Upside;
        }
        upsideDown = !upsideDown;
        //Inverting flaps
        turnFlaps();
    }

    // throw a bomb from the plane
    public void throwBomb()
    {
        if (plane.Bombs.Count > 0)
        {
            GameObject bomb = plane.Bombs.Dequeue();
            //Disconnecting bomb
            bomb.GetComponent<FixedJoint2D>().breakForce = 0;
        }
    }

    //Landing gear switch
    public void switchGear()
    {
        //Switch only if velocity is reached
        if (planeRB.velocity.magnitude > GEAR_UP_VELOCITY)
        {
            gearCtrl.switchGear(!gearCtrl.IsGearUp);
        }
    }

    //Turn the gear on/off
    public void switchGear(bool on)
    {
        //Switch only if velocity is reached
        if (planeRB.velocity.magnitude > GEAR_UP_VELOCITY)
        {
            gearCtrl.switchGear(!on);
        }
    }

    //Shoot bullets
    public void shoot()
    {
        if (!shootingTimer.check())
        {
            return;
        }
        shootingTimer.reset();

        float accuracy = (Random.value - 0.5f) * SHOOTING_ACCURACY;
        float rotation = transform.rotation.eulerAngles.z / 180 * Mathf.PI;
        Vector3 gunPos = new Vector2(-Mathf.Cos(rotation + gunOffsetAngle), -Mathf.Sin(rotation + gunOffsetAngle)) * gunOffset;
        GameObject bullet = Instantiate(GameAssets.Instance.Bullet, gunPos + transform.position, transform.rotation);
        bullet.transform.Rotate(new Vector3(0, 0, accuracy));
        bullet.GetComponent<Rigidbody2D>().velocity = planeRB.velocity;
        plane.Bullets--;
    }

    //Update if part breaks
    public void OnJointBreak2D(Joint2D thejoint)
    {
        if (thejoint.connectedBody.gameObject.transform.name == "elevator")
        {
            elevator.IsBroken = true;
        }
    }

    //Adding bomb GameObjects
    public void AddBomb()
    {
        GameObject bmb = GameObject.Instantiate(GameAssets.Instance.Bomb, bombOffset + transform.position, Quaternion.identity);
        FixedJoint2D joint = bmb.GetComponent<FixedJoint2D>();
        joint.connectedAnchor = bombOffset;
        joint.connectedBody = GetComponent<Rigidbody2D>();
        plane.Bombs.Enqueue(bmb);
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
        foreach (GameObject bomb in plane.Bombs)
        {
            bomb.GetComponent<SpriteRenderer>().enabled = !bomb.GetComponent<SpriteRenderer>().enabled;
        }
    }

    //If plane collides something(during animation)
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTurningBack && collision.gameObject.layer != 8)
        {
            broke();
        }
        if (collision.gameObject.tag == "bullet" && collision.relativeVelocity.magnitude > 5f)
        {
            plane.Damage();
        }
        if (plane.HP < 1)
        {
            fullDestruction();
        }
    }

    //If plane collides a trigger(during animation)
    public void OnTriggerEnter2D()
    {
        if (isTurningBack)
        {
            broke();
        }
    }

    //Break a plane
    void broke()
    {
        planeAnimator.enabled = planeRB.isKinematic = planeRB.freezeRotation = false;
        foreach (Joint2D joint in GetComponents<Joint2D>())
        {
            joint.breakForce = 0;
        }
        if (isPlayer)
        {
            Game.Instance.gameOver("plane is broken");
        }
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
        if (flaps)
        {
            flapMotor.motorSpeed = FLAP_MOTOR_SPEED;
        }
        else
        {
            flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;
        }
    }

    //Switching on/off
    public void switchFlaps(bool on)
    {
        flaps = on;
        if (flaps)
        {
            flapMotor.motorSpeed = FLAP_MOTOR_SPEED;
        }
        else
        {
            flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;
        }
    }

    public void updateFlaps()
    {
        if (flaps)
        {
            flapMotor.motorSpeed = FLAP_MOTOR_SPEED;
        }
        else
        {
            flapMotor.motorSpeed = -FLAP_MOTOR_SPEED;
        }
    }

    //Switching wheel brakes
    void switchBrakes()
    {
        brakes = !brakes;
        updateBrakes();
    }

    //Switching on/off
    public void switchBrakes(bool on)
    {
        brakes = on;
        updateBrakes();
    }

    //Make gearController switching brakes
    public void updateBrakes()
    {
        gearCtrl.switchBrakes(brakes);
    }

    //Reload
    void reloadBombs()
    {
        for (int a = 0; a < plane.BombCount; a++)
        {
            AddBomb();
        }
    }
}
