using System.Collections.Generic;
using UnityEngine;
//A script that controls a plane
public class PlaneBehaviour : MonoBehaviour
{
    [SerializeField] float maxpitch = 1;
    [SerializeField] float jointForce = 300f;
    [SerializeField] float flapMotorSpeed = 20;
    [SerializeField] float flapMaxTorque = 1000;
    [SerializeField] float gearSwitchVelocity = 5;
    [SerializeField] float velocityOffset = 0.3f;
    [SerializeField] float turnBackDrag = 0.1f;
    [SerializeField] float turnBackTime = 2;
    [SerializeField] bool startInOtherHeading = false;

    [SerializeField] float pitchRange = 15;

    public bool Flaps { get => flaps; }
    public bool Brakes { get => brakes; }

    GearController gearCtrl;
    float pitch;
    bool upsideDown;
    int throttle;
    bool flaps;
    bool brakes;
    float flapAngle = 30;
    PlanePart elevator;
    PlanePart gear;
    PlanePart propeller;
    PlanePart flap;
    Rigidbody2D planeRB;
    JointMotor2D hingemotor;
    JointMotor2D flapMotor;
    HingeJoint2D flapJoint;
    HingeJoint2D elevatorHinge;
    PropellerMotor propellerMotor;
    Timers.CooldownTimer throttleTimer;


    Aerofoil[] aerofoilList;
    PlaneAnimator planeAnimator;
    PlaneState state;
    Vector3 lastVelocity;
    float turnTimer = 0;
    float turnSpeedMultiplier = 1;
    Health health;
    GameHandler gameHandler;
    PlanePartManager partManager;
    PlaneComponentDisabler disabler;

    public PlaneState State { get; }

    public bool GearUp { get => gearCtrl.IsGearUp || gear.IsBroken; }

    public bool NormalState { get => checkNormalState(); }

    public enum PlaneState
    {
        common,
        turning,
        stalling,
        turningBack
    }

    public bool UpsideDown { get => upsideDown; }

    //Called after this object initialization
    void Start()
    {
        defineVariables();
        defineComponents();
        planeSetup();
    }

    private void Update()
    {
        if (health.Dead) return;
        planePhysics();
    }

    //Called each frame
    void FixedUpdate()
    {
        if (health.Dead) return;
        updateControls();
    }

    void defineComponents()
    {
        partManager = GetComponent<PlanePartManager>();
        elevator = partManager.getPart("elevator");
        gear = partManager.getPart("gear");
        propeller = partManager.getPart("propeller");
        flap = partManager.getPart("flap");

        planeAnimator = GetComponent<PlaneAnimator>();
        planeRB = GetComponent<Rigidbody2D>();
        elevatorHinge = elevator.GetComponent<HingeJoint2D>();
        aerofoilList = transform.parent.GetComponentsInChildren<Aerofoil>();
        gearCtrl = gear.GetComponent<GearController>();
        propellerMotor = propeller.GetComponent<PropellerMotor>();
        flapJoint = flap.GetComponent<HingeJoint2D>();
        flapMotor = new JointMotor2D();
        hingemotor = new JointMotor2D();
        health = GetComponent<Health>();
        disabler = GetComponent<PlaneComponentDisabler>();
    }

    void defineVariables()
    {
        gameHandler = GameHandler.Instance;
        throttleTimer = new Timers.CooldownTimer(0.01f);


        state = PlaneState.common;
    }

    void planeSetup()
    {
        health.OnDeath.AddListener(totalBreakPlane);

        flapAngle = flapJoint.limits.min;
        hingemotor.maxMotorTorque = 100;
        flapMotor.maxMotorTorque = flapMaxTorque;
        flapMotor.motorSpeed = -flapMotorSpeed;

        if (startInOtherHeading)
        {
            turnOver();
        }
    }


    void planePhysics()
    {
        if (state == PlaneState.turningBack)
        {
            turnTimer += Time.deltaTime * turnSpeedMultiplier;
            //planeRB.velocity = lastVelocity * Mathf.Cos(Mathf.Max(Mathf.PI * turnTimer * (1 - turnTimer * turnBackDrag) + velocityOffset, 0));
            planeRB.velocity = lastVelocity * Mathf.Cos(Mathf.Max(Mathf.PI * turnTimer * (1 - turnTimer * turnBackDrag + velocityOffset) / turnBackTime, 0));
            //Debug.Log(turnTimer);
        }

    }

    void UpdatePitch()
    {
        float angleDiff = -elevatorHinge.jointAngle - pitch;


        float P = 1;
        float D = 0;

        hingemotor.motorSpeed = P * angleDiff * jointForce + D * elevatorHinge.jointSpeed * D;
        //Debug.Log(hingemotor.motorSpeed);
        elevatorHinge.motor = hingemotor;
    }

    void updateControls()
    {
        propellerMotor.Throttle = throttle;

        //Plane pitch
        if (!elevator.IsBroken)
        {
            UpdatePitch();
        }

        //Changing controls if animation puts plane upside down
        if (upsideDown)
        {
            JointMotor2D reverse = new JointMotor2D();
            reverse.motorSpeed = -flapMotor.motorSpeed;
            reverse.maxMotorTorque = flapMaxTorque;
            flapJoint.motor = reverse;
        }
        else
        {
            if (!flap.IsBroken) flapJoint.motor = flapMotor;
        }
    }

    // A half of barrel roll
    public void turn()
    {
        if (!checkNormalState() || !(planeRB.velocity.magnitude > 10f && (gearCtrl.IsGearUp || gear.IsBroken))) return;
        planeAnimator.turn();
        state = PlaneState.turning;
        disabler.setPartsActive(false);
    }

