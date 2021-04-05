using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            [YamlMember(typeof(int), Alias = "mainWidth")]
            public int MainWidth { get; set; }

            [YamlMember(typeof(int), Alias = "mainHeight")]
            public int MainHeight { get; set; }

            [YamlMember(typeof(int), Alias = "mainX")]
            public int MainX { get; set; }

            [YamlMember(typeof(int), Alias = "mainY")]
            public int MainY { get; set; }

            [YamlMember(typeof(int), Alias = "maxRows")]
            public int MaxRows { get; set; }

            [YamlMember(typeof(int), Alias = "alertRows")]
            public int AlertRows { get; set; }
            
            [YamlMember(Alias = "connections")]
            public Dictionary<string, DbSetting> Connections { get; set; }

            public Settings()
            {
                MainWidth = 800;
                MainHeight = 500;
                MainX = 0;
                MainY = 0;
                MaxRows = 1000000;
                AlertRows = 1000000;
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
