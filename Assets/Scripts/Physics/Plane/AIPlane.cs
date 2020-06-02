using UnityEngine;

public class AIPlane : MonoBehaviour
{
    //Enemy plane autopilot v0.1. Enemy can take off, bomb target and then land on base.
    const float BASE_POSITION = -900;
    const float LANDING_ALTITUDE = 25;
    const float DEFAULT_SPEED = 40;
    const float DEFAULT_ALTITUDE = 40;
    const float SENSITIVITY = 5;
    const float TRIM = .2f;
    const float BOMB_THROW_ACCURACY = 0.5f;
    const float BOMB_OFFSET = 18;
    const float MIN_TURN_VELOCITY = 30;
    const float SPEED_ACCURACY = 2;
    const float GEAR_UP_ALTITUDE = 30;
    const float VELOCITY_SENSIVITY = 30;
    const float VELOCITY_COEFFICIENT = 2;
    const float GEAR_DOWN_DISTANCE = 200;
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
    Timers.CooldownTimer turnCooldown;
    Timers.CooldownTimer waitBeforeReurn;

    //Called once when this object initializes
    void Start()
    {
        turnCooldown = new Timers.CooldownTimer(10);
        waitBeforeReurn = new Timers.CooldownTimer(1, true);
        planeBehaviour = GetComponent<PlaneBehaviour>();
        planeBehaviour.isPlayer = false;
        planeBehaviour.throttle = startThrottle;
    }

    //Called once per frame
    //Autopilot realisation
    void Update()
    {
        //If plane has to do nothing
        if (idle) return;
        float velocity = GetComponent<Rigidbody2D>().velocity.magnitude;
        float altitude = transform.position.y;
        float rotation = transform.rotation.eulerAngles.z;
        float deltaAngle = targetAngle - rotation;
        float position = transform.position.x;
        float deltaPosition = targetPosition - position;
        float deltaAltitude = targetAltitude - altitude;
        float distance = Mathf.Abs(deltaPosition);
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
        //Plane heading: 1 when flying left (model is normal) and -1 when flying right (model is mirrored upside down and turned 180 degrees)
        int sign = 1;
        if (planeBehaviour.upsideDown)
        {
            sign = -1;
            deltaAngle += 180;
        }

        float deltaSpeed = targetSpeed - Mathf.Abs(velocity);
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
        float pitch = -Mathf.Sin(deltaAngle / 180 * Mathf.PI) * SENSITIVITY + sign * TRIM;
        planeBehaviour.pitch = MathUtils.clamp(pitch, 1);
        //Gear up when reached altitude
        if (!planeBehaviour.gearCtrl.isGearUp && altitude > GEAR_UP_ALTITUDE) planeBehaviour.switchGear();
        //Smooth reaching altitude and lining when speed is too low
        targetAngle = -MathUtils.clamp(targetAltitude - altitude, maxTargetAngle) * sign * MathUtils.clamp(velocity / VELOCITY_SENSIVITY, VELOCITY_COEFFICIENT);

        //My ballistics calculator
        float timeToCollision = Mathf.Sqrt(altitude / 9.8f);
        float targetDistance = Mathf.Abs(timeToCollision * GetComponent<Rigidbody2D>().velocity.x) + BOMB_OFFSET;
        if (Mathf.Abs(targetDistance - Mathf.Abs(deltaPosition)) < BOMB_THROW_ACCURACY)
        {
            planeBehaviour.throwBomb();
            waitBeforeReurn.reset();
        }
        //Return to base when out of bombs
        if (planeBehaviour.bombs.Count == 0 && waitBeforeReurn.check())
        {
            land = true;
            targetPosition = BASE_POSITION;
            waitBeforeReurn.reset();
            deltaPosition = targetPosition - position;
            distance = Mathf.Abs(deltaPosition);
        }
        //My landing autopilot
        if (land && !planeBehaviour.isTurningBack)
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
                if (planeBehaviour.gearCtrl.isGearUp && !waitBeforeReurn.check())
                {
                    planeBehaviour.switchGear(true);
                }
                //Turn on flaps
                if (!planeBehaviour.flaps && distance < GEAR_DOWN_DISTANCE / 2)
                {
                    planeBehaviour.switchFlaps(true);
                }
                //Stop
                if (distance < GEAR_DOWN_DISTANCE / 2)
                {
                    targetSpeed = 0;
                    //Now do nothing
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
