using Microsoft.Xna.Framework;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Util;


public class Scheduler : IUpdating
{
    private readonly List<Task> tasks = [];

    public void Update(GameTime deltaTime)
    {
        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            var task = tasks[i];
            task.Update(deltaTime);
            if (task.IsCompleted)
            {
                tasks.RemoveAt(i);
            }
        }
    }

    public void Add(Action<Task> action, TimeSpan delay)
    {
        tasks.Add(new Task(action, delay));
    }

    public static void Delay(Action<Task> action, TimeSpan delay)
    {
        Core.Scheduler.Add(action, delay);
    }

    public static void Delay(Action action, TimeSpan delay)
    {
        Core.Scheduler.Add(_ => action(), delay);
    }

    public void Add(Action<RepeatTask> action, TimeSpan delay, TimeSpan repeatDelay)
    {
        var repeatTask = new RepeatTask(action, delay, repeatDelay);
        tasks.Add(repeatTask);
    }

    public static void Repeat(Action<RepeatTask> action, TimeSpan delay, TimeSpan repeatDelay)
    {
        Core.Scheduler.Add(action, delay, repeatDelay);
    }

    public class Task(Action<Task> action, TimeSpan delay) : IUpdating
    {
        private Action<Task> _action = action;
        protected TimeSpan delay = delay;
        public TimeSpan Elapsed { get; protected set; } = TimeSpan.Zero;
        public bool Cancelled { get; set; } = false;

        public virtual void Update(GameTime deltaTime)
        {
            if (Cancelled) return;

            Elapsed += deltaTime.ElapsedGameTime;
            if (IsCompleted)
            {
                _action(this);
            }
        }
    
        public bool IsCompleted => Elapsed >= delay || Cancelled;
    }

    public class RepeatTask : Task
    {
        private readonly Action<RepeatTask> _repeatAction;
        private TimeSpan repeatDelay;
        public int Runs { get; private set; } = 0;

        public RepeatTask(Action<RepeatTask> action, TimeSpan startDelay, TimeSpan repeatDelay)
            : base(null!, startDelay)
        {
            _repeatAction = action;
            this.repeatDelay = repeatDelay;
        }

        public override void Update(GameTime deltaTime)
        {
            if (Cancelled) return;

            Elapsed += deltaTime.ElapsedGameTime;
            if (IsCompleted)
            {
                _repeatAction(this);
                if (Runs == 0) delay = repeatDelay;
                Runs++;
                Elapsed = TimeSpan.Zero;
            }
        }
    }
}
