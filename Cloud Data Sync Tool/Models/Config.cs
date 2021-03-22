using System.Collections.Generic;

namespace CloudSync.Models
{
    public class Config
    {
        public static int MainWidth { get; set; }

        public static int MainHeight { get; set; }

        public static int MainX { get; set; }

        public static int MainY { get; set; }

        public static Dictionary<string, Connection> Connections { get; set; }
    }
}
