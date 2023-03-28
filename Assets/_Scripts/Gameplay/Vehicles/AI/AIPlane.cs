using System;
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
    const float AttackAirCriticalDist = 20;
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
    const float maxGroundTargetAlt = 22;
    const float maxGroundTargetSpeed = 20;

    //Land
    const float turnOffMotorDist = 200;
    const float landingSpeed = 25;
    const float landingSpeedAccuracy = 3;
    const float landingSafeDist = 300;

    //timers
    const float turnCooldownTime = 3;
    const float defaultWaitTime = 2;
    const float dodgeWaitTime = 1;

    [SerializeField] Teams.Team enemyTeam = Teams.Team.Allies;
    Vehicle vehicle;

    Transform home;

    [SerializeField] AIState state = AIState.idle;

    Transform currentEnemy = null;
    Rigidbody2D enemyRB = null;

    PlaneBehaviour planeBehaviour;
    PlaneController planeController;
    Timers.CooldownTimer turnCooldown;
    Timers.CooldownTimer waitCooldown;
    Timers.CooldownTimer dodgeCooldown;
    Rigidbody2D rb;
    Teams teamsInstance;
    Transform bombBayTransform;

    Vector3? waitFlyDirection = null;
    Timers.CooldownTimer waitTimer = null;

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
        preparingToLand,
    }

    void Start()
    {
        turnCooldown = new Timers.CooldownTimer(turnCooldownTime);
        waitCooldown = new Timers.CooldownTimer(defaultWaitTime);
        waitTimer = waitCooldown;
        dodgeCooldown = new Timers.CooldownTimer(dodgeWaitTime);

        planeController = GetComponent<PlaneController>();
        planeBehaviour = GetComponent<PlaneBehaviour>();
        bombBayTransform = planeController.BombBay;
        teamsInstance = Teams.Instance;
        rb = GetComponent<Rigidbody2D>();
        vehicle = GetComponentInParent<Vehicle>();
        home = vehicle.Home;
    }

    void FixedUpdate()
    {
        if (!OnCollisionCourseWithGround()) TurnOver();

        Action stateFunc = state switch
        {
            AIState.idle => FindSomethingToDo,
            AIState.takingOff => TakeOff,
            AIState.attacking => Attack,
            AIState.climbing => Climb,
            AIState.climbingToBomb => ClimbToBomb,
            AIState.stalling => PreventStall,
            AIState.landing => Land,
            AIState.waiting => Wait,
            AIState.chargingAttack => ChargeAttack,
            AIState.preparingToLand => PrepareToLand,
            _ => () => { }
        };
        stateFunc();
    }

    // Initialize AI state if idle
    void FindSomethingToDo()
    {
        currentEnemy = attackerType switch
        {
            AIType.fighter => teamsInstance.FindClosestToMe(enemyTeam, transform.position)?.transform,
            AIType.bomber => teamsInstance.FindBiggest(enemyTeam)?.transform,
            _ => null
        };

        if (!planeBehaviour.GearUp) {
            if (currentEnemy != null) SetState(AIState.takingOff);
            return;
        }

        enemyRB = currentEnemy?.GetComponent<Rigidbody2D>();
        SetState(currentEnemy != null ? AIState.attacking : AIState.preparingToLand);
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

        Action nextAction = DefineTargetType() switch
        {
            TargetType.air => AttackAir,
            TargetType.ground => AttackGround,
            _ => null,
        };
        nextAction();
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
    
    // Predict target movement to shoot accurately
    Vector3 SimplePredict()
    {
        Vector3 delta = currentEnemy.transform.position - transform.position;
        float projectileVelocity = planeController.Guns.GunBulletVelocity;

        Vector3 deltaVelocity = enemyRB.velocity - rb.velocity;

        Vector2 tangentialMovement = Vector3.Project(deltaVelocity, Vector2.Perpendicular(delta));
        float orthogonalLen = tangentialMovement.magnitude;

        float closingSpeed = Mathf.Sqrt(Mathf.Pow(projectileVelocity, 2) - Mathf.Pow(orthogonalLen, 2));
        if (!(closingSpeed > 0)) return delta;

        Vector2 orthogonalMovement = delta.normalized * closingSpeed;
        Vector2 leadVector = orthogonalMovement + tangentialMovement;

        if (!(leadVector.magnitude > 0)) return delta;
        return leadVector;
    }

    // Shoot anything with guns
    void ShootTarget(float overrideRange = 0)
    {
        Vector2 enemyDir = currentEnemy.position - transform.position;
        Vector2 forwardDir = -transform.right;

        float deltaAngle = Vector2.Angle(enemyDir, forwardDir);

        planeController.SetHeading(SimplePredict());
        float dist = (currentEnemy.position - transform.position).magnitude;
        float range = overrideRange == 0 ? planeController.Guns.GunRange : overrideRange;

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
        if (!planeController.Guns.HasAmmo)
        {
            SetState(AIState.preparingToLand);
            return;
        }

        if (preventCollisions)
        {
            Vector3 velocityDelta = enemyRB.velocity - rb.velocity;
            float closingSpeed = (Vector2.Angle(velocityDelta, EnemyDelta) > 90 ? 1 : -1) * velocityDelta.magnitude;

            float dist = EnemyDelta.magnitude;

            if (dist < AttackAirMinDist && closingSpeed > AvoidCollisionVelocityThreshold || dist < AttackAirCriticalDist)
            {
                Vector3 away = -Vector3.Project(enemyRB.velocity, -Vector2.Perpendicular(rb.velocity));

                planeController.SetHeading(away);
                dodgeCooldown.Reset();
                SetState(AIState.waiting);
                waitFlyDirection = away;
                waitTimer = dodgeCooldown;
                return;
            }
        }

        ShootTarget();
    }

    // Attack ground targets
    void AttackGround()
    {
        if (!planeController.HasBombs && planeController.Guns.HasAmmo)
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
        if (currentEnemy == null)
        {
            SetState(AIState.idle);
            return;
        }

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
            SetState(AIState.preparingToLand);
            return;
        }

        Vector3 forwardDir = GetForwardHorizontalDirection();

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

        Vector3 forwardHorizontalDir = GetForwardHorizontalDirection();
        planeController.SetHeading(Quaternion.Euler(0, 0, climbAngle * Mathf.Sign(forwardHorizontalDir.x)) * forwardHorizontalDir);
    }

    // Gather speed
    void GatherSpeed()
    {
        planeController.SetThrottle(100);
        planeController.SetHeading(GetForwardHorizontalDirection());
    }

    // Do nothing for some time
    void Wait()
    {
        if (waitCooldown.Check())
        {
            SetState(AIState.idle);
            waitFlyDirection = null;
            waitTimer = waitCooldown;
        } 
        else
        {
            Vector3 flightDir = waitFlyDirection != null ? (Vector3)waitFlyDirection : rb.velocity;
            planeController.SetHeading(flightDir);
        }
    }

    // Draw predicted bomb fall point
    Vector3 bombPos = Vector3.zero;
    private void OnDrawGizmos()
    {
        if (bombPos == Vector3.zero) return;
        if (home.position == Vector3.zero) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(bombPos, 1);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(home.position, 1);
    }

    // Change state and save previous state
    void SetState(AIState newState)
    {
        state = newState;
    }

    Vector3 GetForwardHorizontalDirection()
    {
        return Vector3.Project(-transform.right, Vector3.right).normalized;
    }

    bool IsHeadingTowardsHome()
    {
        return Mathf.Sign(rb.velocity.x) == Mathf.Sign(home.position.x - transform.position.x);
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
        float currentDir = Mathf.Sign(-transform.right.x);
        return dir == currentDir * (IsUpsideDown() ? 0 : 1);
    }

    Vector3 Reject(Vector3 a, Vector3 b) => a - Vector3.Project(a, b);
    Vector3 ReflectWithCoef(Vector3 a, Vector3 b, float k) => Vector3.Project(a, b) - k * Reject(a, b);

    // Prepare to land
    void PrepareToLand()
    {
        Vector2 delta = home.position - transform.position;
        if (delta.magnitude < landingSafeDist) planeController.SetHeading(-delta);
        else SetState(AIState.landing);
    }

    // Advanced smooth landing path
    void Land()
    {
        Vector2 delta = home.position - transform.position;

        planeController.SetFlaps(true);
        Vector2 approachDir = Vector3.Project(delta, Vector2.left);
        Vector2 approachVector = ReflectWithCoef(approachDir, delta, 1f);

        planeController.SetHeading(approachVector, aimVelocity: true);
        planeController.SetBrakes(true);
        if (IsNotUpsideDownTowards(home.position)) planeController.SetGear(true);

        float speed = rb.velocity.magnitude;
        float speedDiff = landingSpeed - speed;
        int throttle = Mathf.Abs(delta.x) < turnOffMotorDist ? 0 :
            speedDiff > landingSpeedAccuracy ? 100 :
            speedDiff < - landingSpeedAccuracy ? 0 : 50;
        planeController.SetThrottle(throttle);
    }
}