﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

// fman:
// C:\Users\Marc\AppData\Local\fman\fman.exe
// You can specify left & right folders! fman c:\temp c:\projects
// https://fman.io/buy

namespace LaunchPanel
{
    public partial class Launcher : Form
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        const string CONFIG_FILENAME = "config.json";
        const string EXPLORER_APP = "explorer.exe";
        const int RETRY_MOVE_DELAY = 1000;      // ms

        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;

        const int GROUPBOX_LEFT_MARGIN = 10;
        const int GROUPBOX_VERTICAL_MARGIN = 30;
        const int GROUPBOX_HORIZONTAL_PADDING = 10;
        const int GROUPBOX_VERTICAL_START = SCREEN_BUTTON_HEIGHT * 3 + SCREEN_BOX_HEIGHT + SCREEN_BOX_LAUNCH_BOX_SPACING;

        const int BUTTON_HEIGHT = 30;
        const int BUTTON_EXTRA_SLOP = 10;       // For handling MeasureString innacuracies that cause button to thing it needs to wrap the text.
        const int BUTTON_VERTICAL_PADDING = 5;
        const int BUTTON_HORIZONTAL_PADDING = 20;

        const int SCREEN_BUTTON_WIDTH = 30;
        const int SCREEN_BUTTON_HEIGHT = 30;
        const int SCREEN_BOX_HEIGHT = 60;
        const int SCREEN_BOX_LAUNCH_BOX_SPACING = 10;

        // Unicode chars from: https://www.mclean.net.nz/ucf/
        // Arrows.  Diagonal arrows (particularly the down errors) look awful.
        //string[] indicators = new string[]
        //{
        //    "\u2196", "\u2191", "\u2197",
        //    "\u2190", "\u2327", "\u2192",
        //    "\u2199", "\u2193", "\u2198"
        //};
        // Boxes for the corners, arrows for up,left,down,right looks a lot better!
        readonly string[] indicators = new string[]
        {
            "\u25F0", "\u2191", "\u25F3",
            "\u2190", "\u2327", "\u2192",
            "\u25F1", "\u2193", "\u25F2"
        };

        public List<(int x, int y, Button btn, Screen scr)> selectedRegions = new List<(int x, int y, Button btn, Screen scr)>();
        public List<(int x, int y, Button btn, Screen scr)> screenButtons = new List<(int x, int y, Button btn, Screen scr)>();
        public int selectedRegionIdx = 0;
        public Config config;
        public List<LauncherGroupBox> launcherGroupBoxes;

        public Launcher()
        {
            InitializeComponent();
            Shown += (_, __) => FormShown();
        }

        protected void FormShown()
        {
            // Not for those that dislike ternary expressions!
            config = File.Exists(CONFIG_FILENAME) ? LoadConfig() : new Config();
            InitializeForm();
            SetupLaunchers();
            SetupEvents();
            // Initialize size changed event AFTER the form is shown
            // so the event doesn't fire when the form loads.
            SizeChanged += (_, __) => Redraw();
        }

        protected void SetupEvents()
        {
            btnConfig.Click += (_, __) => Configure();
        }

        protected void InitializeForm()
        {
            List<(GroupBox gb, Screen scr)> screenBoxes = CreateScreenGroupBoxes();
            CreateScreenBoxes(screenBoxes);
        }

        protected Config LoadConfig()
        {
            string json = File.ReadAllText(CONFIG_FILENAME);
            Config cfg = Config.Load(json);

            return cfg;
        }

        protected void SetupLaunchers()
        {
            launcherGroupBoxes = CreateGroupBoxes();
            CreateGroupButtons(launcherGroupBoxes);
            AdjustGroupBoxDimensions(launcherGroupBoxes);
            AdjustGroupBoxButtonPositions(launcherGroupBoxes);
            launcherGroupBoxes.ForEach(gb => Controls.Add(gb.GroupBox));
        }

