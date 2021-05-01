using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AeroPlane))]
//A script that controls a plane
public class PlaneBehaviour : MonoBehaviour
{
    AeroPlane plane;
    GearController gearCtrl;
    float pitch;
    bool upsideDown;
    int throttle;

    bool invertPitch = false;

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
    Timers.CooldownTimer throttleTimer;
    Timers.CooldownTimer shootingTimer;
    Aerofoil[] aerofoilList;
    PlaneAnimator planeAnimator;
    PlaneState state;
    Vector3 lastVelocity;
    float turnTimer = 0;
    float turnSpeedMultiplier = 1;

    bool isBroken = false;

    float gunOffsetAngle
    {
        get
        {
            return upsideDown ? -plane.Model.GunOffsetAngle : plane.Model.GunOffsetAngle;
        }
    }

    public AeroPlane Plane { get;  }

    public PlaneState State { get; }

    public GearController GearCtrl
    {
        get { return gearCtrl; }
    }

    public enum PlaneState
    {
        common,
        turning,
        stalling,
        turningBack
    }

    public bool IsPlayer{ get; }

    public bool UpsideDown { get; }

    public float Pitch { get; set; }

    public int Throttle { get; set; }

    //Called after this object initialization
    void Start()
    {
        defineVariables();
        defineComponents();
        planeSetup();
    }

    //Called each frame
    void Update()
    {
        controls();
        updateInfo();
        updateControls();
    }

    void defineComponents()
    {
        planeAnimator = GetComponent<PlaneAnimator>();
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
        shootingTimer = new Timers.CooldownTimer(0.08f);
        state = PlaneState.common;
    }

