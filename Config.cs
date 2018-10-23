using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using Newtonsoft.Json;

namespace LaunchPanel
{
    // Using an abstract class with concrete type specification in the JSON:
    /*
    public abstract class Button
    {
        public string Name { get; set; } = String.Empty;
        public abstract bool Validate();
    }

    public class ExplorerButton : Button
    {
        public string Path { get; set; } = String.Empty;

        public override bool Validate()
        {
            return Name.IsNotEmpty() && Path.IsNotEmpty();
        }
    }
    */

    public enum LauncherType
    {
        Undefined,
        LaunchExplorer,
        LaunchBrowser,
        LaunchApp,
    }

    public class LaunchButton
    {
        // Overrides the group launcher.
        [Category("Options")]
        public LauncherType Launcher { get; set; } = LauncherType.Undefined;
        [Category("Name and Path")]
        public string Name { get; set; } = String.Empty;
        [Category("Name and Path")]
        public string Path { get; set; } = String.Empty;
        [Category("Options")]
        public string WindowCaption { get; set; } = String.Empty;
        [Category("Color")]
        public Color BackColor { get; set; } = Color.LightGray;
        [Category("Color")]
        public Color TextColor { get; set; } = Color.Black;
        [Category("Region")]
        public Point Location { get; set; } = Point.Empty;
        [Category("Region")]
        public Size Size { get; set; } = Size.Empty;

        public bool Validate()
        {
            return Name.IsNotEmpty() && Path.IsNotEmpty();
        }
    }

    public class Group
    {
        public string Name { get; set; } = String.Empty;
        public LauncherType Launcher { get; set; } = LauncherType.Undefined;
        public List<LaunchButton> Buttons { get; set; } = new List<LaunchButton>();

        public bool Validate()
        {
            return Name.IsNotEmpty() && Launcher != LauncherType.Undefined;
        }
    }

    public class Config
    {
        public List<Group> Groups { get; set; } = new List<Group>();

        public static Config Load(string json)
        {
            Config cfg = JsonConvert.DeserializeObject<Config>(json);

            return cfg;
        }
    }
}


/*
https://www.newtonsoft.com/json/help/html/SerializeTypeNameHandling.htm
With concrete types specified:
Config cfg = JsonConvert.DeserializeObject<Config>(json.Value,new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

{
  "Groups": [
    {
        "Name": "Explorer Windows",
        "Launcher": "LaunchExplorer",
        "Buttons": [
          {
            "$type": "LaunchPanel.ExplorerButton, LaunchPanel",
            "Name": "Temp Folder",
            "Path": "c:\temp"
          }
        ]
    }
  ]
}
*/