        protected void CreateScreenBoxes(List<(GroupBox gb, Screen scr)> screenBoxes)
        {
            screenBoxes.ForEach(sb =>
            {
                CreateScreenGroupBoxButtons(sb.gb, sb.scr);
                CreateScreenInfoLabels(sb.gb, sb.scr);
                Controls.Add(sb.gb);
            });
        }

        protected void AdjustGroupBoxDimensions(List<LauncherGroupBox> groupBoxes)
        {
            int x = GROUPBOX_LEFT_MARGIN;

            groupBoxes.ForEach(lgb =>
            {
                lgb.GroupBox.Left = x;
                lgb.GroupBox.Width = lgb.RequiredWidth + BUTTON_HORIZONTAL_PADDING;        // Margins within groupbox
                x += lgb.RequiredWidth + 30;                        // Padding between group boxes
            });
        }

        protected void AdjustGroupBoxButtonPositions(List<LauncherGroupBox> groupBoxes)
        {
            groupBoxes.ForEach(lgb =>
            {
                lgb.Buttons.ForEach(btn =>
                {
                    // Center button in groupbox.
                    btn.Left = (lgb.GroupBox.Width - btn.Width) / 2;
                });
            });
        }

        protected List<LauncherGroupBox> CreateGroupBoxes()
        {
            List<LauncherGroupBox> groupBoxes = new List<LauncherGroupBox>();

            config.Groups.ForEach(group =>
            {
                int gbHeight = 
                    group.Buttons.Count() * BUTTON_HEIGHT +
                    (group.Buttons.Count() - 1) * BUTTON_VERTICAL_PADDING + 
                    GROUPBOX_VERTICAL_MARGIN;

                GroupBox gb = new GroupBox() { Text = group.Name };
                // X and W are determined later.
                gb.Location = new Point(0, GROUPBOX_VERTICAL_MARGIN / 2 + GROUPBOX_VERTICAL_START);
                gb.Size = new Size(0, gbHeight);
                groupBoxes.Add(new LauncherGroupBox() { GroupBox = gb });
            });

            return groupBoxes;
        }

        protected void CreateGroupButtons(List<LauncherGroupBox> groupBoxes)
        {
            config.Groups.ForEachWithIndex((group, idx) =>
            {
                LauncherGroupBox lgb = groupBoxes[idx];
                GroupBox gb = lgb.GroupBox;
                int y = 20;

                Graphics gr = CreateGraphics();
                int maxButtonWidth = 100;

                if (group.Buttons.Count > 0)
                {
                    maxButtonWidth = (int)group.Buttons.Max(button => gr.MeasureString(button.Name, gb.Font).Width) + BUTTON_EXTRA_SLOP;
                }
                else
                {
                    maxButtonWidth = 100;           // Some arbitrary default.
                }

                lgb.RequiredWidth = maxButtonWidth;

                group.Buttons.ForEach(button =>
                {
                    Button btn = new Button() { Text = button.Name, BackColor = button.BackColor, ForeColor = button.TextColor };
                    btn.Location = new Point(0, y);
                    btn.Size = new Size(maxButtonWidth, BUTTON_HEIGHT);
                    gb.Controls.Add(btn);
                    lgb.Buttons.Add(btn);
                    LauncherType lt = (button.Launcher == LauncherType.Undefined) ? group.Launcher : button.Launcher;
                    group.Launcher.Match(
                        (l => l == LauncherType.LaunchBrowser,  ___ => btn.Click += (_, __) => LaunchBrowser(button)),
                        (l => l == LauncherType.LaunchExplorer, ___ => btn.Click += (_, __) => LaunchExplorer(button)),
                        (l => l == LauncherType.LaunchApp,      ___ => btn.Click += (_, __) => LaunchApplication(button))
                    );
                    y += btn.Height + BUTTON_VERTICAL_PADDING;
                });
            });
        }

