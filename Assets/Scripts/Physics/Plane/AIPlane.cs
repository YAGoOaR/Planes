using UnityEngine;

public class AIPlane : MonoBehaviour
{
    const float FIELD_POSITION = -900;
    const float LANDING_ALTITUDE = 25;
    const float DEFAULT_SPEED = 40;
    const float DEFAULT_ALTITUDE = 40;
    const float SENSITIVITY = 5;
    const float TRIM = .2f;
    const float BOMB_THROW_ACCURACY = 0.5f;
    const float BOMB_OFFSET = 18;
    public float targetAltitude = 40;
    float maxTargetAngle = 15;
    [HideInInspector]
    public float targetPosition = 0;
    [HideInInspector]
    public float targetSpeed = DEFAULT_SPEED;
    [HideInInspector]
    public int startThrottle = 0;
    float targetAngle = -15;
    bool land = false;
    bool idle = false;
    PlaneBehaviour planeBehaviour;
    Timers.CooldownTimer cooldownTimer;
    Timers.CooldownTimer waitBeforeTurn;

    void Start()
    {
        cooldownTimer = new Timers.CooldownTimer(10);
        waitBeforeTurn = new Timers.CooldownTimer(1, true);
        planeBehaviour = GetComponent<PlaneBehaviour>();
        planeBehaviour.isPlayer = false;
        planeBehaviour.throttle = startThrottle;
    }

    void Update()
    {
        if (idle) return;
        float velocity = GetComponent<Rigidbody2D>().velocity.magnitude;
        float altitude = transform.position.y;
        float rotation = transform.rotation.eulerAngles.z;
        float deltaAngle = targetAngle - rotation;
        float position= transform.position.x;
        float deltaPosition = targetPosition - position;
        float deltaAltitude = targetAltitude - altitude;
        float distance = Mathf.Abs(deltaPosition);

        if (cooldownTimer.check() && velocity > 30)
        {
            if (!planeBehaviour.upsideDown && position < targetPosition)
            {
                planeBehaviour.turnBack();
                cooldownTimer.reset();
            }
            if (planeBehaviour.upsideDown && position > targetPosition)
            {
                planeBehaviour.turnBack();
                cooldownTimer.reset();
            }
        }

        int sign = 1;
        if (planeBehaviour.upsideDown)
        {
            sign = -1;
            deltaAngle += 180;
        }
        float deltaSpeed = targetSpeed - Mathf.Abs(velocity);
        if (!planeBehaviour.isTurningBack)
        {
            if (deltaSpeed > 2) planeBehaviour.throttle = 100;
            else if (deltaSpeed < -2) planeBehaviour.throttle = 0;
            else
            {
                planeBehaviour.throttle = 50;
            }
        }
        float pitch = -Mathf.Sin(deltaAngle / 180 * Mathf.PI) * SENSITIVITY + sign * TRIM;
        planeBehaviour.pitch = MathUtils.clamp(pitch, 1);
        if (!planeBehaviour.gearCtrl.isGearUp && altitude > 30) { planeBehaviour.switchGear(); }
        targetAngle = -MathUtils.clamp(targetAltitude - altitude, maxTargetAngle) * sign * MathUtils.clamp(velocity / 30, 2);
        float timeToCollision = Mathf.Sqrt(altitude / 9.8f);
        float targetDistance = Mathf.Abs(timeToCollision * GetComponent<Rigidbody2D>().velocity.x) + BOMB_OFFSET;
        if (Mathf.Abs(targetDistance - Mathf.Abs(deltaPosition)) < BOMB_THROW_ACCURACY)
        {
            planeBehaviour.throwBomb();
            waitBeforeTurn.reset();
        }
        if (planeBehaviour.bombs.Count == 0 && waitBeforeTurn.check())
        {
            land = true;
            targetPosition = FIELD_POSITION;
            waitBeforeTurn.reset();
            deltaPosition = targetPosition - position;
            distance = Mathf.Abs(deltaPosition);
        }
        if (land && !planeBehaviour.isTurningBack)
        {
            targetAltitude = MathUtils.clamp(Mathf.Abs(deltaPosition / 1000), 1) * (DEFAULT_ALTITUDE - LANDING_ALTITUDE) + LANDING_ALTITUDE;
            if (deltaAltitude < 2)
            {
                targetSpeed = MathUtils.clamp(Mathf.Abs(deltaPosition / 500), 1) * DEFAULT_SPEED;
            }
            else
            {
                targetSpeed = DEFAULT_SPEED;
            }   
            if (distance < 200)
            {
                if (planeBehaviour.gearCtrl.isGearUp && !waitBeforeTurn.check())
                {
                    planeBehaviour.switchGear(true);
                }
                if (!planeBehaviour.flaps && distance < 100)
                {
                    planeBehaviour.switchFlaps(true);
                }
                if (distance < 100)
                {
                    targetSpeed = 0;
                    if (velocity < 20)
                    {
                        land = false;
                        idle = true;
                    }
                }
            }
        }
    }


}
