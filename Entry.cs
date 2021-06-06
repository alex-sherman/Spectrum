using Spectrum.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Entry<T> where T : SpectrumGame, new()
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            LoadHelper.SetMainAssembly<T>();
#if !DEBUG
            try
            {
#endif
            using (var game = new T())
                game.Run();
#if !DEBUG
            }
            catch (Exception e)
            {
                DebugPrinter.Print(e.ToString());
            }
#endif
        }
    }
}
