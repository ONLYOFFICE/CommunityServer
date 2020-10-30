using System.Collections.Concurrent;
using System.Collections.Specialized;

namespace System.Configuration
{
    //mono:hack
    public static class ConfigurationManagerExtension
    {
        private static ConcurrentDictionary<string, object> Sections { get; set; }

        static ConfigurationManagerExtension()
        {
            Sections = new ConcurrentDictionary<string, object>();
        }

        public static NameValueCollection AppSettings
        {
            get
            {
                var section = GetSection("appSettings");
                if (section == null || !(section is NameValueCollection))
                {
                    throw new ConfigurationErrorsException("config exception");
                }

                return (NameValueCollection)section;
            }
        }

        public static object GetSection(string sectionName)
        {
            return Sections.GetOrAdd(sectionName, ConfigurationManager.GetSection(sectionName));
        }

        public static ConnectionStringSettingsCollection ConnectionStrings
        {
            get
            {
                var section = GetSection("connectionStrings");
                if (section == null || section.GetType() != typeof(ConnectionStringsSection))
                {
                    throw new ConfigurationErrorsException("Config_connectionstrings_declaration_invalid");
                }

                var connectionStringsSection = (ConnectionStringsSection)section;
                return connectionStringsSection.ConnectionStrings;
            }
        }
    }
}
