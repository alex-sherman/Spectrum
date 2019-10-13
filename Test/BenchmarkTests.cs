using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumTest
{
    public struct TimingResult
    {
        public double T;
        public double N;
        public override string ToString()
        {

            if (T >= 1)
                return $"{T} s";
            if (T >= 0.001)
                return $"{T * 1e3} ms";
            return $"{T * 1e6} us";
        }
    }
    [TestClass]
    public class BenchmarkTests
    {
        public TimingResult Time(Action del, int n = 1_000, double t = 2)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            for (int i = 0; i < n; i++)
            {
                del();
            }
            s.Stop();
            return new TimingResult() { T = s.ElapsedTicks * 1.0 / Stopwatch.Frequency / n, N = n };
        }
        [TestMethod]
        public void LargeUpdateBatch()
        {
            var manager = new EntityManager();
            for (int i = 0; i < 1e3; i++)
            {
                manager.AddEntity(new GameObject() { ID = Guid.NewGuid() });
            }
            var time = Time(() => manager.Update(0.16666f));
        }
    }
}
