using System.Collections.Generic;
using UnityEngine;

//My timers, here are timeout, interval and cooldown timers
public class Timers : MonoBehaviour
{
    public delegate void customFunc();
    static List<Interval> intervalTimers = new List<Interval>();
    static List<CooldownTimer> cooldownTimers = new List<CooldownTimer>();
    static List<Timeout> timeoutTimers = new List<Timeout>();

    //Checking the timer returns true if current time reached max time
    public class CooldownTimer
    {

        float currentTime;
        readonly float maxTime;
        readonly bool destroyAfterTimeout = false;

        public CooldownTimer(float max)
        {
            cooldownTimers.Add(this);
            maxTime = max;
            currentTime = max;
        }

        public CooldownTimer(float max, bool destroyAfterTimeout)
        {
            cooldownTimers.Add(this);
            maxTime = max;
            currentTime = max;
            this.destroyAfterTimeout = destroyAfterTimeout;
        }

        public static void Refresh()
        {
            foreach (CooldownTimer timer in cooldownTimers)
            {
                if (timer.currentTime <= timer.maxTime)
                {
                    timer.currentTime += Time.deltaTime;
                }
            }
        }

        public bool Check()
        {
            if (currentTime > maxTime && destroyAfterTimeout) cooldownTimers.Remove(this);
            return currentTime > maxTime;
        }

        public void Reset()
        {
            currentTime = 0;
        }
    }

    //Calls callback function after timeout
    public class Timeout
    {
        float time;
        float currentTime;
        customFunc callback;

        public static Timeout SetTimeout(float time, customFunc callback)
        {
            Timeout timer = new Timeout();
            timer.callback = callback;
            timer.time = time;
            timeoutTimers.Add(timer);
            return timer;
        }

        public static void Refresh()
        {
            List<Timeout> timerList = new List<Timeout>(timeoutTimers);
            foreach (Timeout timer in timerList)
            {
                if (timer.currentTime <= timer.time)
                {
                    timer.currentTime += Time.deltaTime;
                }
                else
                {
                    timer.callback();
                    timeoutTimers.Remove(timer);
                }
            }
        }

        public void Abort()
        {
            timeoutTimers.Remove(this);
        }

        public bool Check()
        {
            return currentTime >= time;
        }
    }

    //Starts calling callback function each specified time
    public class Interval
    {

        readonly float time;
        float currentTime;
        readonly customFunc callback;

        public Interval(float time, customFunc callback)
        {
            this.callback = callback;
            this.time = time;
            intervalTimers.Add(this);
        }

        public void Clear()
        {
            intervalTimers.Remove(this);
        }

        public static void Refresh()
        {
            List<Interval> timerList = new List<Interval>(intervalTimers);
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

    public static Interval SetInterval(float time, customFunc callback)
    {
        Interval timer = new Interval(time, callback);
        return timer;
    }

    public static Timeout Delay(float time, customFunc callback)
    {
        return Timeout.SetTimeout(time, callback);
    }

    void Update()
    {
        CooldownTimer.Refresh();
        Timeout.Refresh();
        Interval.Refresh();
    }
    private void OnDestroy()
    {
        intervalTimers.Clear();
        timeoutTimers.Clear();
        cooldownTimers.Clear();
    }
}
