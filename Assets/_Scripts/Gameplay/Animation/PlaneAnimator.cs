using UnityEngine;

public class PlaneAnimator : SpriteAnimator
{
    AeroPlane planeBehaviour;
    [SerializeField] Sprite[] turnAnimation;
    [SerializeField] Sprite[] shakeAnimation;
    [SerializeField] Sprite[] turnBackStartAnimation;
    [SerializeField] Sprite[] turnBackEndAnimation;
    void Start()
    {
        planeBehaviour = GetComponent<AeroPlane>();
    }

    public void Turn()
    {
        PlayAnimation(turnAnimation, 1, () => { planeBehaviour.OnTurnExit(); });
    }

    public void TurnBack(float duration)
    {
        PlayAnimation(turnBackStartAnimation, duration / 2, () => {
            planeBehaviour.OnTurnBackMiddle();
            PlayAnimation(turnBackEndAnimation, duration / 2, () => {
                planeBehaviour.OnTurnBackExit();
            });
            OnFrame = (frame) => {
                if (frame == 3) {
                    planeBehaviour.HidePropeller(false);
                }
            };
        });
        OnFrame = (frame) => {
            if (frame == 3)
            {
                planeBehaviour.HidePropeller(true);
            }
        };

    }

}
