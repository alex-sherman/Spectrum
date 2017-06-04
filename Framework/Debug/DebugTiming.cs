using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    class TimingInfo
    {
        public int Count;
        public double TotalTime;
        public double AvgerageTime { get { return Count == 0 ? 0 : TotalTime / Count; } }
    }
    class FrameTiming
    {
        public Dictionary<string, TimingInfo> times = new Dictionary<string, TimingInfo>();
        public void LogTime(TimingResult result)
        {
            string name = result.name;
            if (!times.ContainsKey(name))
                times[name] = new TimingInfo();
            times[name].Count += 1;
            times[name].TotalTime += result.ElapsedTime;
        }
    }
    public class DebugTiming
    {
        public static readonly DebugTiming Render = new DebugTiming("Render");
        public static readonly DebugTiming Main = new DebugTiming("Main");
        public static readonly DebugTiming Update = new DebugTiming("Update");
        public static readonly DebugTiming Content = new DebugTiming("Content", true);
        public static readonly DebugTiming Scripts = new DebugTiming("Scripts");
        public static readonly List<DebugTiming> Groups = new List<DebugTiming>() { Main, Render, Update, Content };
        List<FrameTiming> frames = new List<FrameTiming>();
        public static int MaxFrameStorage = 10;
        public string Name { get; private set; }
        public bool ShowCumulative;

        public static double Now()
        {
            // Apparently windows ticks are tenths of a millisecond
            return DateTime.UtcNow.ToFileTimeUtc() / 1e7;
        }
        public static void StartFrame()
        {
            foreach (var group in Groups)
            {
                if (group.ShowCumulative) continue;
                group.frames.Add(new FrameTiming());
                if (group.frames.Count > MaxFrameStorage)
                    group.frames.RemoveAt(0);
            }
        }

        public DebugTiming(string name, bool showCumulative = false)
        {
            Name = name;
            ShowCumulative = showCumulative;
            frames.Add(new FrameTiming());
        }
        public IEnumerable<Tuple<string, double>> FrameAverages()
        {
            return frames.SelectMany(f => f.times).GroupBy(r => r.Key)
                .Select(g => new Tuple<string, double>(g.Key,
                    g.Select(kvp => kvp.Value.TotalTime).DefaultIfEmpty(0).Average()))
                .OrderByDescending(t => t.Item2);
        }
        public IEnumerable<Tuple<string, double>> AverageTimes(double previousWindow)
        {
            return frames.SelectMany(f => f.times).GroupBy(r => r.Key)
                .Select(g => new Tuple<string, double>(g.Key,
                    g.Select(kvp => kvp.Value.AvgerageTime).DefaultIfEmpty(0).Average()))
                .OrderByDescending(t => t.Item2);
        }
        public IEnumerable<Tuple<string, double>> CumulativeTimes
        {
            get
            {
                return frames.SelectMany(f => f.times).GroupBy(r => r.Key)
              .Select(g => new Tuple<string, double>(g.Key,
                  g.Select(kvp => kvp.Value.TotalTime).DefaultIfEmpty(0).Sum()))
              .OrderByDescending(t => t.Item2);
            }
        }
        public TimingResult Time(string name)
        {
            return new TimingResult(this, name);
        }
        public void LogTime(TimingResult result)
        {
            frames.Last().LogTime(result);
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
        public double Age { get { return DebugTiming.Now() - StartTime; } }
        public TimingResult(DebugTiming group, string name)
        {
            this.group = group;
            this.name = name;
            StartTime = DebugTiming.Now();
            timer.Start();
        }
        public void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                ElapsedTime = timer.Elapsed.TotalMilliseconds;
                timer = null;
                group.LogTime(this);
            }
        }
    }
}
