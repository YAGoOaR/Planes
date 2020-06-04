using UnityEngine;

[RequireComponent(typeof(PlaneBehaviour))]
[RequireComponent(typeof(AeroPlane))]
public class AIPlane : MonoBehaviour
{
    //Enemy plane autopilot v0.1. Enemy can take off, bomb target and then land on base.
    const float BASE_POSITION = -860;
    const float LANDING_ALTITUDE = 21.2f;
    const float DEFAULT_SPEED = 40;
    const float DEFAULT_ALTITUDE = 40;
    const float SENSITIVITY = 5;
    const float TRIM = 0;
    const float BOMB_THROW_ACCURACY = 0.5f;
    const float BOMB_OFFSET = 18;
    const float MIN_TURN_VELOCITY = 30;
    const float SPEED_ACCURACY = 2;
    const float GEAR_UP_ALTITUDE = 30;
    const float VELOCITY_SENSIVITY = 30;
    const float VELOCITY_COEFFICIENT = 2;
    const float GEAR_DOWN_DISTANCE = 200;
    const float BRAKE_DISTANCE = GEAR_DOWN_DISTANCE * 2 / 3;
    const float MAX_TARGET_ANGLE = 15;
    [HideInInspector]
    public float targetAltitude = 40;
    [HideInInspector]
    public float targetPosition = 0;
    [HideInInspector]
    public float targetSpeed = DEFAULT_SPEED;
    [HideInInspector]
    public int startThrottle = 0;
    float targetAngle = -15;
    AeroPlane plane;
    public AIState state = AIState.takingOff;
    PlaneBehaviour planeBehaviour;
    Timers.CooldownTimer turnCooldown;
    Timers.CooldownTimer waitBeforeReturn;

    float velocity;
    float altitude;
    float rotation;
    float deltaAngle;
    float position;
    float deltaPosition;
    float deltaAltitude;
    float distance;
    float deltaSpeed;
    int heading;

    public enum AIState
    {
        idle,
        takingOff,
        landing,
        bombingTarget,
        attacking,
    }

    //Called once when this object initializes
    void Start()
    {
        plane = GetComponent<AeroPlane>();
        turnCooldown = new Timers.CooldownTimer(10);
        waitBeforeReturn = new Timers.CooldownTimer(1, true);
        planeBehaviour = GetComponent<PlaneBehaviour>();
        planeBehaviour.isPlayer = false;
        planeBehaviour.throttle = startThrottle;
    }

    //Called once per frame
    //Autopilot realisation
    void Update()
    {
        //If plane has to do nothing
        if (state == AIState.idle) return;
        updateVariables();
        reachTarget();
        if (state == AIState.takingOff) takeOff();
        if (state == AIState.bombingTarget) bombTarget();
        if (state == AIState.landing && !planeBehaviour.isTurningBack) land();
    }

    void updateVariables()
    {
        deltaSpeed = targetSpeed - Mathf.Abs(velocity);
        velocity = GetComponent<Rigidbody2D>().velocity.magnitude;
        altitude = transform.position.y;
        rotation = transform.rotation.eulerAngles.z;
        deltaAngle = targetAngle - rotation;
        position = transform.position.x;
        deltaPosition = targetPosition - position;
        deltaAltitude = targetAltitude - altitude;
        distance = Mathf.Abs(deltaPosition);
        //Plane heading: 1 when flying left (model is normal) and -1 when flying right (model is mirrored upside down and turned 180 degrees)
        heading = 1;
        if (planeBehaviour.upsideDown)
        {
            heading = -1;
            deltaAngle += 180;
        }
    }

    void reachTarget()
    {
        //Turn to target
        if (turnCooldown.check() && velocity > MIN_TURN_VELOCITY)
        {
            if (!planeBehaviour.upsideDown && position < targetPosition)
            {
                planeBehaviour.turnBack();
                turnCooldown.reset();
            }
            if (planeBehaviour.upsideDown && position > targetPosition)
            {
                planeBehaviour.turnBack();
                turnCooldown.reset();
            }
        }

        if (!planeBehaviour.isTurningBack)
        {
            //Reaching target speed
            if (deltaSpeed > SPEED_ACCURACY) planeBehaviour.throttle = 100;
            else if (deltaSpeed < -SPEED_ACCURACY) planeBehaviour.throttle = 0;
            else
            {
                planeBehaviour.throttle = 50;
            }
        }

        //Reaching target angle
        float pitch = -Mathf.Sin(deltaAngle / 180 * Mathf.PI) * SENSITIVITY + heading * TRIM;
        planeBehaviour.pitch = MathUtils.clamp(pitch, 1);
        //Smooth reaching altitude and lining when speed is too low
        targetAngle = -MathUtils.clamp(targetAltitude - altitude, MAX_TARGET_ANGLE) * heading * MathUtils.clamp(velocity / VELOCITY_SENSIVITY, VELOCITY_COEFFICIENT);
    }

    void takeOff()
    {
        //Gear up when reached altitude
        if (!planeBehaviour.gearCtrl.isGearUp && altitude > GEAR_UP_ALTITUDE)
        {
            planeBehaviour.switchGear();
            state = AIState.bombingTarget;
        }
    }

    void bombTarget()
    {
        //My ballistics calculator
        float timeToCollision = Mathf.Sqrt(altitude / 9.8f);
        float attackPosition = Mathf.Abs(timeToCollision * GetComponent<Rigidbody2D>().velocity.x) + BOMB_OFFSET;
        float distanceToAttack = Mathf.Abs(attackPosition - Mathf.Abs(deltaPosition));
        if (distanceToAttack < BOMB_THROW_ACCURACY)
        {
            planeBehaviour.throwBomb();
            waitBeforeReturn.reset();
        }
        //Return to base when out of bombs
        if (plane.bombs.Count == 0 && waitBeforeReturn.check())
        {
            targetPosition = BASE_POSITION;
            waitBeforeReturn.reset();
            deltaPosition = targetPosition - position;
            distance = Mathf.Abs(deltaPosition);
            state = AIState.landing;
        }
    }

    //My landing autopilot
    void land()
    {
        //Smooth altitude lowering
        targetAltitude = MathUtils.clamp(Mathf.Abs(deltaPosition / 1000), 1) * (DEFAULT_ALTITUDE - LANDING_ALTITUDE) + LANDING_ALTITUDE;
        //Reach landing speed if not missed altitude, otherwise reach normal
        if (deltaAltitude < 2)
        {
            targetSpeed = MathUtils.clamp(Mathf.Abs(deltaPosition / 500), 1) * DEFAULT_SPEED;
        }
        else
        {
            targetSpeed = DEFAULT_SPEED;
        }
        //When close to base
        if (distance < GEAR_DOWN_DISTANCE)
        {
            //Gear down if it is still up
            if (planeBehaviour.gearCtrl.isGearUp && !waitBeforeReturn.check())
            {
                planeBehaviour.switchGear(true);
                planeBehaviour.switchBrakes(true);
            }
            //Stop
            if (distance < BRAKE_DISTANCE)
            {
                planeBehaviour.switchFlaps(true);
                targetSpeed = 0;
                //Now do nothing
                if (velocity < 20)
                {
                    state = AIState.idle;
                }
            }
        }
    }
}
