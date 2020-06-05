using UnityEngine;

[RequireComponent(typeof(PlaneBehaviour))]
[RequireComponent(typeof(AeroPlane))]
public class AIPlane : MonoBehaviour
{
    //Enemy plane autopilot v0.2. Enemy can take off, bomb target, attack player and then land on base.
    const float PI = 180;
    const float GRAVITY = 9.8f;
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
    const float MAX_VELOCITY_COEF = 2;
    const float GEAR_DOWN_DISTANCE = 200;
    const float BRAKE_DISTANCE = GEAR_DOWN_DISTANCE * 2 / 3;
    const float MAX_TARGET_ANGLE = 15;
    const float ATTACK_DISTANCE = 240;
    const float BOMBING_DISTANCE = 250;
    const float SHOOTING_ACCURACY = 0.1f;
    const float SHOOTING_DISTANCE = 100;
    const float TURN_BACK_THRESHOLD = -0.1f;
    const float TIME_TO_COLLISION = 4;
    const float SAFE_ALTITUDE = 30;
    const float STALL_VELOCITY = 15;
    const float LANDING_SENSITIVITY = 1000;
    const float UP_ANGLE = 270;
    const float SAFE_ANGLE = 60;
    const float DEFAULT_ANGLE = 10;
    const float BREAK_VELOCITY = 20;

    private bool enemyDestroyed;
    public bool EnemyDestroyed
    {
        get { return enemyDestroyed; }
        set { enemyDestroyed = value; }
    }

