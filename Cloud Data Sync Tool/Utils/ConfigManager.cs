using CloudSync.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CloudSync.Utils
{
    public class ConfigManager
    {
        private readonly IDeserializer _deserialBuilder;
        private readonly ISerializer _serialBuilder;
        
        public Config Config { get; set; }

        public ConfigManager()
        {
            _deserialBuilder = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            _serialBuilder = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        }

        public void Save()
        {
            var yaml = _serialBuilder.Serialize(Config);
        }

        public void Load()
        {
            Config = _deserialBuilder.Deserialize<Config>("config.yaml");
        }
    }
}
