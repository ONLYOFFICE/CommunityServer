using System.Configuration;

namespace ASC.Socket.IO.Svc
{
    class SocketIOCfgSectionHandler : ConfigurationSection
    {
        [ConfigurationProperty("path", DefaultValue = "../ASC.Socket.IO")]
        public string Path
        {
            get { return (string)base["path"]; }
        }

        [ConfigurationProperty("port", DefaultValue = "9899")]
        public string Port
        {
            get { return (string)base["port"]; }
        }

        [ConfigurationProperty("redis")]
        public RedisCfgElement Redis
        {
            get { return (RedisCfgElement)base["redis"]; }
        }
    }

    public class RedisCfgElement : ConfigurationElement
    {
        [ConfigurationProperty("host")]
        public string Host
        {
            get { return (string)base["host"]; }
        }

        [ConfigurationProperty("port")]
        public string Port
        {
            get { return (string)base["port"]; }
        }
    }
}
