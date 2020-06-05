using UnityEngine;

//A script to controll plane landing gear
public class GearController : MonoBehaviour
{
    public bool isGearUp = false;
    Animator gearAnimator;
    CircleCollider2D wheel;
    PhysicsMaterial2D wheelMaterial;
    PhysicsMaterial2D wheelBrakeMaterial;

    void Start()
    {
        wheel = GetComponent<CircleCollider2D>();
        wheelMaterial = GameAssets.instance.wheelMaterial;
        wheelBrakeMaterial = GameAssets.instance.wheelBrakeMaterial;
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