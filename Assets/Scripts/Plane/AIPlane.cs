using UnityEngine;

public class AIPlane : MonoBehaviour
{
    //Take off
    const float GEAR_UP_ALTITUDE = 25;
    const float TAKE_OFF_ALTITUDE = 40;

    //Climb
    const float climbAngle = 30;
    const float CLIMB_ALTITUDE = 40;

    //Attack
    const float SHOOTING_ACCURACY = 5;

    //PreventCollision
    const float DANGEROUS_ALT = 50;
    const float DANGEROUS_ANGLE = -20;


    //const float TIME_TO_COLLISION = 2;

    [SerializeField] Teams.Team enemyTeam = Teams.Team.Allies;

    ////Land
    //const float GEAR_DOWN_DISTANCE = 200;
    //const float BRAKE_DISTANCE = GEAR_DOWN_DISTANCE * 2 / 3;

    //const float PI = 180;
    //const float GRAVITY = 9.8f;
    //const float BASE_POSITION = -860;
    //const float LANDING_ALTITUDE = 21.2f;
    //const float DEFAULT_SPEED = 40;
    //const float DEFAULT_ALTITUDE = 40;
    //const float SENSITIVITY = 5;
    //const float BOMB_THROW_ACCURACY = 0.5f;
    //const float BOMB_OFFSET = 18;
    //const float MIN_TURN_VELOCITY = 30;
    //const float SPEED_ACCURACY = 2;

    //const float VELOCITY_SENSIVITY = 30;
    //const float MAX_VELOCITY_COEF = 2;


    //const float MAX_TARGET_ANGLE = 15;
    //const float ATTACK_DISTANCE = 240;
    //const float BOMBING_DISTANCE = 250;

    //const float TURN_BACK_THRESHOLD = -0.1f;

    //const float SAFE_ALTITUDE = 40;
    //const float STALL_VELOCITY = 15;
    //const float LANDING_SENSITIVITY = 1000;
    //const float UP_ANGLE = 250;
    //const float SAFE_ANGLE = 120;
    //const float DEFAULT_ANGLE = 10;
    //const float BRAKE_VELOCITY = 20;


    //float targetSpeed = DEFAULT_SPEED;

    [SerializeField]
    AIState state = AIState.takingOff;
    AIState prevState = AIState.takingOff;

    GameObject currentEnemy = null;

    PlaneBehaviour planeBehaviour;
    PlaneController planeController;
    Timers.CooldownTimer turnCooldown;

    Teams teamsInstance;

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

    void Start()
    {
        turnCooldown = new Timers.CooldownTimer(3);
        planeController = GetComponent<PlaneController>();
        planeBehaviour = GetComponent<PlaneBehaviour>();
        teamsInstance = Teams.Instance;
    }

    void setState(AIState newState)
    {
        Debug.Log(newState);
        prevState = state;
        state = newState;
    }

    void Update()
    {
        if (!PreventCollision()) TurnOver();


        switch (state)
        {
            case AIState.idle:
                FindSomethingToDo();
                break;
            case AIState.takingOff:
                TakeOff();
                break;
            case AIState.attacking:
                Attack();
                break;
            case AIState.climbing:
                Climb();
                break;
            default:
                break;
        }
    }

    void FindSomethingToDo()
    {
        if (!planeBehaviour.GearUp) setState(AIState.takingOff);

        currentEnemy = teamsInstance.FindClosestToMe(enemyTeam, transform.position)?.gameObject;
        if (currentEnemy != null)
        {
            setState(AIState.attacking);
        }
    }

    void TakeOff()
    {
        bool gearUp = planeBehaviour.GearUp;
        if (transform.position.y > GEAR_UP_ALTITUDE && !gearUp) {
            planeController.SetGear(false);
        }
        if (transform.position.y > TAKE_OFF_ALTITUDE && gearUp)
        {
            setState(AIState.idle);
            return;
        }
        GainAltitude();
    }

    Vector3 GetForwardHorizontalDir()
    {
        return Vector3.Project(-transform.right, Vector3.right).normalized;
    }

    void Climb()
    {
        if (transform.position.y > DANGEROUS_ALT)
        {
            setState(AIState.idle);
            return;
        }
        GainAltitude();
    }

    void GainAltitude()
    {
        planeController.SetThrottle(100);

        Vector3 forwardHorizontalDir = GetForwardHorizontalDir();
        planeController.SetHeading(Quaternion.Euler(0, 0, climbAngle * Mathf.Sign(forwardHorizontalDir.x)) * forwardHorizontalDir);
    }