        protected List<(GroupBox, Screen)> CreateScreenGroupBoxes()
        {
            // Order screens left-to right, top to bottom, but they're displayed left to right.
            var screens = Screen.AllScreens.OrderBy(s => s.Bounds.X);
            List<(GroupBox, Screen)> screenBoxes = new List<(GroupBox, Screen)>();

            screens.ForEachWithIndex((screen, idx) =>
            {
                GroupBox gb = new GroupBox();
                gb.Location = new Point(10 + (3 * SCREEN_BUTTON_WIDTH + 20) * idx, 10);
                gb.Size = new Size(3 * SCREEN_BUTTON_WIDTH + 20, 3 * SCREEN_BUTTON_HEIGHT + SCREEN_BOX_HEIGHT);
                screenBoxes.Add((gb, screen));
            });

            return screenBoxes;
        }

        protected void CreateScreenGroupBoxButtons(GroupBox gb, Screen screen)
        {
            /*
            TL - TC - TR | 
            ML - MC - MR
            LL - LC - LR
            */

            // User can select the bounds by selecting two buttons.
            // For example, if the user wants the app to be displayed in the left 1/3 of the screen,
            // they would select TL and LL.
            // If the user wants the app to be displayed in the left 1/2 of the screen,
            // they would select TL and LC.
            // Or the top left quarter: TL and LC

            // The logic is slightly more complex than that.
            // If a L or R is selected along with a C, the window edge is position in the center of C (1/2 screen width)
            // If only C's are selected, the window width is 1/3 the width of the screen, centered.
            // If only one of the edge locations is selected (TL, ML, LL, TC, TR, MR, LC, LR) the window is 1/3 of the screen.

            // Another example: selecting TL and TC would display the window in the top 1/3 of the screen, with 1/2 width of the screen.

            Size buttonSize = new Size(SCREEN_BUTTON_WIDTH, SCREEN_BUTTON_HEIGHT);

            (3, 3).ForEach((x, y) =>
            {
                Button btn = new Button();
                btn.Location = new Point(x * SCREEN_BUTTON_WIDTH + 10, y * SCREEN_BUTTON_HEIGHT + 12);
                btn.Size = buttonSize;
                btn.Text = indicators[y * 3 + x];
                btn.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
                btn.TextAlign = ContentAlignment.MiddleCenter;
                btn.Click += (_, __) => SelectRegion(btn, x, y, screen);
                gb.Controls.Add(btn);
                screenButtons.Add((x, y, btn, screen));
            });
        }

