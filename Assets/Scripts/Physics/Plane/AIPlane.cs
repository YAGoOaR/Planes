using UnityEngine;

public class AIPlane : MonoBehaviour
{
    const float fieldPos = -900;
    const float landingALT = 25;
    const float defaultSpeed = 40;
    const float defaultALT = 40;
    const float sensitivity = 5;
    const float trim = .2f;

    [HideInInspector]
    public float targetALT = 40;
    [HideInInspector]
    public float targetPos = 0;
    [HideInInspector]
    public float targetSpeed = defaultSpeed;
    [HideInInspector]
    public int startThrottle = 0;

    float targetAngle = -15;
    bool land = false;
    bool idle = false;
    PlaneBehaviour PB;
    Timers.CooldownTimer cooldownTimer;
    Timers.CooldownTimer waitBeforeTurn;

    void Start()
    {
        cooldownTimer = new Timers.CooldownTimer(10);
        waitBeforeTurn = new Timers.CooldownTimer(1, true);
        PB = GetComponent<PlaneBehaviour>();
        PB.isPlayer = false;
        PB.throttle = startThrottle;
    }

    void Update()
    {
        if (idle) return;
        float velocity = GetComponent<Rigidbody2D>().velocity.magnitude;
        float alt = transform.position.y;
        float rotation = transform.rotation.eulerAngles.z;
        float deltaAngle = targetAngle - rotation;
        float pos = transform.position.x;
        float deltaPos = targetPos - pos;
        float deltaALT = targetALT - alt;
        float distance = Mathf.Abs(deltaPos);

        if (cooldownTimer.check() && velocity > 30)
        {
            if (!PB.upsideDown && pos < targetPos)
            {
                PB.turnBack();
                cooldownTimer.reset();
            }
            if (PB.upsideDown && pos > targetPos)
            {
                PB.turnBack();
                cooldownTimer.reset();
            }
        }

        int sign = 1;
        if (PB.upsideDown)
        {
            sign = -1;
            deltaAngle += 180;
        }
        float deltaSpeed = targetSpeed - Mathf.Abs(velocity);
        if (!PB.isTurningBack)
        {
            if (deltaSpeed > 2) PB.throttle = 100;
            else if (deltaSpeed < -2) PB.throttle = 0;
            else
            {
                PB.throttle = 50;
            }
        }
        float pitch = -Mathf.Sin(deltaAngle / 180 * Mathf.PI) * sensitivity + sign * trim;
        PB.pitch = MathUtils.clamp(pitch, 1);
        if (!PB.gearCtrl.isGearUp && alt > 30) { PB.switchGear(); }
        targetAngle = -MathUtils.clamp(targetALT - alt, 15) * sign * MathUtils.clamp(velocity / 30, 2);
        float timeToCollision = Mathf.Sqrt(alt / 9.8f);
        float targetDistance = Mathf.Abs(timeToCollision * GetComponent<Rigidbody2D>().velocity.x) + 18;
        if (Mathf.Abs(targetDistance - Mathf.Abs(deltaPos)) < .5f)
        {
            PB.throwBomb();
            waitBeforeTurn.reset();
        }
        if (PB.bombs.Count == 0 && waitBeforeTurn.check())
        {
            land = true;
            targetPos = fieldPos;
        }
        if (land && !PB.isTurningBack)
        {
            targetALT = MathUtils.clamp(Mathf.Abs(deltaPos / 1000), 1) * (defaultALT - landingALT) + landingALT;
            if (deltaALT < 2)
            {
                targetSpeed = MathUtils.clamp(Mathf.Abs(deltaPos / 500), 1) * defaultSpeed;
            }
            else
            {
                targetSpeed = defaultSpeed;
            }
            if (distance < 200)
            {
                if (PB.gearCtrl.isGearUp)
                {
                    PB.switchGear(true);
                }
                if (!PB.flaps && distance < 100)
                {
                    PB.switchFlaps(true);
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
