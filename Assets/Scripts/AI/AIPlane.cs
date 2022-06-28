using UnityEngine;

public class AIPlane : MonoBehaviour
{
    //Take off
    const float gearUpAltitude = 25;
    const float takeOffAltitude = 30;

    //Climb
    const float climbAngle = 30;

    //Attack
    const float shootingAccuracy = 5;
    const float stallVelocity = 15;
    const float stallExitVelocity = 40;
    const float groundAttackRange = 200;

    const float AttackAirMinDist = 70;
    const float AvoidCollisionVelocityThreshold = 70;

    const float AttackGroundMinDist = 60;
    const float AttackGroundExit = 200;

    const float collisionAvoidAngle = 20;

    //PreventCollision
    const float dangerousAlt = 50;
    const float dangerousAngle = -20;
    [SerializeField] bool preventCollisions = false;

    //Bomb
    const float bombThrowAccuracy = 1f;
    const float bombAttackDist = 400;
    [SerializeField] float climbBombAlt = 70;
    [SerializeField] float climbBombAltMin = 50;

    //Target
    const float maxGroundTargetAlt = 15;
    const float maxGroundTargetSpeed = 20;

    //Land
    const float LandAccuracy = 80;

    //timers
    const float turnCooldownTime = 3;
    const float waitTime = 2;

    [SerializeField] Teams.Team enemyTeam = Teams.Team.Allies;

    Vector3 home;

    [SerializeField]
    AIState state = AIState.takingOff;
    AIState prevState = AIState.takingOff;

    Transform currentEnemy = null;
    Rigidbody2D enemyRB = null;

    PlaneBehaviour planeBehaviour;
    PlaneController planeController;
    Timers.CooldownTimer turnCooldown;
    Timers.CooldownTimer waitCooldown;
    Rigidbody2D rb;
    Teams teamsInstance;
    Transform bombBayTransform;
    Vector3 EnemyDelta { get => currentEnemy.position - transform.position; }
    [SerializeField] AIType attackerType;

    public enum AIType
    {
        fighter,
        bomber,
    }

    public enum TargetType
    {
        air,
        ground,
    }

    public enum AIState
    {
        idle,
        takingOff,
        landing,
        attacking,
        climbing,
        climbingToBomb,
        reachingTarget,
        stalling,
        waiting,
        chargingAttack,
    }

    void Start()
    {
        turnCooldown = new Timers.CooldownTimer(turnCooldownTime);
        waitCooldown = new Timers.CooldownTimer(waitTime);
        planeController = GetComponent<PlaneController>();
        planeBehaviour = GetComponent<PlaneBehaviour>();
        bombBayTransform = planeController.BombBay;
        teamsInstance = Teams.Instance;
        rb = GetComponent<Rigidbody2D>();
        home = transform.position;
    }

