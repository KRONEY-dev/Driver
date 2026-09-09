using System;
using System.Collections.Generic;

namespace Driver.Controllers.Interfaces
{
    public interface ITimerController
    {
        /// <summary>
        /// Adds a timer that triggers an action after a specified duration.
        /// </summary>
        /// <param name="duration">Duration in seconds before the action is executed.</param>
        /// <param name="action">The action to execute once the timer completes.</param>
        void AddTimer(float duration, Action action);

        /// <summary>
        /// Create a timer that triggers an action after a specified duration.
        /// </summary>
        /// <param name="duration">Duration in seconds before the action is executed.</param>
        /// <param name="action">The action to execute once the timer completes.</param>
        TimerController.ITimer CreateTimer(float duration, Action action);

        /// <summary>
        /// Removes a previously added timer, stopping it from triggering its associated action.
        /// </summary>
        /// <param name="timer">The timer to remove and stop.</param>
        void RemoveTimer(TimerController.ITimer timer);

        /// <summary>
        /// Removes the specified timers, stopping them from triggering their associated actions.
        /// </summary>
        /// <param name="timers">A list of timers to remove.</param>
        void RemoveTimers(List<TimerController.ITimer> timers);
    }
}