using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public class TimingInfo
    {
        public double Count;
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
    static class TimingExtension
    {
        public static TimingInfo Average(this IEnumerable<TimingInfo> times, int frameCount)
        {
            var output = new TimingInfo();
            foreach (var time in times)
            {
                output.Count += time.Count / frameCount;
                output.TotalTime += time.TotalTime / frameCount;
            }
            return output;
        }
    }
    public class DebugTiming
    {
        public static bool Enabled = false;
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
        public IEnumerable<Tuple<string, TimingInfo>> FrameInfo()
        {
            return frames.SelectMany(f => f.times).GroupBy(r => r.Key)
                .Select(g => new Tuple<string, TimingInfo>(g.Key,
                    g.Select(kvp => kvp.Value).DefaultIfEmpty().Average(frames.Count)))
                .OrderByDescending(t => t.Item2.TotalTime);
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
        public List<TimingResult> pool = new List<TimingResult>();
        public TimingResult Time(string name)
        {
            if (!Enabled)
                return null;
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    var timer = pool.Pop();
                    timer.name = name;
                    timer.group = this;
                    return timer.Start();
                }
            }
            return new TimingResult(this, name);
        }
        public void LogTime(TimingResult result)
        {
            frames.Last().LogTime(result);
            lock (pool)
            {
                pool.Add(result);
            }
        }
    }

    public class TimingResult : IDisposable
    {
        private static Dictionary<string, DebugTiming> timings = new Dictionary<string, DebugTiming>();
        Stopwatch timer = new Stopwatch();
        public DebugTiming group;
        public string name;
        public double StartTime { get; private set; }
        public double ElapsedTime { get; private set; }
        public double Age { get { return DebugTiming.Now() - StartTime; } }
        public TimingResult(DebugTiming group, string name)
        {
            this.group = group;
            this.name = name;
            StartTime = DebugTiming.Now();
            timer.Restart();
        }
        public TimingResult Start()
        {
            StartTime = DebugTiming.Now();
            timer.Restart();
            return this;
        }
        public void Stop()
        {
            timer.Stop();
            ElapsedTime = timer.Elapsed.TotalMilliseconds;
            group.LogTime(this);
        }
        public void Dispose() => Stop();
    }
}