    void FixedUpdate()
    {
        if (!OnCollisionCourseWithGround()) TurnOver();

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
            case AIState.climbingToBomb:
                ClimbToBomb();
                break;
            case AIState.stalling:
                PreventStall();
                break;
            case AIState.landing:
                Land();
                break;
            case AIState.waiting:
                Wait();
                break;
            case AIState.chargingAttack:
                ChargeAttack();
                break;
            default:
                break;
        }
    }

    // Initialize AI state if idle

    void FindSomethingToDo()
    {
        if (!planeBehaviour.GearUp) { SetState(AIState.takingOff); return; }

        if(attackerType == AIType.fighter)
        {
            currentEnemy = teamsInstance.FindClosestToMe(enemyTeam, transform.position)?.transform;
        }
        else if (attackerType == AIType.bomber)
        {
            currentEnemy = teamsInstance.FindBiggest(enemyTeam)?.transform;
        }

        enemyRB = currentEnemy?.GetComponent<Rigidbody2D>();

        if (currentEnemy != null)
        {
            SetState(AIState.attacking);
        }
    }

    // Define if the enemy is a ground or air target
    TargetType DefineTargetType()
    {
        return (
            currentEnemy.position.y < maxGroundTargetAlt
            ? enemyRB.velocity.magnitude < maxGroundTargetSpeed
                ? TargetType.ground
                : TargetType.air
            : TargetType.air
        );
    }

    // A method to attack anything while in AIState.attacking state
    void Attack()
    {
        // if plane is likely to fall because of the lack of speed
        if (rb.velocity.magnitude < stallVelocity)
        {
            SetState(AIState.stalling);
            return;
        }

        if(currentEnemy == null)
        {
            SetState(AIState.idle);
            return;
        }

        switch (DefineTargetType())
        {
            case TargetType.air:
                AttackAir();
                break;
            case TargetType.ground:
                AttackGround();
                break;
            default:
                break;
        }
    }

    // If the vehicle is upside down then turn over
    void TurnOver()
    {
        if (IsUpsideDown() && turnCooldown.Check())
        {
            planeController.Roll();
            turnCooldown.Reset();
        }
    }

    // Shoot anything with guns
    void ShootTarget(float overrideRange = 0)
    {
        Vector2 enemyDir = currentEnemy.position - transform.position;
        Vector2 forwardDir = -transform.right;

        float deltaAngle = Vector2.Angle(enemyDir, forwardDir);

        planeController.SetTarget(currentEnemy.position);
        float dist = (currentEnemy.position - transform.position).magnitude;
        float range = overrideRange == 0 ? planeController.GunRange : overrideRange;

        if (Mathf.Abs(deltaAngle) < shootingAccuracy && dist < range && planeBehaviour.NormalState)
        {
            planeController.Shoot();
        }
    }

    // Gather altitude to bomb targets
    void ClimbToBomb()
    {
        if (transform.position.y > climbBombAlt)
        {
            SetState(AIState.attacking);
            return;
        }
        GatherAltitude();
    }

    // Attack air targets
    void AttackAir()
    {
        if (!planeController.HasAmmo)
        {
            SetState(AIState.landing);
            return;
        }

        if (preventCollisions)
        {
            Vector3 velocityDelta = enemyRB.velocity - rb.velocity;
            float closingSpeed = (Vector2.Angle(velocityDelta, EnemyDelta) > 90 ? 1 : -1) * velocityDelta.magnitude;

            if (EnemyDelta.magnitude < AttackAirMinDist && closingSpeed > AvoidCollisionVelocityThreshold)
            {
                float angleBetweenVelocities = Vector2.SignedAngle(enemyRB.velocity, rb.velocity);
                planeController.SetHeading(Quaternion.Euler(0, 0, -Mathf.Sign(angleBetweenVelocities) * collisionAvoidAngle) * -EnemyDelta.normalized);
                return;
            }
        }

        ShootTarget();
    }

    // Attack ground targets
    void AttackGround()
    {
        if (!planeController.HasBombs && planeController.HasAmmo)
        {
            if (EnemyDelta.magnitude < AttackGroundMinDist)
            {
                if (Vector2.Angle(EnemyDelta, -transform.right) < 90) planeController.TurnBack();
                SetState(AIState.chargingAttack);
                return;
            }
            ShootTarget(groundAttackRange);
        }
        else
        {
            BombTarget();
        }
    }


    // Gather distance to perform a new attack
    void ChargeAttack()
    {
        Vector3 delta = EnemyDelta;
        if (delta.magnitude > AttackGroundExit)
        {
            SetState(AIState.attacking);
            return;
        }
        planeController.SetHeading(-delta);
    }

    // Accurately throw a bomb using ballistics calculation
    void BombTarget()
    {
        if (transform.position.y < climbBombAltMin)
        {
            SetState(AIState.climbingToBomb);
            return;
        }

        if (!planeController.HasBombs && IsHeadingTowardsHome())
        {
            SetState(AIState.landing);
            return;
        }

        Vector3 forwardDir = GetForwardHorizontalDir();

        Vector3 delta = currentEnemy.position - bombBayTransform.position;

        planeController.SetHeading(forwardDir);

        bool sameDir = Mathf.Sign(forwardDir.x) == Mathf.Sign(delta.x);

        if (!sameDir && Mathf.Abs(delta.x) > bombAttackDist) planeController.TurnBack();

        float g = 9.81f;
        float D = Mathf.Pow(-rb.velocity.y, 2) - 2 * g * (delta.y);

        float timeToCollision = (rb.velocity.y + Mathf.Sqrt(D)) / (2 * g / 2);

        float fallPoint = rb.velocity.x * timeToCollision;
        bombPos = new Vector3(transform.position.x + fallPoint, currentEnemy.position.y, 0);

        float accuracy = Mathf.Abs(delta.x - fallPoint);

        if (accuracy < bombThrowAccuracy)
        {
            planeController.ThrowBomb();
            waitCooldown.Reset();
            SetState(AIState.waiting);
            return;
        }
    }

    // Reach specific altitude and raise the gear
    void TakeOff()
    {
        bool gearUp = planeBehaviour.GearUp;
        if (transform.position.y > gearUpAltitude && !gearUp)
        {
            planeController.SetGear(false);
            planeController.SetFlaps(false);
        }
        if (transform.position.y > takeOffAltitude && gearUp)
        {
            SetState(AIState.idle);
            return;
        }
        GatherAltitude();
    }

    // Perform actions to exit stalling
    void PreventStall()
    {
        if (rb.velocity.magnitude > stallExitVelocity)
        {
            SetState(AIState.idle);
            return;
        }
        if (transform.position.y < dangerousAlt)
        {
            SetState(AIState.climbing);
            return;
        }
        GatherSpeed();
    }

    // Gather altitude state
    void Climb()
    {
        if (transform.position.y > dangerousAlt)
        {
            SetState(AIState.idle);
            return;
        }
        GatherAltitude();
    }

    // Gather altitude when called
    void GatherAltitude()
    {
        planeController.SetThrottle(100);

        Vector3 forwardHorizontalDir = GetForwardHorizontalDir();
        planeController.SetHeading(Quaternion.Euler(0, 0, climbAngle * Mathf.Sign(forwardHorizontalDir.x)) * forwardHorizontalDir);
    }

    // Gather speed
    void GatherSpeed()
    {
        planeController.SetThrottle(100);
        planeController.SetHeading(GetForwardHorizontalDir());
    }

    void Wait()
    {
        if (waitCooldown.Check())
        {
            SetState(AIState.idle);
            return;
        }
    }

    // Draw predicted bomb fall point
    Vector3 bombPos = Vector3.zero;
    private void OnDrawGizmos()
    {
        if (bombPos == Vector3.zero) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(bombPos, 1);
    }

    void SetState(AIState newState)
    {
        prevState = state;
        state = newState;
    }

    Vector3 GetForwardHorizontalDir()
    {
        return Vector3.Project(-transform.right, Vector3.right).normalized;
    }

    bool IsHeadingTowardsHome()
    {
        return Mathf.Sign(rb.velocity.x) == Mathf.Sign(home.x - transform.position.x);
    }

    bool OnCollisionCourseWithGround()
    {
        bool falling = CheckIfIsFalling();
        if (state != AIState.climbing && falling)
        {
            planeController.TurnBack();
            SetState(AIState.climbing);
        }
        return falling;
    }

    bool CheckIfIsFalling()
    {
        float alt = transform.position.y;
        return alt < dangerousAlt && Vector2.Angle(Vector2.down, -transform.right) < 90 + dangerousAngle;
    }

    bool IsUpsideDown()
    {
        return (transform.up * (planeBehaviour.UpsideDown ? -1 : 1)).y < 0;
    }

    bool IsNotUpsideDownTowards(Vector3 destination)
    {
        Vector3 delta = destination - transform.position;
        float dir = Mathf.Sign(delta.x);
        float currentDir = IsUpsideDown() ? 1 : -1;
        return dir == currentDir;
    }

    void Land()
    {
        Vector2 delta = home - transform.position;
        if (rb.velocity.magnitude < stallVelocity && Mathf.Abs(delta.x) > LandAccuracy)
        {
            SetState(AIState.stalling);
            return;
        }
        planeController.SetFlaps(true);
        planeController.SetTarget(home);
        planeController.SetBrakes(true);
        if (IsNotUpsideDownTowards(home)) planeController.SetGear(true);
        planeController.SetThrottle(0);
    }
}