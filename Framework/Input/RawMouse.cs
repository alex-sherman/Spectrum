using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spectrum.Framework.Input
{
    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms645578(v=vs.85).aspx
    class RawMouse
    {
        #region API structs
        public enum RawInputDeviceType : uint
        {
            RIM_TYPEMOUSE = 0,
            RIM_TYPEKEYBOARD = 1,
            RIM_TYPEHID = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public int dwFlags;
            public IntPtr hwndTarget;
        }

        public enum RawInputDeviceInfoType : uint
        {
            RIDI_DEVICENAME = 0x20000007,
            RIDI_DEVICEINFO = 0x2000000b,
            RIDI_PREPARSEDDATA = 0x20000005,
        }

        public const ushort RIDEV_INPUTSINK = 0x00000100;
        public const ushort RIDEV_PAGEONLY = 0x00000020;

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTHEADER
        {
            public RawInputDeviceType dwType;
            public int dwSize;
            public IntPtr hDevice;
            public uint wParam;
        }
        #endregion

        #region API functions
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll")]
        static extern uint GetRegisteredRawInputDevices([In, Out] RAWINPUTDEVICE[] InputdeviceList, [In, Out] ref uint puiNumDevices, [In] uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputBuffer([In, Out] RAWINPUT[] pData, [In, Out] ref uint pcbSize, [In] uint cbSizeHeader);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputData([In] IntPtr rawinput, [In] uint uiCommand, [In, Out] IntPtr pData, [In, Out] ref uint pcbSize, [In] uint cbSizeHeader);
        #endregion

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWINPUT
        {
            [FieldOffset(20)]
            public RAWMOUSE mouse;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWMOUSE
        {
            [FieldOffset(0)]
            public ushort buttonFlags;
            [FieldOffset(2)]
            public short buttonData;
            [FieldOffset(4)]
            public uint rawButtons;
            [FieldOffset(8)]
            public int lastX;
            [FieldOffset(12)]
            public int lastY;
        }

        public static bool[] buttons = new bool[16];
        public static int lastX = 0;
        public static int lastY = 0;
        public static int lastZ = 0;

        public static bool RegisterRawInputDeviceHandler()
        {
            Application.AddMessageFilter(new MouseMessageFilter());
            return true;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public class MouseMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == 0xff)
                {
                    uint dwSize = 40;

                    byte[] raw = new byte[40];
                    IntPtr rawPtr = Marshal.AllocHGlobal(raw.Length);

                    GetRawInputData(m.LParam, 0x10000003, rawPtr, ref dwSize, (uint)Marshal.SizeOf(new RAWINPUTHEADER()));

                    Marshal.Copy(rawPtr, raw, 0, 40);

                    GCHandle handle = GCHandle.Alloc(raw, GCHandleType.Pinned);
                    RAWINPUT rawData = (RAWINPUT)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(RAWINPUT));
                    handle.Free();
                    lastX += rawData.mouse.lastX;
                    lastY += rawData.mouse.lastY;
                    // 0x400 is MouseWHeel
                    if (rawData.mouse.buttonFlags == 0x0400)
                        lastZ += rawData.mouse.buttonData;
                    for (int i = 0; i < 5; i++)
                    {
                        buttons[i] |= (((1 << (i * 2)) & rawData.mouse.buttonFlags) != 0);
                        buttons[i] &= (((2 << (i * 2)) & rawData.mouse.buttonFlags) == 0);
                    }
                }
                return false;
            }
        }

        static bool inited = false;

        static RAWINPUTDEVICE[] rawInputDevicesToMonitor = new RAWINPUTDEVICE[] {
            new RAWINPUTDEVICE
            {
                dwFlags = RIDEV_INPUTSINK,
                hwndTarget = SpectrumGame.Game.Window.Handle,
                usUsage = 0x2,
                usUsagePage = 0x1
            }
        };
        public static void Update()
        {
            lastX = 0;
            lastY = 0;
            lastZ = 0;
            if (SpectrumMouse.UseRaw && Process.GetCurrentProcess().MainWindowHandle.ToInt32() != 0)
            {
                rawInputDevicesToMonitor[0].hwndTarget = SpectrumGame.Game.Window.Handle;
                if (!RegisterRawInputDevices(rawInputDevicesToMonitor, (uint)1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
                {
                    SpectrumMouse.UseRaw = false;
                    var error = Marshal.GetLastWin32Error();
                }
                if (!inited)
                {
                    inited = true;
                    RegisterRawInputDeviceHandler();
                }
            }
        }
    }
}
