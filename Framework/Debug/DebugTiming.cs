using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public class DebugTiming
    {
        public static readonly DebugTiming Render = new DebugTiming("Render");
        public static readonly DebugTiming Main = new DebugTiming("Main");
        public static readonly DebugTiming Update = new DebugTiming("Update");
        public static readonly DebugTiming Content = new DebugTiming("Content", true);
        public static readonly DebugTiming Scripts = new DebugTiming("Scripts");
        public static readonly List<DebugTiming> Groups = new List<DebugTiming>() { Main, Render, Update, Content };
        Dictionary<string, List<TimingResult>> times = new Dictionary<string, List<TimingResult>>();
        public int MaxStorage = 100;
        public string Name { get; private set; }
        public bool ShowCumulative;
        public DebugTiming(string name, bool showCumulative = false)
        {
            Name = name;
            ShowCumulative = showCumulative;
        }
        public IEnumerable<Tuple<string, double>> AverageTimes(double previousWindow)
        {
            return times.Select(kvp => new Tuple<string, double>(kvp.Key, 
                kvp.Value.Where(result => result.Age < previousWindow)
                .Select(result => result.ElapsedTime).DefaultIfEmpty(0).Average()))
                .OrderByDescending(t => t.Item2);
        }
        public IEnumerable<Tuple<string, double>> CumulativeTimes
        {
            get { return times.Select(kvp => new Tuple<string, double>(kvp.Key, kvp.Value.Select(result => result.ElapsedTime).Sum())).OrderBy(t => t.Item2); }
        }
        public TimingResult Time(string name)
        {
            return new TimingResult(this, name);
        }
        public void LogTime(TimingResult result)
        {
            string name = result.name;
            if (!times.ContainsKey(name))
                times[name] = new List<TimingResult>();
            times[name].Add(result);
            if (times[name].Count > MaxStorage)
                times[name].RemoveAt(0);
        }
    }

    public class TimingResult
    {
        private static Dictionary<string, DebugTiming> timings = new Dictionary<string, DebugTiming>();
        Stopwatch timer = new Stopwatch();
        DebugTiming group;
        public string name;
        public double StartTime { get; private set; }
        public double ElapsedTime { get; private set; }
        public double Age { get { return DateTime.UtcNow.Second + DateTime.UtcNow.Millisecond / 1000.0 - StartTime; } }
        public TimingResult(DebugTiming group, string name)
        {
            this.group = group;
            this.name = name;
            StartTime = DateTime.UtcNow.Second + DateTime.UtcNow.Millisecond / 1000.0;
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
            ElapsedTime = timer.Elapsed.TotalMilliseconds;
            timer = null;
            group.LogTime(this);
        }
    }
}
