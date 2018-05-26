using picibird.hbs.viewmodels.osk;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace picibird.hbs.wpf.services
{
    public class Osk : IOsk
    {
        public bool IsEnabled { get; set; } = true;

        public void close(bool force = false)
        {
            if ((IsEnabled || force) && IsOpened())
                Close();
        }

        public bool isOpen()
        {
            return IsOpened();
        }

        public void open(bool force = false)
        {
            if ((IsEnabled || force) && !IsOpened())
                Show();
        }

        public void toggle()
        {
            if (IsEnabled)
            {
                if (IsOpened())
                {
                    Close();
                }
                else
                {
                    Show();
                }
            }
        }

        private static void StartTabTip()
        {
            var p = Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
            int handle = 0;
            while ((handle = NativeMethods.FindWindow("IPTIP_Main_Window", "")) <= 0)
            {
                Thread.Sleep(100);
            }
        }

        private static void ShowByCom()
        {
            var type = Type.GetTypeFromCLSID(Guid.Parse("4ce576fa-83dc-4F88-951c-9d0782b4e376"));
            var instance = (ITipInvocation)Activator.CreateInstance(type);
            instance.Toggle(NativeMethods.GetDesktopWindow());
            Marshal.ReleaseComObject(instance);
        }

        private static void Show()
        {
            int handle = NativeMethods.FindWindow("IPTIP_Main_Window", "");
            if (handle > 0)
            {
                var style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);
                //Console.WriteLine( "Style {0:X8}", style );
                if (!NativeMethods.IsVisible(style))
                {
                    ShowByCom();
                }
            }
            else
            {
                StartTabTip();
                // on some devices starting TabTip don't show keyboard, on some does  ¯\_(ツ)_/¯
                if (!IsOpened())
                {
                    ShowByCom();
                }
            }
        }

        public static bool Close()
        {
            // find it
            int handle = NativeMethods.FindWindow("IPTIP_Main_Window", "");
            bool active = handle > 0;
            if (active)
            {
                // don't check style - just close
                NativeMethods.SendMessage(handle, NativeMethods.WM_SYSCOMMAND, NativeMethods.SC_CLOSE, 0);
            }
            return active;
        }

        private static bool IsOpened()
        {
            bool? isOpen1709 = GetIsOpen1709();
            bool isOpenLegacy = GetIsOpenLegacy();
            Debug.WriteLine("isOpen1709: " + isOpen1709);
            Debug.WriteLine("isOpenLegacy: " + isOpenLegacy);
            return isOpen1709 ?? isOpenLegacy;
        }

        private static bool? GetIsOpen1709()
        {
            // if there is a top-level window - the keyboard is closed
            //var wnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, WindowClass1709, WindowCaption1709);
            //if (wnd != IntPtr.Zero)
            //    return false;

            var parent = IntPtr.Zero;
            for (; ; )
            {
                parent = FindWindowEx(IntPtr.Zero, parent, WindowParentClass1709);
                if (parent == IntPtr.Zero)
                    return null; // no more windows, keyboard state is unknown

                // if it's a child of a WindowParentClass1709 window - the keyboard is open
                var wnd = FindWindowEx(parent, IntPtr.Zero, WindowClass1709, WindowCaption1709);
                if (wnd != IntPtr.Zero)
                    return true;
            }
        }

        private static bool GetIsOpenLegacy()
        {
            var wnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, WindowClass);
            if (wnd == IntPtr.Zero)
                return false;

            var style = GetWindowStyle(wnd);
            return style.HasFlag(WindowStyle.Visible)
                && !style.HasFlag(WindowStyle.Disabled);
        }

        private const string WindowClass = "IPTip_Main_Window";
        private const string WindowParentClass1709 = "ApplicationFrameWindow";
        private const string WindowClass1709 = "Windows.UI.Core.CoreWindow";
        private const string WindowCaption1709 = "Microsoft Text Input Application";

        private enum WindowStyle : uint
        {
            Disabled = 0x08000000,
            Visible = 0x10000000,
        }

        private static WindowStyle GetWindowStyle(IntPtr wnd)
        {
            return (WindowStyle)GetWindowLong(wnd, -16);
        }

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string className, string title = null);

        [DllImport("user32.dll", SetLastError = false)]
        private static extern uint GetWindowLong(IntPtr wnd, int index);

    }
}
