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

    public class Timeout
    {
        public static List<Timeout> timers = new List<Timeout>();
        public float time;
        public float currentTime;
        customFunc callback;

        public Timeout(float time, customFunc callback)
        {
            this.callback = callback;
            this.time = time;
            timers.Add(this);
        }

        public static void refresh()
        {
            List<Timeout> timerList = new List<Timeout>(timers);
            foreach (Timeout timer in timerList)
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

    public class Interval
    {
        public static List<Interval> timers = new List<Interval>();
        public float time;
        public float currentTime;
        customFunc callback;

        public Interval(float time, customFunc callback)
        {
            this.callback = callback;
            this.time = time;
            timers.Add(this);
        }

        public void clear(){
            timers.Remove(this);
        }

        public static void refresh()
        {
            List<Interval> timerList = new List<Interval>(timers);
            foreach (Interval timer in timerList)
            {
                if (timer.currentTime <= timer.time) timer.currentTime += Time.deltaTime;
                else
                {
                    timer.currentTime = Time.deltaTime;
                    timer.callback();
                }
            }
        }
    }

    public static Interval interval(float time, customFunc callback)
    {
        Interval timer = new Interval(time, callback);
        return timer;
    }

    public static void timeout(float time, customFunc callback)
    {
        Timeout timer = new Timeout(time, callback);
    }

    void Update()
    {
        CooldownTimer.refresh();
        Timeout.refresh();
        Interval.refresh();
    }
}
