using System.Collections.Generic;
using UnityEngine;

public class Timers : MonoBehaviour
{
    public class CooldownTimer
    {
        public static List<CooldownTimer> timers = new List<CooldownTimer>();
        public float curTime;
        public float maxTime;
        public bool destroyAfterTimeout = false;

        public CooldownTimer(float max)
        {
            timers.Add(this);
            maxTime = max;
            curTime = max;
        }

        public CooldownTimer(float max, bool destroyAfterTimeout)
        {
            timers.Add(this);
            maxTime = max;
            curTime = max;
            this.destroyAfterTimeout = destroyAfterTimeout;
        }

        public static void refresh()
        {
            foreach (CooldownTimer timer in timers)
            {
                if (timer.curTime <= timer.maxTime) timer.curTime += Time.deltaTime;
            }
        }

        public bool check()
        {
            if (curTime > maxTime) {
                if (destroyAfterTimeout)
                {
                    timers.Remove(this);
                } 
                return true;
            }
            return false;
        }

        public void reset()
        {
            curTime = 0;
        }
    }

    void Update()
    {
        CooldownTimer.refresh();
    }
}
