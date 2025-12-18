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
    private List<Task> tasks = [];

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

    public void Add(Action action, TimeSpan delay)
    {
        tasks.Add(new Task(action, delay));
    }

    public static void Delay(Action action, TimeSpan delay)
    {
        Core.Scheduler.Add(action, delay);
    }

    public class Task(Action action, TimeSpan delay) : IUpdating
    {
        private Action _action = action;
        private TimeSpan delay = delay;
        private TimeSpan elapsed = TimeSpan.Zero;

        public void Update(GameTime deltaTime)
        {
            elapsed += deltaTime.ElapsedGameTime;
            if (IsCompleted)
            {
                _action();
            }
        }
    
        public bool IsCompleted => elapsed >= delay;
    }
}
