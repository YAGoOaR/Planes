using UnityEngine;

//A script to controll plane landing gear
public class GearController : MonoBehaviour
{
    bool isGearUp;
    public bool IsGearUp
    {
        get => isGearUp;
    }

    Animator gearAnimator;
    CircleCollider2D wheel;
    PhysicsMaterial2D wheelMaterial;
    PhysicsMaterial2D wheelBrakeMaterial;
    PlanePart gear;

    void Start()
    {
        wheel = GetComponent<CircleCollider2D>();
        wheelMaterial = GameAssets.Instance.WheelMaterial;
        wheelBrakeMaterial = GameAssets.Instance.WheelBrakeMaterial;
        gearAnimator = GetComponentInChildren<Animator>();
        gear = GetComponent<PlanePart>();
    }

    public void SwitchGear(bool on)
    {
        if (gear.IsBroken) return;
        gearAnimator.SetBool("gearUp", on);
    }

    public void GearSwitched()
    {
        if (gear.IsBroken) return;
        isGearUp = !isGearUp;
        GetComponent<CircleCollider2D>().enabled = !isGearUp;
    }

    public void SwitchBrakes(bool on)
    {
        if (gear.IsBroken) return;

        wheel.sharedMaterial = !on ? wheelMaterial : wheelBrakeMaterial;
    }
}