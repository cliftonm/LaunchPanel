using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LaunchPanel
{
    // https://www.pinvoke.net/default.aspx/user32.enumwindows
    public class WndSearcher
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);
        private static List<IntPtr> handles = new List<IntPtr>();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref SearchData data);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static IntPtr SearchForWindow(string title)
        {
            SearchData sd = new SearchData { Title = title };
            EnumWindows(new EnumWindowsProc(EnumProc), ref sd);
            return sd.hWnd;
        }

        public static List<IntPtr> GetWindowHandles()
        {
            handles.Clear();
            SearchData sd = new SearchData();
            EnumWindows(new EnumWindowsProc(EnumHandles), ref sd);

            return new List<IntPtr>(handles);           // clone.
        }

        public static bool EnumProc(IntPtr hWnd, ref SearchData data)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetWindowText(hWnd, sb, sb.Capacity);

            if (sb.ToString().Contains(data.Title))
            {
                data.hWnd = hWnd;
                return false;    // Found the wnd, halt enumeration
            }

            return true;
        }

        public static bool EnumHandles(IntPtr hWnd, ref SearchData data)
        {
            handles.Add(hWnd);
            return true;
        }

        public class SearchData
        {
            // You can put any dicks or Doms in here...
            public string Title;
            public IntPtr hWnd;
        }
    }
}