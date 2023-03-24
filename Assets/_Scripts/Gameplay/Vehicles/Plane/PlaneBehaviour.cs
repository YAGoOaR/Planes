using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
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
    Health health;
    PlanePartManager partManager;
    PlaneComponentDisabler disabler;

    public PlaneState State { get; }

    public bool GearUp { get => gearCtrl.IsGearUp || gear.IsBroken; }

    public bool NormalState { get => CheckNormalState(); }

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
        DefineVariables();
        DefineComponents();
        PlaneSetup();
    }

    private void Update()
    {
        if (health.Dead) return;
        PlanePhysics();
    }

    //Called each frame
    void FixedUpdate()
    {
        if (health.Dead) return;
        UpdateControls();
    }

    void DefineComponents()
    {
        partManager = GetComponent<PlanePartManager>();
        elevator = partManager.GetPart(PlanePartManager.PartType.elevator);
        gear = partManager.GetPart(PlanePartManager.PartType.gear);
        propeller = partManager.GetPart(PlanePartManager.PartType.engine);
        flap = partManager.GetPart(PlanePartManager.PartType.flap);

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

    void DefineVariables()
    {
        throttleTimer = new Timers.CooldownTimer(0.01f);


        state = PlaneState.common;
    }

    void PlaneSetup()
    {
        health.OnDeath.AddListener(TotalBreakPlane);

        flapAngle = flapJoint.limits.min;
        hingemotor.maxMotorTorque = 100;
        flapMotor.maxMotorTorque = flapMaxTorque;
        flapMotor.motorSpeed = -flapMotorSpeed;

        if (startInOtherHeading) TurnOver();
    }


    void PlanePhysics()
    {
        if (state == PlaneState.turningBack)
        {
            turnTimer += Time.deltaTime;
            planeRB.velocity = lastVelocity * Mathf.Cos(Mathf.Max(Mathf.PI * turnTimer * (1 - turnTimer * turnBackDrag + velocityOffset) / turnBackTime, 0));
        }

    }

    void UpdatePitch()
    {
        float angleDiff = -elevatorHinge.jointAngle - pitch;


        float P = 1;
        float D = 0;

        hingemotor.motorSpeed = P * angleDiff * jointForce + D * elevatorHinge.jointSpeed * D;
        elevatorHinge.motor = hingemotor;
    }

    void UpdateControls()
    {
        propellerMotor.Throttle = throttle;

        if (!elevator.IsBroken) UpdatePitch();

        if (upsideDown)
        {
            JointMotor2D reverse = new JointMotor2D
            {
                motorSpeed = -flapMotor.motorSpeed,
                maxMotorTorque = flapMaxTorque
            };
            flapJoint.motor = reverse;
        }
        else if (!flap.IsBroken) flapJoint.motor = flapMotor;
    }

    // A half of barrel roll
    public void Turn()
    {
        if (!CheckNormalState() || !(planeRB.velocity.magnitude > 10f && (gearCtrl.IsGearUp || gear.IsBroken))) return;
        planeAnimator.Turn();
        state = PlaneState.turning;
        disabler.SetPartsActive(false);
    }

    //Place the plane upside down
    public void TurnOver()
    {
        Vector3 scale = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
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

        TurnFlaps();
        UpdatePartsPos();
    }

    public void SwitchGear()
    {
        if (!CheckNormalState()) return;
        if (planeRB.velocity.magnitude > gearSwitchVelocity)
        {
            gearCtrl.SwitchGear(!gearCtrl.IsGearUp);
        }
    }

    public void SetGear(bool on)
    {
        if (planeRB.velocity.magnitude > gearSwitchVelocity)
        {
            gearCtrl.SwitchGear(!on);
        }
    }

    public void OnJointBreak2D(Joint2D thejoint)
    {
        if (thejoint.connectedBody.gameObject.transform.name == "elevator")
        {
            elevator.IsBroken = true;
        }
    }

    public void TurnFlaps()
    {
        flapJoint.limits = new JointAngleLimits2D
        {
            max = 0,
            min = upsideDown ? -flapAngle : flapAngle,
        };
    }

    public void Rotate()
    {
        gameObject.transform.Rotate(0, 0, 180);
    }

    public void Rotate(float angle)
    {
        gameObject.transform.Rotate(0, 0, angle / Mathf.PI * 180);
    }

    public void TurnBack()
    {
        if (
            !CheckNormalState() ||
            !(planeRB.velocity.magnitude > 10f && (gearCtrl.IsGearUp || gear.IsBroken)) ||
            !(Mathf.Cos(planeRB.rotation * Mathf.Deg2Rad - Vector2.SignedAngle(Vector2.right, planeRB.velocity) * Mathf.Deg2Rad) < -0.95f)
        ) { return; }

        turnTimer = 0f;
        state = PlaneState.turningBack;
        float velocityAngle = Vector2.SignedAngle(Vector2.right, planeRB.velocity) * Mathf.Deg2Rad;

        planeAnimator.TurnBack(turnBackTime);
        planeRB.isKinematic = true;
        planeRB.freezeRotation = true;
        lastVelocity = new Vector3(planeRB.velocity.x, planeRB.velocity.y, 0);
        SwitchAerofoilActive();

        float headingAngle = transform.rotation.eulerAngles.z / 180 * Mathf.PI + Mathf.PI;
        Rotate(velocityAngle - headingAngle);

        disabler.SetPartsActive(false);
    }

    public void SwitchAerofoilActive()
    {
        foreach (Aerofoil aerofoil in aerofoilList)
        {
            aerofoil.enabled = !aerofoil.enabled;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == PlaneState.turningBack && collision.gameObject.layer == 9)
        {
            health.Kill();
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (state == PlaneState.turningBack)
        {
            health.Kill();
        }
    }

    void BreakPlane()
    {
        planeRB.isKinematic = planeRB.freezeRotation = false;
        planeAnimator.StopAnimation();
    }

    void TotalBreakPlane()
    {
        BreakPlane();

        foreach (PlanePart part in partManager.Parts)
        {
            part.Break();
        }
    }

    void ForceTurnBack()
    {
        Rotate();
        TurnOver();
    }

    public void SetFlaps(bool on)
    {
        flaps = on;
        UpdateFlaps();
    }

    public void UpdateFlaps()
    {
        flapMotor.motorSpeed = flaps ? flapMotorSpeed : -flapMotorSpeed;
    }

    public void SwitchBrakes()
    {
        brakes = !brakes;
        UpdateBrakes();
    }

    public bool CheckNormalState()
    {
        return state != PlaneState.turning && state != PlaneState.turningBack && !health.Dead;
    }

    public void SetBrakes(bool on)
    {
        if (!CheckNormalState()) return;
        brakes = on;
        UpdateBrakes();
    }

    public void UpdateBrakes()
    {
        gearCtrl.SwitchBrakes(brakes);
    }

    public void UpdatePartsPos()
    {
        foreach (PlanePart part in partManager.Parts)
        {
            part.transform.up = transform.up;
            if (!part.TryGetComponent(out AnchoredJoint2D joint)) return;
            part.transform.position = transform.position + Vector3.Scale(transform.rotation * joint.connectedAnchor - transform.rotation * joint.anchor, transform.localScale);
            Rigidbody2D partrb = part.GetComponent<Rigidbody2D>();
            partrb.velocity = planeRB.velocity;
            partrb.angularVelocity = planeRB.angularVelocity;
        }
    }

    public void OnTurnExit()
    {
        state = PlaneState.common;
        TurnOver();
        disabler.SetPartsActive(true);
    }

    public void OnTurnBackMiddle()
    {
        ForceTurnBack();
    }

    public void OnTurnBackExit()
    {
        state = PlaneState.common;
        planeRB.isKinematic = false;
        planeRB.freezeRotation = false;
        SwitchAerofoilActive();
        disabler.SetPartsActive(true);
    }

    public void HidePropeller(bool hide)
    {
        propeller.Hide(hide);
    }

    public void AdjustThrottle(int diff)
    {
        if (health.Dead) return;
        if (throttleTimer.Check())
        {
            throttle += diff;
            throttle = Mathf.Clamp(throttle, 0, 100);
            throttleTimer.Reset();
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
