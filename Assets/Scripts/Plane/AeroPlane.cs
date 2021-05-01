using System;
using System.Collections.Generic;
using UnityEngine;
public class AeroPlane : MonoBehaviour
{
    private PlaneBehaviour planeBehaviour;

    [SerializeField]
    bool isPlayer = true;

    [SerializeField]
    bool startInOtherHeading = false;

    private PlanePart[] parts;
    [SerializeField]
    private int hp = 4;
    [SerializeField]
    private int bullets = 150;
    [SerializeField]
    private int bombCount = 1;
    Queue<GameObject> bombs;

    [SerializeField] private FlightModel model;

    [SerializeField] private Vector3 bombOffset = new Vector3(0, -0.5f, 0);

    [Serializable]
    public class FlightModel
    {
        [SerializeField] private float shootingAccuracy = 2;
        [SerializeField] private float jointForce = 20f;
        [SerializeField] private float flapMotorSpeed = 20;
        [SerializeField] private float flapMaxTorque = 1000;
        [SerializeField] private float pitchForceCoef = 15;
        [SerializeField] private float gearSwitchVelocity = 5;
        [SerializeField] private float maxVelocityCoef = 0.2f;
        [SerializeField] private float minAngleCoef = 0.9f;
        [SerializeField] private float velocityOffset = 0.3f;
        [SerializeField] private float turnBackDrag = 0.1f;
        [SerializeField] private float trimPitch = 0f;
        [SerializeField] private float gunOffset = 1.6f;
        [SerializeField] private float gunOffsetAngle = -0.2f;

        public float ShootingAccuracy { get => shootingAccuracy; }
        public float JointForce { get => jointForce; }
        public float FlapMotorSpeed { get => flapMotorSpeed; }
        public float FlapMaxTorque { get => flapMaxTorque; }
        public float PitchForceCoef { get => pitchForceCoef; }
        public float GearSwitchVelocity { get => gearSwitchVelocity; }
        public float MaxVelocityCoef { get => maxVelocityCoef; }
        public float MinAngleCoef { get => minAngleCoef; }
        public float VelocityOffset { get => velocityOffset; }
        public float TurnBackDrag { get => turnBackDrag; }
        public float TrimPitch { get => trimPitch; }
        public float GunOffset { get => gunOffset; }
        public float GunOffsetAngle { get => gunOffsetAngle; }
    }

    public PlanePart[] Parts
    {
        get { return parts; }
    }

    public int HP
    {
        get { return hp; }
    }

    public int Bullets
    {
        get { return bullets; }
        set { bullets = value; }
    }

    public int BombCount
    {
        get { return bombCount; }
        set { bombCount = value; }
    }

    public Queue<GameObject> Bombs
    {
        get { return bombs; }
    }

    public FlightModel Model { get => model; }
    public Vector3 BombOffset { get => bombOffset; }
    public bool IsPlayer { get => isPlayer; }
    public bool StartInOtherHeading { get => startInOtherHeading; }

    void Awake()
    {
        planeBehaviour = gameObject.AddComponent<PlaneBehaviour>();
    }

    //Called after plane initialization
    void Start()
    {
        bombs = new Queue<GameObject>();
        parts = transform.GetComponentsInChildren<PlanePart>(true);
    }

    public PlanePart getPart(string name)
    {
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].PartName == name)
            {
                return parts[i];
            }
        }
        return null;
    }

    public void Damage()
    {
        hp--;
    }
}