    [SerializeField]
    float targetPosition = -30;
    [SerializeField]
    float targetAltitude = 40;
    float targetSpeed = DEFAULT_SPEED;
    float targetAngle = -15;
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
    float distanceToEnemy;

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
        plane = GetComponent<AeroPlane>();
        turnCooldown = new Timers.CooldownTimer(3);
        planeBehaviour = GetComponent<PlaneBehaviour>();
        enemy = GameHandler.Instance.Player;
    }

    //Called once per frame
    //Autopilot realisation
    void Update()
    {
        if (state == AIState.idle)
        {
            return;
        }

        updateVariables();

        if (state == AIState.attacking)
        {
            attack();
        }
        else
        {
            reachTarget();
        }

        if (state == AIState.takingOff)
        {
            takeOff();
        }
        else if (state == AIState.climbing)
        {
            climb();
            if (altitude > SAFE_ALTITUDE)
            {
                state = AIState.bombingTarget;
            }
        }
        else
        {
            if (distanceToEnemy < ATTACK_DISTANCE && plane.Bullets > 0 && !enemyDestroyed && deltaPosition > BOMBING_DISTANCE)
            {
                planeBehaviour.Throttle = 100;
                state = AIState.attacking;
            }
        }

        if (state == AIState.bombingTarget)
        {
            bombTarget();
        }
        if (state == AIState.landing && !planeBehaviour.IsTurningBack)
        {
            land();
        }
    }

    void updateVariables()
    {
        enemyPos = enemy.transform.position;
        distanceToEnemy = (enemyPos - transform.position).magnitude;
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
        if (turnCooldown.check() && velocity.magnitude > MIN_TURN_VELOCITY && !CheckRotationBounds(DEFAULT_ANGLE, PI - DEFAULT_ANGLE))
        {
            if (!planeBehaviour.UpsideDown && position < targetPosition)
            {
                planeBehaviour.turnBack();
                turnCooldown.reset();
            }
            if (planeBehaviour.UpsideDown && position > targetPosition)
            {
                planeBehaviour.turnBack();
                turnCooldown.reset();
            }
        }

        if (!planeBehaviour.IsTurningBack)
        {
            //Reaching target speed
            if (deltaSpeed > SPEED_ACCURACY)
            {
                planeBehaviour.Throttle = 100;
            }
            else if (deltaSpeed < -SPEED_ACCURACY)
            {
                planeBehaviour.Throttle = 0;
            }
            else
            {
                planeBehaviour.Throttle = 50;
            }
        }

        float deltaAngle = toRadian(targetAngle - rotation);
        int heading = 1;
        if (planeBehaviour.UpsideDown)
        {
            deltaAngle += Mathf.PI;
            heading = -1;
        }

        //Smooth reaching altitude and lining when speed is too low
        float velocityCoef = MathUtils.clamp(velocity.magnitude / VELOCITY_SENSIVITY, MAX_VELOCITY_COEF);
        float altitudeCoef = -MathUtils.clamp(targetAltitude - altitude, MAX_TARGET_ANGLE);
        targetAngle = altitudeCoef * velocityCoef * heading;

        //Reaching target angle
        setPitch(deltaAngle);
    }

    void takeOff()
    {
        //Gear up when reached altitude
        if (!planeBehaviour.GearCtrl.IsGearUp && altitude > GEAR_UP_ALTITUDE)
        {
            planeBehaviour.switchGear();
            state = AIState.bombingTarget;
        }
    }

    void bombTarget()
    {
        //My ballistics calculator
        float timeToCollision = Mathf.Sqrt(altitude / GRAVITY);
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
        targetAltitude = MathUtils.clamp(Mathf.Abs(deltaPosition / LANDING_SENSITIVITY), 1) * (DEFAULT_ALTITUDE - LANDING_ALTITUDE) + LANDING_ALTITUDE;
        //Reach landing speed if not missed altitude, otherwise reach normal
        if (deltaAltitude < 2)
        {
            targetSpeed = MathUtils.clamp(Mathf.Abs(deltaPosition / LANDING_SENSITIVITY * 2), 1) * DEFAULT_SPEED;
        }
        else
        {
            targetSpeed = DEFAULT_SPEED;
        }
        //When close to base
        if (distance < GEAR_DOWN_DISTANCE)
        {
            //Gear down if it is still up
            if (planeBehaviour.GearCtrl.IsGearUp)
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
                if (velocity.magnitude < BREAK_VELOCITY)
                {
                    state = AIState.idle;
                }
            }
        }
    }

    //Attack player
    void attack()
    {
        turnOver();

        bool willCollide = altitude + TIME_TO_COLLISION * velocity.y < 0;
        bool falling = altitude < SAFE_ALTITUDE && CheckRotationBounds(SAFE_ANGLE, PI - SAFE_ANGLE);
        if (willCollide || falling || velocity.magnitude < STALL_VELOCITY)
        {
            state = AIState.climbing;
        }

        targetAngle = MathUtils.Vector2ToAngle(enemyPos - transform.position) + Mathf.PI;

        float deltaAngle = targetAngle - toRadian(rotation);
        setPitch(deltaAngle);

        if (Mathf.Cos(deltaAngle) < TURN_BACK_THRESHOLD)
        {
            planeBehaviour.turnBack();
        }

        if (Mathf.Abs(deltaAngle) < SHOOTING_ACCURACY && distanceToEnemy < SHOOTING_DISTANCE && !planeBehaviour.IsTurningBack)
        {
            planeBehaviour.shoot();
        }
    }

    //Turn over if upside down
    void turnOver()
    {
        if (turnCooldown.check())
        {
            if (CheckRotationBounds(2 * PI - SAFE_ANGLE, SAFE_ANGLE))
            {
                if (planeBehaviour.UpsideDown)
                {
                    planeBehaviour.turn();
                    turnCooldown.reset();
                    return;
                }
            }
            if (CheckRotationBounds(PI + SAFE_ANGLE, PI - SAFE_ANGLE))
            {
                if (!planeBehaviour.UpsideDown)
                {
                    planeBehaviour.turn();
                    turnCooldown.reset();
                }
            }
        }
    }

    //Climb if altitude is too low or plane is going to hit the ground
    void climb()
    {
        planeBehaviour.Throttle = 100;
        targetAngle = UP_ANGLE + getHeading() * SAFE_ANGLE;
        float deltaAngle = toRadian(targetAngle - rotation);
        setPitch(deltaAngle);
    }

    //Control plane elevator according to target angle
    void setPitch(float deltaAngle)
    {
        float pitch = -Mathf.Sin(deltaAngle) * SENSITIVITY;
        planeBehaviour.Pitch = MathUtils.clamp(pitch, 1);
    }

    //Degrees to radian
    float toRadian(float angle)
    {
        return angle / 180 * Mathf.PI;
    }

    //Check if plane rotation is between angle a and b
    bool CheckRotationBounds(float a, float b)
    {
        if (Mathf.Abs(b - a) < PI)
        {
            return rotation < Mathf.Max(a, b) && rotation > Mathf.Min(a, b);
        }
        else
        {
            return rotation > Mathf.Max(a, b) || rotation < Mathf.Min(a, b);
        }
    }

    //Heading of the plane. -1 when looking left, 1 when right.
    float getHeading()
    {
        return Mathf.Sign(Mathf.Cos(toRadian(rotation)));
    }
}