    //Place the plane upside down
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

        foreach (PlanePart part in partManager.Parts)
        {
            if (part.PartName != "propeller")
            {
                part.transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
            }
        }

        //Inverting flaps
        turnFlaps();
        UpdatePartsPos();
    }

    //Landing gear
    public void switchGear()
    {
        if (!checkNormalState()) return;
        if (planeRB.velocity.magnitude > gearSwitchVelocity)
        {
            gearCtrl.switchGear(!gearCtrl.IsGearUp);
        }
    }

    public void setGear(bool on)
    {
        if (planeRB.velocity.magnitude > gearSwitchVelocity)
        {
            gearCtrl.switchGear(!on);
        }
    }



    //Update if plane part breaks
    public void OnJointBreak2D(Joint2D thejoint)
    {
        if (thejoint.connectedBody.gameObject.transform.name == "elevator")
        {
            elevator.IsBroken = true;
        }
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
        if (
            !checkNormalState() ||
            !(planeRB.velocity.magnitude > 10f && (gearCtrl.IsGearUp || gear.IsBroken)) ||
            !(Mathf.Cos(planeRB.rotation * Mathf.Deg2Rad - MathUtils.Vector2ToAngle(planeRB.velocity)) < -0.95f)
        ) { return; }

        turnTimer = 0f;
        state = PlaneState.turningBack;
        //float velocityCoefficient = Mathf.Min(1 / planeRB.velocity.magnitude * 10, maxVelocityCoef);
        float velocityAngle = MathUtils.Vector2ToAngle(planeRB.velocity);
        //float angleCoefficient = Mathf.Max(-Mathf.Cos(velocityAngle + Mathf.PI / 2) + 1, minAngleCoef);

        //turnSpeedMultiplier = velocityCoefficient * angleCoefficient;

        planeAnimator.turnBack(turnBackTime);
        planeRB.isKinematic = true;
        planeRB.freezeRotation = true;
        lastVelocity = new Vector3(planeRB.velocity.x, planeRB.velocity.y, 0);
        switchAerofoilActive();

        float headingAngle = transform.rotation.eulerAngles.z / 180 * Mathf.PI + Mathf.PI;
        rotate(velocityAngle - headingAngle);

        disabler.setPartsActive(false);
    }

    //Turn off aerofoil physics(during animation)
    public void switchAerofoilActive()
    {
        foreach (Aerofoil aerofoil in aerofoilList)
        {
            aerofoil.enabled = !aerofoil.enabled;
        }
    }

    //If plane collides something(during animation)
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == PlaneState.turningBack && collision.gameObject.layer != 8 && collision.gameObject.layer != 10)
        {
            health.Kill();
        }
    }

    //If plane collides a trigger(water, etc)(during animation)
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (state == PlaneState.turningBack)
        {
            health.Kill();
        }
    }

    //Stop plane animation, behaviour, etc
    void breakPlane()
    {
        planeRB.isKinematic = planeRB.freezeRotation = false;
        planeAnimator.StopAnimation();
    }

    void totalBreakPlane()
    {
        breakPlane();

        foreach (PlanePart part in partManager.Parts)
        {
            part.Break();
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
        updateFlaps();
    }

    public void setFlaps(bool on)
    {
        flaps = on;
        updateFlaps();
    }

    public void updateFlaps()
    {
        flapMotor.motorSpeed = flaps ? flapMotorSpeed : -flapMotorSpeed;
    }

    void switchBrakes()
    {
        brakes = !brakes;
        updateBrakes();
    }

    public bool checkNormalState()
    {
        return state != PlaneState.turning && state != PlaneState.turningBack && !health.Dead;
    }

    public void setBrakes(bool on)
    {
        if (!checkNormalState()) return;
        brakes = on;
        updateBrakes();
    }

    public void updateBrakes()
    {
        gearCtrl.switchBrakes(brakes);
    }
    void UpdatePartsPos()
    {
        foreach (PlanePart part in partManager.Parts)
        {
            part.transform.up = transform.up;
            AnchoredJoint2D joint;
            if(!part.TryGetComponent(out joint)) return;
            part.transform.position = transform.position + Vector3.Scale(transform.rotation * joint.connectedAnchor - transform.rotation * joint.anchor, transform.localScale);
            Rigidbody2D partrb = part.GetComponent<Rigidbody2D>();
            partrb.velocity = planeRB.velocity;
            partrb.angularVelocity = planeRB.angularVelocity;
        }
    }

    public void OnTurnExit()
    {
        state = PlaneState.common;
        turnOver();
        disabler.setPartsActive(true);
    }

    public void OnTurnBackMiddle()
    {
        forceTurnBack();
    }

    public void OnTurnBackExit()
    {
        state = PlaneState.common;
        planeRB.isKinematic = false;
        planeRB.freezeRotation = false;
        switchAerofoilActive();
        disabler.setPartsActive(true);
    }

    public void HidePropeller(bool hide)
    {
        propeller.hide(hide);
    }

    public void AdjustThrottle(int diff)
    {
        if (health.Dead) return;
        if (throttleTimer.check())
        {
            throttle += diff;
            throttle = Mathf.Clamp(throttle, 0, 100);
            throttleTimer.reset();
        }
    }

    public void SetThrottle(int throttle)
    {
        if (health.Dead) return;
        this.throttle = throttle;
    }

    public void SetPitch(float value)
    {
        if (health.Dead) return;
        pitch = Mathf.Clamp(value, -1, 1) * pitchRange * maxpitch;
    }
}
