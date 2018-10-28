using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public static class SpecTime
    {
        public static double Time = 0;
        static SortedList<double, TaskCompletionSource<float>> timers = new SortedList<double, TaskCompletionSource<float>>();
        public static Task<float> Wait(float dt)
        {
            var tcs = new TaskCompletionSource<float>();
            timers.Add(Time + dt, tcs);
            return tcs.Task;
        }
        public static void Update(float dt)
        {
            Time += dt;
            while (timers.Any() && timers.First().Key <= Time)
            {
                timers.First().Value.SetResult(dt);
                timers.RemoveAt(0);
            }
        }
    }
}
