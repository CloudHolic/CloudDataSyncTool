using System.Collections.Generic;

namespace CloudSync.Models
{
    public class Config
    {
        public static int MainWidth { get; set; }

        public static int MainHeight { get; set; }

        public static int MainX { get; set; }

        public static int MainY { get; set; }

        public static int MaxRows { get; set; }

        public static int AlertRows { get; set; }

        public static Dictionary<string, DbSetting> Connections { get; set; }
    }
}
