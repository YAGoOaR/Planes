using System.Collections.Generic;
using UnityEngine;

//My timers, here are timeout, interval and cooldown timers
public class Timers : MonoBehaviour
{
    public delegate void customFunc();

    //Checking the timer returns true if current time reached max time
    public class CooldownTimer
    {
        static List<CooldownTimer> timers = new List<CooldownTimer>();
        float currentTime;
        readonly float maxTime;
        readonly bool destroyAfterTimeout;

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
                if (timer.currentTime <= timer.maxTime)
                {
                    timer.currentTime += Time.deltaTime;
                }
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

    //Calls callback function after timeout
    public class Timeout
    {
        static List<Timeout> timers = new List<Timeout>();
        float time;
        float currentTime;
        customFunc callback;

        public static Timeout setTimeout(float time, customFunc callback)
        {
            Timeout timer = new Timeout();
            timer.callback = callback;
            timer.time = time;
            timers.Add(timer);
            return timer;
        }

        public static void refresh()
        {
            List<Timeout> timerList = new List<Timeout>(timers);
            foreach (Timeout timer in timerList)
            {
                if (timer.currentTime <= timer.time)
                {
                    timer.currentTime += Time.deltaTime;
                }
                else
                {
                    timer.callback();
                    timers.Remove(timer);
                }
            }
        }
    }

    //Starts calling callback function each specified time
    public class Interval
    {
        static List<Interval> timers = new List<Interval>();
        float time;
        float currentTime;
        readonly customFunc callback;

        public Interval(float time, customFunc callback)
        {
            this.callback = callback;
            this.time = time;
            timers.Add(this);
        }

        public void clear()
        {
            timers.Remove(this);
        }

        public static void refresh()
        {
            List<Interval> timerList = new List<Interval>(timers);
            foreach (Interval timer in timerList)
            {
                if (timer.currentTime <= timer.time)
                {
                    timer.currentTime += Time.deltaTime;
                }
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
        Timeout.setTimeout(time, callback);
    }

    void Update()
    {
        CooldownTimer.refresh();
        Timeout.refresh();
        Interval.refresh();
    }
}
