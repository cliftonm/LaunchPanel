using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LaunchPanel
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        const string EXPLORER_APP = "explorer.exe";
        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;

        public Form1()
        {
            InitializeComponent();
            InitializeExplorerEvents();
        }

        protected void InitializeExplorerEvents()
        {
            btnExplorer1.Click += (_, __) => LaunchExplorer(btnExplorer1.Tag.ToString());
            btnBrowser1.Click += (_, __) => LaunchBrowser(btnBrowser1.Tag.ToString());
            btnBrowser2.Click += (_, __) => LaunchBrowser(btnBrowser2.Tag.ToString());
        }

        protected void LaunchBrowser(string url)
        {
            Process.Start(url);
        }

        protected void LaunchExplorer(string folderName)
        {
            Process p = new Process();
            p.StartInfo.FileName = EXPLORER_APP;
            p.StartInfo.Arguments = folderName;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.Start();

            bool moved = false;
            int tries = 5;

            // Attempt to move the window, which may take a couple tries.
            while (!moved && (--tries >= 0))
            {
                Thread.Sleep(100);                
                moved = MoveWindow("file:///" + folderName.Replace("\\", "/").ToLower(), 0, 0, 300, 600);
            }
        }

        protected bool MoveWindow(string url, int x, int y, int w, int h)
        {
            bool moved = false;

            // https://stackoverflow.com/questions/20845140/add-reference-shdocvw-in-c-sharp-project-using-visual-c-sharp-2010-express
            // https://stackoverflow.com/questions/22989789/moving-windows-explorer-windows-to-a-set-location
            foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows())
            {
                string filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();

                if (filename.ToLowerInvariant() == "explorer")
                {
                    if (window.LocationURL.ToLower() == url)
                    {
                        window.Left = x;
                        window.Top = y;
                        window.Width = w;
                        window.Height = h;
                        moved = true;
                        break;
                    }
                }
            }

            return moved;
        }
    }
}