    void Attack()
    {
        Vector2 enemyDir = currentEnemy.transform.position - transform.position;
        Vector2 forwardDir = -transform.right;

        float deltaAngle = Vector2.Angle(enemyDir, forwardDir);

        planeController.SetTarget(currentEnemy.transform.position);
        float dist = (currentEnemy.transform.position - transform.position).magnitude;

        if (Mathf.Abs(deltaAngle) < SHOOTING_ACCURACY && dist < planeController.GunRange && planeBehaviour.NormalState)
        {
            planeController.Shoot();
        }
    }

    bool IsUpsideDown()
    {
        return (transform.up * (planeBehaviour.UpsideDown ? -1 : 1)).y < 0;
    }

    void TurnOver()
    {
        if (IsUpsideDown() && turnCooldown.check())
        {
            planeController.Roll();
            turnCooldown.reset();
        }
    }

    bool CheckIfIsFalling()
    {
        float alt = transform.position.y;
        return alt < DANGEROUS_ALT && Vector2.Angle(Vector2.down, -transform.right) < 90 + DANGEROUS_ANGLE;
    }

    bool PreventCollision()
    {
        //bool willCollide = altitude + TIME_TO_COLLISION * velocity.y < 0;
        //bool falling = checkIfIsFalling();
        //if (willCollide || falling || velocity.magnitude < STALL_VELOCITY)
        //{
        //    setState(AIState.climbing);
        //}
        bool falling = CheckIfIsFalling();
        if (state != AIState.climbing && falling) 
        {
            planeController.TurnBack();
            setState(AIState.climbing);
        }
        return falling;
    }

    //void reachTarget()
    //{
    //    planeController.SetTarget(enemyPos);
    //}



    //void bombTarget()
    //{
    //    float timeToCollision = Mathf.Sqrt(altitude / GRAVITY);
    //    float attackPosition = Mathf.Abs(timeToCollision * GetComponent<Rigidbody2D>().velocity.x) + BOMB_OFFSET;
    //    float distanceToAttack = Mathf.Abs(attackPosition - Mathf.Abs(deltaPosition));
    //    if (distanceToAttack < BOMB_THROW_ACCURACY)
    //    {
    //        bombBay.throwBomb();
    //        setState(state = AIState.reachingTarget);
    //        Timers.delay(2, () =>
    //        {
    //            targetPosition = BASE_POSITION;
    //            deltaPosition = targetPosition - position;
    //            distance = Mathf.Abs(deltaPosition);
    //            setState(state = AIState.landing);
    //        });
    //    }
    //}

    //void land()
    //{
    //    //Smooth altitude lowering
    //    targetAltitude = MathUtils.clampPlusMinus(Mathf.Abs(deltaPosition / LANDING_SENSITIVITY), 1) * (DEFAULT_ALTITUDE - LANDING_ALTITUDE) + LANDING_ALTITUDE;
    //    //Reach landing speed if not missed altitude, otherwise reach normal
    //    if (deltaAltitude < 2)
    //    {
    //        targetSpeed = MathUtils.clampPlusMinus(Mathf.Abs(deltaPosition / LANDING_SENSITIVITY * 2), 1) * DEFAULT_SPEED;
    //    }
    //    else
    //    {
    //        targetSpeed = DEFAULT_SPEED;
    //    }
    //    //When close to base
    //    if (distance < GEAR_DOWN_DISTANCE)
    //    {
    //        //Gear down if it is still up
    //        if (planeBehaviour.GearUp)
    //        {
    //            planeController.SetGear(true);
    //            planeController.SetBrakes(true);
    //        }
    //        //Stop
    //        if (distance < BRAKE_DISTANCE)
    //        {
    //            planeController.SetFlaps(true);
    //            targetSpeed = 0;
    //            //Now do nothing
    //            if (velocity.magnitude < BRAKE_VELOCITY)
    //            {
    //                setState(state = AIState.idle);
    //            }
    //        }
    //    }
    //}




    //bool checkTurnSafety()
    //{
    //    const float MAX_VELOCITY = 0.3f;
    //    const float MIN_ANGLE = 0.9f;
    //    const float SAFE_ALTITUDE = 5;
    //    float velocityCoefficient = Mathf.Min(1 / velocity.magnitude * 10, MAX_VELOCITY);
    //    float angleCoefficient = Mathf.Max(-Mathf.Sin(rotation * Mathf.Deg2Rad) + 1, MIN_ANGLE);
    //    return altitude - velocity.y * velocityCoefficient * angleCoefficient > SAFE_ALTITUDE;
    //}
}
