using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField]
    private Sprite[] frameArray;
    private SpriteRenderer spriteRenderer;
    private float timer;
    private int currentFrame;
    private float duration = 5;
    private bool animating = false;
    private bool loop = false;

    public delegate void callback();
    public delegate void listener(int frame);

    private callback cb;
    private listener onFrame;

    public listener OnFrame { set { onFrame = value; } }

    public Sprite[] FrameArray
    {
        get { return frameArray; }
        set { frameArray = value; }
    }

    public bool Loop
    {
        get { return loop; }
        set { loop = value; }
    }

    private float framerate
    {
        get
        {
            return duration / frameArray.Length;
        }
    }

    public int frames { get { return frameArray.Length; } }

    public int frame
    {
        set
        {
            if (value > 1 || value < 0) return;
            currentFrame = value;
            spriteRenderer.sprite = frameArray[currentFrame];
        }
    }

    public void PlayAnimation(Sprite[] frameArr, float duration, callback onLoopEnd)
    {
        onFrame = null;
        frameArray = frameArr;
        animating = true;
        currentFrame = 0;
        this.duration = duration;
        timer = framerate;
        cb = onLoopEnd;
    }

    public void stopAnimation()
    {
        animating = false;
    }

    private void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!animating) return;
        timer += Time.deltaTime;
        float maxtime = framerate;
        if(timer >= maxtime)
        {
            timer -= maxtime;
            currentFrame++;
            if (currentFrame >= frames)
            {
                animating = loop;
                currentFrame = 0;
                timer = maxtime;
                cb();
            }
            if (onFrame != null)
            {
                onFrame(currentFrame);
            }
            spriteRenderer.sprite = frameArray[currentFrame];
        }
    }
}
