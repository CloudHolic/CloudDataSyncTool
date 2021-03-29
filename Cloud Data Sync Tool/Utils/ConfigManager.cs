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
            public int MainWidth { get; set; }

            public int MainHeight { get; set; }

            public int MainX { get; set; }

            public int MainY { get; set; }

            public int MaxRows { get; set; }

            public int AlertRows { get; set; }
            
            public Dictionary<string, DbSetting> Connections { get; set; }
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

            Load();
        }

        public void Save()
        {
            File.WriteAllText("config.yaml", _serialBuilder.Serialize(Config));
        }

        public void Load()
        {
            Config = _deserialBuilder.Deserialize<Settings>(File.ReadAllText("config.yaml"));
        }
    }
}
