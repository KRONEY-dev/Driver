using System;
using System.Collections.Generic;
using Driver.Controllers.Interfaces;
using UnityEngine;
using Zenject;

namespace Driver.Controllers
{
    public class TimerController : MonoBehaviour, ITimerController
    {
        private List<Timer> timers;
        private List<Timer> timersPendingRemoval;
        private List<Timer> timersPendingAdd;

        [Inject]
        private void Construct()
        {
            timers = new();
            timersPendingRemoval = new();
            timersPendingAdd = new();
        }

        private void OnDestroy()
        {
            timers.ForEach(timer => timer?.Execute());

            timers.Clear();
            timersPendingRemoval.Clear();
            timersPendingAdd.Clear();
        }

        private void Update()
        {
            Timer tempTimer = null;
            for (int i = timers.Count - 1; i >= 0; i--)
            {
                tempTimer = timers[i];
                if (tempTimer.CheckAndExecute())
                {
                    tempTimer.Cancel();
                    timers.RemoveAt(i);
                }
            }

            foreach (var timer in timersPendingRemoval)
            {
                timer.Cancel();
                timers.Remove(timer);
            }
            timersPendingRemoval.Clear();

            if (timersPendingAdd.Count > 0)
            {
                timers.AddRange(timersPendingAdd);
                timersPendingAdd.Clear();
            }
        }

        public void AddTimer(float duration, Action action)
        {
            timersPendingAdd.Add(new Timer(DateTime.Now.AddSeconds(duration), action));
        }

        public ITimer CreateTimer(float duration, Action action)
        {
            Timer timer = new Timer(DateTime.Now.AddSeconds(duration), action);
            timersPendingAdd.Add(timer);
            return timer;
        }

        public void RemoveTimer(ITimer timer)
        {
            if (timer != null && timer is Timer timerTemp)
            {
                timerTemp.Cancel();
                timersPendingRemoval.Add(timerTemp);
            }
        }

        public void RemoveTimers(List<ITimer> timersToRemove)
        {
            foreach (ITimer timer in timersToRemove)
            {
                RemoveTimer(timer);
            }
        }

        public interface ITimer
        {
            void SetPause(bool value);
        }

        private class Timer : ITimer
        {
            private DateTime endTime;
            private DateTime pauseTime;
            private Action action;
            private bool isPaused;

            public Timer(DateTime endTime, Action action)
            {
                this.endTime = endTime;
                this.action = action;
            }

            public void SetPause(bool value)
            {
                if (isPaused != value)
                {
                    isPaused = value;

                    if (isPaused)
                    {
                        pauseTime = DateTime.Now;
                    }
                    else
                    {
                        TimeSpan pauseDuration = DateTime.Now - pauseTime;
                        endTime += pauseDuration;
                    }
                }
            }

            public void Cancel()
            {
                action = null;
            }

            public bool CheckAndExecute()
            {
                if (!isPaused && DateTime.Now >= endTime)
                {
                    Execute();
                    return true;
                }
                return false;
            }

            public void Execute()
            {
                action?.Invoke();
                Cancel();
            }
        }
    }
}