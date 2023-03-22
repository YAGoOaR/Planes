using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField]
    private Sprite[] frameArray;
    [SerializeField] SpriteRenderer spriteRenderer;
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

    private float Framerate
    {
        get
        {
            return duration / frameArray.Length;
        }
    }

    public int Frames { get { return frameArray.Length; } }

    public int Frame
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
        timer = Framerate;
        cb = onLoopEnd;
    }

    public void StopAnimation()
    {
        animating = false;
    }

    private void Start()
    {
        if (spriteRenderer != null) return;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!animating) return;
        timer += Time.deltaTime;
        float maxtime = Framerate;
        if(timer >= maxtime)
        {
            timer -= maxtime;
            currentFrame++;
            if (currentFrame >= Frames)
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
