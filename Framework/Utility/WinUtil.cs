using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Utility
{
    public static class WinUtil
    {
        [DllImport("shcore.dll")]
        public static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);

        public enum ProcessDPIAwareness
        {
            DPI_Unaware = 0,
            System_DPI_Aware = 1,
            Per_Monitor_DPI_Aware = 2
        }
    }
}
