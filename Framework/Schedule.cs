using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public class Scheduler
    {
        public static Scheduler Current => Context<Scheduler>.Current;
        public double Time = 0;
        SortedList<double, Action<float>> timers = new SortedList<double, Action<float>>();
        const double BestEffortBudget = 0.001;
        ConcurrentQueue<Action<float>> bestEffort = new ConcurrentQueue<Action<float>>();
        public void Wait(float scheduleAt, Action<float> done) => timers.Add(Time + scheduleAt, done);
        // TODO: This could run immediate if `canBestEffort`
        public void RunBestEffort(Action<float> done) => bestEffort.Enqueue(done);
        public void Update(float dt)
        {
            Time += dt;
            while (timers.Any() && timers.First().Key <= Time)
            {
                timers.First().Value(dt);
                timers.RemoveAt(0);
            }
            var start = DebugTiming.Now();
            while (BestEffortBudget > (DebugTiming.Now() - start) && bestEffort.TryDequeue(out var action))
                action(dt);
        }
    }
    public static class Schedule
    {
        public static void Wait(float scheduleAt, Action<float> done) => Scheduler.Current.Wait(scheduleAt, done);
        public static void RunBestEffort(Action<float> done) => Scheduler.Current.RunBestEffort(done);
    }
}
