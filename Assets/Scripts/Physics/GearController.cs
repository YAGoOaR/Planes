using UnityEngine;

public class GearController : MonoBehaviour
{
    public bool isGearUp { get; private set; }
    private Animator gearAnimator;

    void Start()
    {
        isGearUp = false;
        gearAnimator = gameObject.GetComponent<Animator>();
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
}