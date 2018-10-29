using System.Collections.Generic;
using System.Windows.Forms;

namespace LaunchPanel
{
    public class LauncherGroupBox
    {
        public GroupBox GroupBox { get; set; }
        public int RequiredWidth { get; set; }
        public List<Button> Buttons { get; set; }

        public LauncherGroupBox()
        {
            Buttons = new List<Button>();
        }
    }
}
