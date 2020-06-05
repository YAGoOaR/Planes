using UnityEngine;

//A script to controll plane landing gear
public class GearController : MonoBehaviour
{
    bool isGearUp;
    public bool IsGearUp
    {
        get { return isGearUp; }
    }

    Animator gearAnimator;
    CircleCollider2D wheel;
    PhysicsMaterial2D wheelMaterial;
    PhysicsMaterial2D wheelBrakeMaterial;

    void Start()
    {
        wheel = GetComponent<CircleCollider2D>();
        wheelMaterial = GameAssets.Instance.WheelMaterial;
        wheelBrakeMaterial = GameAssets.Instance.WheelBrakeMaterial;
        gearAnimator = GetComponent<Animator>();
    }

    public void switchGear(bool on)
    {
        gearAnimator.SetBool("gearUp", on);
    }

    public void gearSwitched()
    {
        isGearUp = !isGearUp;
        gameObject.GetComponent<CircleCollider2D>().enabled = !isGearUp;
    }

    public void switchBrakes(bool on)
    {
        if (!on)
        {
            wheel.sharedMaterial = wheelMaterial;
        }
        else
        {
            wheel.sharedMaterial = wheelBrakeMaterial;
        }
    }
}