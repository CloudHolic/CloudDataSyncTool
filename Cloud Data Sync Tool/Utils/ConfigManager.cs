using System.Collections.Generic;
using System.IO;
using CloudSync.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CloudSync.Utils
{
    public class ConfigManager
    {
        public class Settings
        {
            [YamlMember(typeof(double))]
            public double WindowLeft { get; set; }

            [YamlMember(typeof(double))]
            public double WindowTop { get; set; }

            [YamlMember(typeof(double))]
            public double WindowHeight { get; set; }

            [YamlMember(typeof(double))]
            public double WindowWidth { get; set; }

            [YamlMember(typeof(int))]
            public int MaxRows { get; set; }
            
            [YamlMember(typeof(Dictionary<string, DbSetting>))]
            public Dictionary<string, DbSetting> Connections { get; set; }

            public Settings()
            {
                WindowLeft = 0;
                WindowTop = 0;
                WindowHeight = 500;
                WindowWidth = 800;
                MaxRows = 500000;
                Connections = new Dictionary<string, DbSetting>();
            }
        }

        private readonly ISerializer _serialBuilder;
        private readonly IDeserializer _deserialBuilder;

        private static volatile ConfigManager _instance;
        private static readonly object Lock = new object();

        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                            _instance = new ConfigManager();
                    }
                }

                return _instance;
            }
        }
        
        public Settings Config { get; set; }
        
        private ConfigManager()
        {
            _serialBuilder = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            _deserialBuilder = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            Config = new Settings();

            if (!File.Exists("config.yaml"))
                Save();

            Load();
        }

        public void Save()
        {
            File.WriteAllText("config.yaml", _serialBuilder.Serialize(Config));
        }

        public void Load()
        {
            Config = _deserialBuilder.Deserialize<Settings>(File.ReadAllText("config.yaml"));
            if (Config.Connections == null)
                Config.Connections = new Dictionary<string, DbSetting>();
        }
    }
}
