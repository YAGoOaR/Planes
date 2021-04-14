using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteAnimator), typeof(AeroPlane))]
public class PlaneAnimator : MonoBehaviour
{
    private SpriteAnimator spriteAnimator;
    private PlaneBehaviour planeBehaviour;
    [SerializeField]
    private Sprite[] turnAnimation;
    [SerializeField]
    private Sprite[] shakeAnimation;
    [SerializeField]
    private Sprite[] turnBackStartAnimation;
    [SerializeField]
    private Sprite[] turnBackEndAnimation;
    void Start()
    {
        spriteAnimator = GetComponent<SpriteAnimator>();
        planeBehaviour = GetComponent<PlaneBehaviour>();
    }

    public void StopAnimation()
    {
        spriteAnimator.stopAnimation();
    }

    public void turn()
    {
        spriteAnimator.PlayAnimation(turnAnimation, 1, () => { planeBehaviour.onTurnExit(); });
    }

    public void turnBack(float multiplier)
    {
        spriteAnimator.PlayAnimation(turnBackStartAnimation, 1 / multiplier / 2, () => {
            planeBehaviour.onTurnBackMiddle();
            spriteAnimator.PlayAnimation(turnBackEndAnimation, 1 / multiplier / 2, () => {
                planeBehaviour.onTurnBackExit();
            });
            spriteAnimator.OnFrame = (frame) => {
                if (frame == 3) {
                    planeBehaviour.hidePropeller(false);
                }
            };
        });
        spriteAnimator.OnFrame = (frame) => {
            if (frame == 3)
            {
                planeBehaviour.hidePropeller(true);
            }
        };

    }

}