        protected void CreateScreenInfoLabels(GroupBox gb, Screen screen)
        {
            Label lblLocation = new Label()
            {
                Text = screen.WorkingArea.Location.ToCoord(),
                Location = new Point(5, SCREEN_BUTTON_HEIGHT * 3 + 15),
                Size = new Size(gb.ClientRectangle.Width - 10, 20),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            Label lblSize = new Label()
            {
                Text = screen.WorkingArea.Size.ToCoord(),
                Location = new Point(5, SCREEN_BUTTON_HEIGHT * 3 + 15 + lblLocation.Size.Height),
                Size = new Size(gb.ClientRectangle.Width - 10, 20),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            gb.Controls.Add(lblLocation);
            gb.Controls.Add(lblSize);
        }

        // Behavior is that 1 or 2 regions can be selected.  If a region is selected twice, any other region
        // is also deselected.  If a third region is selected, the first two are deselected.
        // Also, if the selected screen is different (screen spanning is not allowed), deselect all selections and
        // then select the new region on the new screen.
        protected void SelectRegion(Button btn, int x, int y, Screen screen)
        {
            // If a selected region is clicked on again, deselect all regions.
            if (selectedRegions.Any(r => r.x == x && r.y == y && r.scr == screen))
            {
                DeselectAllRegionButtons();
            }
            else
            {
                // If already 2 regions selected, deselect them both and select the new region.
                // If selecting regions on two different screens, deselect the first.
                if (selectedRegions.Count >= 2 || selectedRegions.Any(r=>r.scr != screen))
                {
                    DeselectAllRegionButtons();
                    SelectRegionButton(btn, x, y, screen);
                }
                else
                {
                    SelectRegionButton(btn, x, y, screen);
                    FillRegions(screen);
                }
            }
        }

        protected void DeselectAllRegionButtons()
        {
            selectedRegions.Where(r => r.btn.IsNotNull()).ForEach(r =>
            {
                r.btn.BackColor = Color.FromKnownColor(KnownColor.Control);
                r.btn.ForeColor = Color.Black;
                // Reset, otherwise control color isn't restored.
                r.btn.UseVisualStyleBackColor = true;
            });

            selectedRegions.Clear();
        }

        protected void SelectRegionButton(Button btn, int x, int y, Screen screen)
        {
            selectedRegions.Add((x, y, btn, screen));
            btn.BackColor = Color.DarkGreen;
            btn.ForeColor = Color.White;
        }

        protected void FillRegions(Screen screen)
        {
            // The first two regions (if there are 2) possibly define a rectangle that needs to be highlighted.
            if (selectedRegions.Count == 2)
            {
                int x1 = Math.Min(selectedRegions[0].x, selectedRegions[1].x);
                int y1 = Math.Min(selectedRegions[0].y, selectedRegions[1].y);
                int x2 = Math.Max(selectedRegions[0].x, selectedRegions[1].x);
                int y2 = Math.Max(selectedRegions[0].y, selectedRegions[1].y);

                for (int x = x1; x <= x2; x++)
                {
                    for (int y = y1; y <= y2; y++)
                    {
                        // Kludgy, do to [x1,y1] through [x2,y2] inclusive ranges and we want to avoid adding
                        // the already selected region the the selectedRegions list.
                        if (!selectedRegions.Exists(r => r.x == x && r.y == y && r.scr == screen))
                        {
                            Button btn = screenButtons.Single(b => b.x == x && b.y == y && b.scr == screen).btn;
                            SelectRegionButton(btn, x, y, screen);
                        }
                    }
                }
            }
        }

        protected void LaunchBrowser(LaunchButton button)
        {
            Process.Start(button.Path);
        }

        protected void LaunchApplication(LaunchButton button)
        {
            var handles = WndSearcher.GetWindowHandles();

            // http://csharphelper.com/blog/2016/12/set-another-applications-size-and-position-in-c/
            Process p = new Process();
            p.StartInfo.FileName = EXPLORER_APP;
            p.StartInfo.Arguments = button.Path;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.WorkingDirectory = Path.GetFullPath(button.Path);
            p.Start();
            MoveApplicationWindow(button, handles);
            DeselectAllRegionButtons();
        }

        protected void EnumerateWindows()
        {
        }

        protected void LaunchExplorer(LaunchButton button)
        {
            Process p = new Process();
            p.StartInfo.FileName = EXPLORER_APP;
            p.StartInfo.Arguments = button.Path;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.Start();
            MoveExplorerWindow(button);
            DeselectAllRegionButtons();
        }

        // Use the launch button's configuration for the window unless overridden by a screen selection -- 
        // An override is selected in one of the screens region buttons.
        protected void GetWindowRegion(LaunchButton button, out Point location, out Size size)
        {
            if (selectedRegions.Count > 0)
            {
                Screen screen = selectedRegions[0].scr;
                int x1 = selectedRegions.Min(r => r.x);
                int y1 = selectedRegions.Min(r => r.y);
                int x2 = selectedRegions.Max(r => r.x);
                int y2 = selectedRegions.Max(r => r.y);

                // There are six combinations of columns and row selection, with the rules for the width/height of the selection
                // X - - : [0, 1/3]
                // X X - : [0, 1/2]
                // X X X : [0, 1]
                // - X X : [1/2, 1]
                // - - X : [1/3, 1]
                // - X - : [1/3, 2/3]

                // Assume full screen
                int x = screen.WorkingArea.X;
                int y = screen.WorkingArea.Y;
                int w = screen.WorkingArea.Width;
                int h = screen.WorkingArea.Height;
                Compute(x1, x2, ref x, ref w);
                Compute(y1, y2, ref y, ref h);
                location = new Point(x, y);
                size = new Size(w, h);
            }
            else
            {
                location = button.Location;
                size = button.Size;
            }
        }

        // (a1, a2) is either (x1, x2) or (y1, y2)
        // a is either x1 or y1
        // s is either w or h
        protected void Compute(int a1, int a2, ref int a, ref int s)
        {
            if (a1 == 0 && a2 == 0)
            {
                s = s / 3;
            }
            else if (a1 == 0 && a2 == 1)
            {
                s = s / 2;
            }
            else if (a1 == 1 && a2 == 2)
            {
                a = a + s / 2;
                s = s / 2;
            }
            else if (a1 == 1 && a2 == 1)
            {
                a = a + s / 3;
                s = s / 3;
            }
            else if (a1 == 2)
            {
                a = a + s * 2 / 3;
                s = s / 3;
            }
        }

        protected bool MoveApplicationWindow(LaunchButton button, List<IntPtr> handles)
        {
            GetWindowRegion(button, out Point location, out Size size);
            bool moved = false;

            int tries = 30;

            while (!moved && (--tries >= 0))
            {
                IntPtr ptr = WndSearcher.SearchForWindow(button.WindowCaption);

                if (ptr != IntPtr.Zero)
                {
                    SetWindowPos(ptr, 0, location.X, location.Y, size.Width, size.Height, SWP_SHOWWINDOW);
                    moved = true;
                }
                else
                {
                    /*
                    var newHandles = WndSearcher.GetWindowHandles();
                    var newWindows = newHandles.Where(h => !handles.Any(h2 => h2 == h));

                    if (newWindows.Count() != 0)
                    {
                        newWindows.ForEach(nw =>
                        {
                            SetWindowPos(nw, 0, location.X, location.Y, size.Width, size.Height, SWP_SHOWWINDOW);
                        });

                        handles = newHandles;
                        // moved = true;
                    }
                    */

                    Thread.Sleep(RETRY_MOVE_DELAY);
                }
            }

            return moved;
        }

        protected bool MoveExplorerWindow(LaunchButton button)
        {
            GetWindowRegion(button, out Point location, out Size size);
            bool moved = false;
            int tries = 5;
            string path;

            if (button.Path.StartsWith("\\\\"))     
            {
                // network path
                path = "file:" + button.Path.Replace("\\", "/").ToLower();
            }
            else
            {
                // local path
                path = "file:///" + button.Path.Replace("\\", "/").ToLower();
            }

            while (!moved && (--tries >= 0))
            {
                // https://stackoverflow.com/questions/20845140/add-reference-shdocvw-in-c-sharp-project-using-visual-c-sharp-2010-express
                // https://stackoverflow.com/questions/22989789/moving-windows-explorer-windows-to-a-set-location
                foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows())
                {
                    string filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();

                    if (filename.ToLowerInvariant() == "explorer")
                    {
                        if (window.LocationURL.ToLower() == path)
                        {
                            window.Left = location.X;
                            window.Top = location.Y;
                            window.Width = size.Width;
                            window.Height = size.Height;
                            moved = true;
                        }
                    }
                }

                if (!moved)
                {
                    Thread.Sleep(RETRY_MOVE_DELAY);
                }
            }

            return moved;
        }                                                           

        protected void Configure()
        {
            DialogResult ret = new ConfigDlg(config, CONFIG_FILENAME).ShowDialog();

            if (ret == DialogResult.OK)
            {
                Redraw();
            }
        }

        protected void Redraw()
        {
            // Preserves screen and config buttons and other non-launcher group box controls.
            launcherGroupBoxes.ForEach(lgb => Controls.Remove(lgb.GroupBox));
            SetupLaunchers();
        }
    }
}
