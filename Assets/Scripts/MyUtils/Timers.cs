using System.Collections.Generic;
using UnityEngine;

public class Timers : MonoBehaviour
{
    public delegate void customFunc();

    public class CooldownTimer
    {
        public static List<CooldownTimer> timers = new List<CooldownTimer>();
        public float currentTime;
        public float maxTime;
        public bool destroyAfterTimeout = false;

        public CooldownTimer(float max)
        {
            timers.Add(this);
            maxTime = max;
            currentTime = max;
        }

        public CooldownTimer(float max, bool destroyAfterTimeout)
        {
            timers.Add(this);
            maxTime = max;
            currentTime = max;
            this.destroyAfterTimeout = destroyAfterTimeout;
        }

        public static void refresh()
        {
            foreach (CooldownTimer timer in timers)
            {
                if (timer.currentTime <= timer.maxTime) timer.currentTime += Time.deltaTime;
            }
        }

        public bool check()
        {
            if (currentTime > maxTime)
            {
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
            currentTime = 0;
        }
    }

    public class TimeoutTimer
    {
        public static List<TimeoutTimer> timers = new List<TimeoutTimer>();
        public float time;
        public float currentTime;
        customFunc callback;

        public TimeoutTimer(float time, customFunc callback)
        {
            this.callback = callback;
            this.time = time;
            timers.Add(this);
        }

        public static void refresh()
        {
            List<TimeoutTimer> timerList = new List<TimeoutTimer>(timers);
            foreach (TimeoutTimer timer in timerList)
            {
                if (timer.currentTime <= timer.time) timer.currentTime += Time.deltaTime;
                else
                {
                    timer.callback();
                    timers.Remove(timer);
                }
            }
        }
    }

    void Update()
    {
        CooldownTimer.refresh();
        TimeoutTimer.refresh();
    }
}