    void planeSetup()
    {
        reloadBombs();

        flapAngle = flapJoint.limits.min;
        hingemotor.maxMotorTorque = 100;
        flapMotor.maxMotorTorque = plane.Model.FlapMaxTorque;
        flapMotor.motorSpeed = -plane.Model.FlapMotorSpeed;

        if (plane.StartInOtherHeading)
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
        propellerMotor.Throttle = throttle;
        //Plane pitch
        if (!elevator.IsBroken)
        {
            hingemotor.motorSpeed = (pitch * plane.Model.PitchForceCoef - hinge.jointAngle) * plane.Model.JointForce;
            hinge.motor = hingemotor;
        }
        //Changing controls if animation puts plane upside down
        if (upsideDown)
        {
            JointMotor2D reverse = new JointMotor2D();
            reverse.motorSpeed = -flapMotor.motorSpeed;
            reverse.maxMotorTorque = plane.Model.FlapMaxTorque;
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
        if (!plane.IsPlayer)
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

    void controls()
    {
        if (!plane.IsPlayer || isBroken)
        {
            return;
        }
        if (Input.GetMouseButton(0) && plane.Bullets > 0)
        {
            shoot();
        }
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

        if(state == PlaneState.turningBack)
        {
            planeRB.velocity = lastVelocity * Mathf.Cos(Mathf.Max(Mathf.PI * turnTimer  * (1 - turnTimer * plane.Model.TurnBackDrag) + plane.Model.VelocityOffset, 0));
            turnTimer += Time.deltaTime * turnSpeedMultiplier;
        }

        //Checking if turning animation is ended
        if (state != PlaneState.turning && state != PlaneState.turningBack)
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
                    turnBack();
                }

            }
            //Landing gear
            if (Input.GetKey(KeyCode.G))
            {
                switchGear();
            }
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
                pitch = plane.Model.TrimPitch;
            }
        }
        else
        {
            pitch = plane.Model.TrimPitch;
        }
        throttle = Mathf.FloorToInt(MathUtils.clamped(throttle, 0, 100));
    }

    // A half of barrel roll
    public void turn()
    {
        planeAnimator.turn();
        state = PlaneState.turning;
        hideParts(true);
        hideBombs();
    }

    //Place a plane upside down
    public void turnOver()
    {
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

    public void throwBomb()
    {
        if (plane.Bombs.Count > 0)
        {
            GameObject bomb = plane.Bombs.Dequeue();
            //Disconnecting bomb
            bomb.GetComponent<FixedJoint2D>().breakForce = 0;
        }
    }

    //Landing gear
    public void switchGear()
    {
        if (planeRB.velocity.magnitude > plane.Model.GearSwitchVelocity)
        {
            gearCtrl.switchGear(!gearCtrl.IsGearUp);
        }
    }

    public void switchGear(bool on)
    {
        if (planeRB.velocity.magnitude > plane.Model.GearSwitchVelocity)
        {
            gearCtrl.switchGear(!on);
        }
    }

    public void shoot()
    {
        if (!shootingTimer.check())
        {
            return;
        }
        shootingTimer.reset();

        float accuracy = (Random.value - 0.5f) * plane.Model.ShootingAccuracy;
        float rotation = transform.rotation.eulerAngles.z / 180 * Mathf.PI;
        Vector3 gunPos = new Vector2(-Mathf.Cos(rotation + gunOffsetAngle), -Mathf.Sin(rotation + gunOffsetAngle)) * plane.Model.GunOffset;
        GameObject bullet = Instantiate(GameAssets.Instance.Bullet, gunPos + transform.position, transform.rotation);
        bullet.transform.Rotate(new Vector3(0, 0, accuracy));
        bullet.GetComponent<Rigidbody2D>().velocity = planeRB.velocity;
        plane.Bullets--;
    }

    //Update if plane part breaks
    public void OnJointBreak2D(Joint2D thejoint)
    {
        if (thejoint.connectedBody.gameObject.transform.name == "elevator")
        {
            elevator.IsBroken = true;
        }
    }

    //Creating a bomb GameObject
    public void AddBomb()
    {
        GameObject bmb = GameObject.Instantiate(GameAssets.Instance.Bomb, plane.BombOffset + transform.position, Quaternion.identity);
        FixedJoint2D joint = bmb.GetComponent<FixedJoint2D>();
        joint.connectedAnchor = plane.BombOffset;
        joint.connectedBody = GetComponent<Rigidbody2D>();
        plane.Bombs.Enqueue(bmb);
    }

    //Placing plane flaps upside down (after turning animation)
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

    public void rotate(float angle)
    {
        gameObject.transform.Rotate(0, 0, angle / Mathf.PI * 180);
    }

    public void turnBack()
    {
        turnTimer = 0f;
        hideParts(true);
        state = PlaneState.turningBack;
        float velocityCoefficient = Mathf.Min(1 / planeRB.velocity.magnitude * 10, plane.Model.MaxVelocityCoef);
        float velocityAngle = MathUtils.Vector2ToAngle(planeRB.velocity);
        float angleCoefficient = Mathf.Max(-Mathf.Cos(velocityAngle + Mathf.PI / 2) + 1, plane.Model.MinAngleCoef);
        turnSpeedMultiplier = velocityCoefficient * angleCoefficient;
        planeAnimator.turnBack(turnSpeedMultiplier);
        planeRB.isKinematic = true;
        planeRB.freezeRotation = true;
        lastVelocity = new Vector3(planeRB.velocity.x, planeRB.velocity.y, 0);
        switchAerofoilActive();
        hideBombs();
        float headingAngle = transform.rotation.eulerAngles.z / 180 * Mathf.PI + Mathf.PI;
        rotate(velocityAngle - headingAngle);
    }

    //Turn off aerofoil physics(during animation)
    public void switchAerofoilActive()
    {
        foreach (Aerofoil aerofoil in aerofoilList)
        {
            aerofoil.enabled = !aerofoil.enabled;
        }
    }

    //Hide bomb textures(during animation)
    public void hideBombs()
    {
        foreach (GameObject bomb in plane.Bombs)
        {
            bomb.GetComponent<SpriteRenderer>().enabled = !bomb.GetComponent<SpriteRenderer>().enabled;
        }
    }

    //If plane collides something(during animation)
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == PlaneState.turningBack && collision.gameObject.layer != 8)
        {
            breakPlane();
        }
        if (collision.gameObject.tag == "bullet" && collision.relativeVelocity.magnitude > 5f)
        {
            plane.Damage();
        }
        if (plane.HP < 1)
        {
            totalBreakPlane();
        }
    }

    //If plane collides a trigger(water, etc)(during animation)
    public void OnTriggerEnter2D()
    {
        if (state == PlaneState.turningBack)
        {
            breakPlane();
        }
    }

    //Stop plane animation, behaviour, etc
    void breakPlane()
    {
        planeRB.isKinematic = planeRB.freezeRotation = false;
        planeAnimator.StopAnimation();
        isBroken = true;
        foreach (Joint2D joint in GetComponents<Joint2D>())
        {
            joint.breakForce = 0;
        }
        if (plane.IsPlayer)
        {
            Game.Instance.gameOver("plane is broken");
        }
    }

    void totalBreakPlane()
    {
        breakPlane();
        Joint2D[] joints = GetComponents<Joint2D>();
        //Disconnect all plane parts
        foreach (Joint2D joint in joints)
        {
            joint.breakForce = 0;
        }
    }

    //Turn plane without animation
    void forceTurnBack()
    {
        rotate();
        turnOver();
    }

    void switchFlaps()
    {
        flaps = !flaps;
        if (flaps)
        {
            flapMotor.motorSpeed = plane.Model.FlapMotorSpeed;
        }
        else
        {
            flapMotor.motorSpeed = -plane.Model.FlapMotorSpeed;
        }
    }

    public void switchFlaps(bool on)
    {
        flaps = on;
        if (flaps)
        {
            flapMotor.motorSpeed = plane.Model.FlapMotorSpeed;
        }
        else
        {
            flapMotor.motorSpeed = -plane.Model.FlapMotorSpeed;
        }
    }

    public void updateFlaps()
    {
        if (flaps)
        {
            flapMotor.motorSpeed = plane.Model.FlapMotorSpeed;
        }
        else
        {
            flapMotor.motorSpeed = -plane.Model.FlapMotorSpeed;
        }
    }

    void switchBrakes()
    {
        brakes = !brakes;
        updateBrakes();
    }

    public void switchBrakes(bool on)
    {
        brakes = on;
        updateBrakes();
    }

    public void updateBrakes()
    {
        gearCtrl.switchBrakes(brakes);
    }

    void reloadBombs()
    {
        for (int a = 0; a < plane.BombCount; a++)
        {
            AddBomb();
        }
    }

    void hideParts(bool hide)
    {
        foreach (PlanePart part in plane.Parts)
        {
            if (part.PartName != "propeller")
            {
                part.hide(hide);
            }
        }
    }

    public void onTurnExit()
    {
        state = PlaneState.common;
        turnOver();
        hideParts(false);
        hideBombs();
    }

    public void onTurnBackMiddle()
    {
        forceTurnBack();
    }

    public void onTurnBackExit()
    {
        state = PlaneState.common;
        //turnOver();
        hideParts(false);
        planeRB.isKinematic = false;
        planeRB.freezeRotation = false;
        switchAerofoilActive();
        hideBombs();
    }
    public void hidePropeller(bool hide)
    {
        propeller.hide(hide);
    }
}
