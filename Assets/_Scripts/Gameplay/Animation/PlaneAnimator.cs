using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAnimator : MonoBehaviour
{
    SpriteAnimator spriteAnimator;
    PlaneBehaviour planeBehaviour;
    [SerializeField] Sprite[] turnAnimation;
    [SerializeField] Sprite[] shakeAnimation;
    [SerializeField] Sprite[] turnBackStartAnimation;
    [SerializeField] Sprite[] turnBackEndAnimation;
    void Start()
    {
        spriteAnimator = GetComponent<SpriteAnimator>();
        planeBehaviour = GetComponent<PlaneBehaviour>();
    }

    public void StopAnimation()
    {
        spriteAnimator.StopAnimation();
    }

    public void Turn()
    {
        spriteAnimator.PlayAnimation(turnAnimation, 1, () => { planeBehaviour.OnTurnExit(); });
    }

    public void TurnBack(float duration)
    {
        spriteAnimator.PlayAnimation(turnBackStartAnimation, duration / 2, () => {
            planeBehaviour.OnTurnBackMiddle();
            spriteAnimator.PlayAnimation(turnBackEndAnimation, duration / 2, () => {
                planeBehaviour.OnTurnBackExit();
            });
            spriteAnimator.OnFrame = (frame) => {
                if (frame == 3) {
                    planeBehaviour.HidePropeller(false);
                }
            };
        });
        spriteAnimator.OnFrame = (frame) => {
            if (frame == 3)
            {
                planeBehaviour.HidePropeller(true);
            }
        };

    }

}
