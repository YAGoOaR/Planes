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
    const float SAFE_ALTITUDE = 40;
    const float STALL_VELOCITY = 15;
    const float LANDING_SENSITIVITY = 1000;
    const float UP_ANGLE = 250;
    const float SAFE_ANGLE = 60;
    const float DEFAULT_ANGLE = 10;
    const float BRAKE_VELOCITY = 20;

    private bool enemyDestroyed;

    [SerializeField]
    float targetPosition = -30;
    [SerializeField]
    float targetAltitude = 40;
    float targetSpeed = DEFAULT_SPEED;
    float targetAngle = -15;
    AeroPlane plane;
    [SerializeField]
    AIState state = AIState.takingOff;
    AIState prevState = AIState.takingOff;
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

    public bool EnemyDestroyed
    {
        get { return enemyDestroyed; }
        set { enemyDestroyed = value; }
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

        turnOver();
        preventCollision();

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
                setState(prevState);
            }
        }
        else if (state != AIState.attacking)
        {
            if (distanceToEnemy < ATTACK_DISTANCE && plane.Bullets > 0 && !enemyDestroyed && deltaPosition > BOMBING_DISTANCE)
            {
                planeBehaviour.Throttle = 100;
                setState(AIState.attacking);
            }
        }

        if (state == AIState.bombingTarget)
        {
            bombTarget();
        }
        if (state == AIState.landing && !(planeBehaviour.State == PlaneBehaviour.PlaneState.turningBack))
        {
            land();
        }
    }

    void setState(AIState newState)
    {
        if(state == newState)
        {
            return;
        }
        prevState = state;
        state = newState;
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
        if (turnCooldown.check() && checkTurnSafety() && velocity.magnitude > MIN_TURN_VELOCITY && !CheckRotationBounds(DEFAULT_ANGLE, PI - DEFAULT_ANGLE))
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

        if (!(planeBehaviour.State == PlaneBehaviour.PlaneState.turningBack))
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
        float velocityCoef = MathUtils.clamped(velocity.magnitude / VELOCITY_SENSIVITY, MAX_VELOCITY_COEF);
        float altitudeCoef = -MathUtils.clamped(targetAltitude - altitude, MAX_TARGET_ANGLE);
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
            setState(AIState.bombingTarget);
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
            setState(state = AIState.reachingTarget);
            Timers.timeout(2, () =>
            {
                targetPosition = BASE_POSITION;
                deltaPosition = targetPosition - position;
                distance = Mathf.Abs(deltaPosition);
                setState(state = AIState.landing);
            });
        }
    }

    //My landing autopilot
    void land()
    {
        //Smooth altitude lowering
        targetAltitude = MathUtils.clamped(Mathf.Abs(deltaPosition / LANDING_SENSITIVITY), 1) * (DEFAULT_ALTITUDE - LANDING_ALTITUDE) + LANDING_ALTITUDE;
        //Reach landing speed if not missed altitude, otherwise reach normal
        if (deltaAltitude < 2)
        {
            targetSpeed = MathUtils.clamped(Mathf.Abs(deltaPosition / LANDING_SENSITIVITY * 2), 1) * DEFAULT_SPEED;
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
                if (velocity.magnitude < BRAKE_VELOCITY)
                {
                    setState(state = AIState.idle);
                }
            }
        }
    }

    //Attack player
    void attack()
    {
        turnOver();
        preventCollision();

        targetAngle = MathUtils.Vector2ToAngle(enemyPos - transform.position) + Mathf.PI;

        float deltaAngle = targetAngle - toRadian(rotation);
        setPitch(deltaAngle);

        if (Mathf.Cos(deltaAngle) < TURN_BACK_THRESHOLD && checkTurnSafety())
        {
            planeBehaviour.turnBack();
        }

        if (Mathf.Abs(deltaAngle) < SHOOTING_ACCURACY && distanceToEnemy < SHOOTING_DISTANCE && !(planeBehaviour.State == PlaneBehaviour.PlaneState.turningBack))
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
        planeBehaviour.Pitch = MathUtils.clamped(pitch, 1);
    }

    void preventCollision()
    {
        bool willCollide = altitude + TIME_TO_COLLISION * velocity.y < 0;
        bool falling = altitude < SAFE_ALTITUDE && CheckRotationBounds(SAFE_ANGLE, PI - SAFE_ANGLE);
        if (willCollide || falling || velocity.magnitude < STALL_VELOCITY)
        {
            setState(AIState.climbing);
        }
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

    bool checkTurnSafety()
    {
        const float MAX_VELOCITY = 0.3f;
        const float MIN_ANGLE = 0.9f;
        const float SAFE_ALTITUDE = 5;
        float velocityCoefficient = Mathf.Min(1 / velocity.magnitude * 10, MAX_VELOCITY);
        float angleCoefficient = Mathf.Max(-Mathf.Sin(toRadian(rotation)) + 1, MIN_ANGLE);
        return altitude - velocity.y * velocityCoefficient * angleCoefficient > SAFE_ALTITUDE;
    }

    //Heading of the plane. -1 when looking left, 1 when right.
    float getHeading()
    {
        return Mathf.Sign(Mathf.Cos(toRadian(rotation)));
    }
}
