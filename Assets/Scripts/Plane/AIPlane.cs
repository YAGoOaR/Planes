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
    const float GEAR_UP_ALTITUDE = 25;
    const float VELOCITY_SENSIVITY = 30;
    const float VELOCITY_COEFFICIENT = 2;
    const float GEAR_DOWN_DISTANCE = 200;
    const float BRAKE_DISTANCE = GEAR_DOWN_DISTANCE * 2 / 3;
    const float MAX_TARGET_ANGLE = 15;
    public bool hostile = true;
    public bool enemyDestroyed = false;
    public float targetAltitude = 40;
    [HideInInspector]
    public float targetPosition = -30;
    [HideInInspector]
    public float targetSpeed = DEFAULT_SPEED;
    [HideInInspector]
    public float targetAngle = -15;
    AeroPlane plane;
    AIState state = AIState.takingOff;
    GameObject enemy;
    PlaneBehaviour planeBehaviour;
    Timers.CooldownTimer turnCooldown;

    Vector3 enemyPos;
    Vector2 velocity;
    float altitude;
    float rotation;
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
        climbing,
        reachingTarget,
    }

    //Called once when this object initializes
    void Start()
    {
        //Timers.interval(1, () => { Debug.Log(rotation); });
        plane = GetComponent<AeroPlane>();
        turnCooldown = new Timers.CooldownTimer(3);
        planeBehaviour = GetComponent<PlaneBehaviour>();
        if (hostile)
        {
            enemy = GameHandler.instance.player;
        }
        planeBehaviour.isPlayer = false;
    }

    //Called once per frame
    //Autopilot realisation
    void Update()
    {
        //If plane has to do nothing
        if (state == AIState.idle) return;
        updateVariables();
        if (state == AIState.attacking) attack();
        else reachTarget();
        if (state == AIState.takingOff) takeOff();
        if (state == AIState.climbing)
        {
            climb();
            if (altitude > 30)
            {
                state = AIState.bombingTarget;
            }
        }
        if (state == AIState.bombingTarget) bombTarget();
        if (state == AIState.landing && !planeBehaviour.isTurningBack) land();
        if ((enemyPos - transform.position).magnitude < 240 && state != AIState.climbing && state != AIState.takingOff && plane.bullets > 0 && !enemyDestroyed && deltaPosition > 250)
        {
            planeBehaviour.throttle = 100;
            state = AIState.attacking;
        }
    }

    void updateVariables()
    {
        enemyPos = enemy.transform.position;
        deltaSpeed = targetSpeed - Mathf.Abs(velocity.magnitude);
        velocity = GetComponent<Rigidbody2D>().velocity;
        altitude = transform.position.y;
        rotation = transform.rotation.eulerAngles.z;
        position = transform.position.x;
        deltaPosition = targetPosition - position;
        deltaAltitude = targetAltitude - altitude;
        distance = Mathf.Abs(deltaPosition);
    }

    void reachTarget()
    {
        //Turn to target
        if (turnCooldown.check() && velocity.magnitude > MIN_TURN_VELOCITY && (!CheckRotationBounds(10, 170)))
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

        //Smooth reaching altitude and lining when speed is too low
        targetAngle = -MathUtils.clamp(targetAltitude - altitude, MAX_TARGET_ANGLE) * heading * MathUtils.clamp(velocity.magnitude / VELOCITY_SENSIVITY, VELOCITY_COEFFICIENT);
        float deltaAngle = toRadian(targetAngle - rotation);
        //Plane heading: 1 when flying left (model is normal) and -1 when flying right (model is mirrored upside down and turned 180 degrees)
        heading = 1;
        if (planeBehaviour.upsideDown)
        {
            heading = -1;
            deltaAngle += Mathf.PI;
        }
        //Reaching target angle
        setPitch(deltaAngle);
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
            state = AIState.reachingTarget;
            Timers.timeout(2, () =>
            {
                targetPosition = BASE_POSITION;
                deltaPosition = targetPosition - position;
                distance = Mathf.Abs(deltaPosition);
                state = AIState.landing;
            });
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
            if (planeBehaviour.gearCtrl.isGearUp)
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
                if (velocity.magnitude < 20)
                {
                    state = AIState.idle;
                }
            }
        }
    }
    void attack()
    {
        float distance = Mathf.Abs((enemyPos - transform.position).magnitude);
        turnOver();
        if (altitude + 4 * velocity.y < 0 || (altitude < 30 && CheckRotationBounds(60, 120)) || velocity.magnitude < 15) state = AIState.climbing;
        targetAngle = MathUtils.Vector2ToAngle(enemyPos - transform.position) + Mathf.PI;
        float deltaAngle = targetAngle - toRadian(rotation);
        if (Mathf.Cos(deltaAngle) < -0.1f)
        {
            planeBehaviour.turnBack();
        }
        setPitch(deltaAngle);
        if (Mathf.Abs(deltaAngle) < 0.1f && distance < 100)
        {
            planeBehaviour.shoot();
        }

    }
    void turnOver()
    {
        if (turnCooldown.check())
        {
            if (CheckRotationBounds(300, 60))
            {
                if (planeBehaviour.upsideDown)
                {
                    planeBehaviour.turn();
                    turnCooldown.reset();
                    return;
                }
            }
            if (CheckRotationBounds(240, 120))
            {
                if (!planeBehaviour.upsideDown)
                {
                    planeBehaviour.turn();
                    turnCooldown.reset();
                    return;
                }
            }
        }
    }

    void climb()
    {
        planeBehaviour.throttle = 100;
        float heading = Mathf.Sign(Mathf.Cos(toRadian(rotation)));
        targetAngle = 270 + heading * 60;
        float deltaAngle = toRadian(targetAngle - rotation);
        setPitch(deltaAngle);
    }

    void setPitch(float deltaAngle)
    {
        float pitch = -Mathf.Sin(deltaAngle) * SENSITIVITY;
        planeBehaviour.pitch = MathUtils.clamp(pitch, 1);
    }

    float toRadian(float angle)
    {
        return angle / 180 * Mathf.PI;
    }

    bool CheckRotationBounds(float a, float b)
    {
        if (Mathf.Abs(b - a) < 180)
        {
            return rotation < Mathf.Max(a, b) && rotation > Mathf.Min(a, b);
        }
        else return rotation > Mathf.Max(a, b) || rotation < Mathf.Min(a, b);
    }
}
