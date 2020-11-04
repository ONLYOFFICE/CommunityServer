/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Configuration;

namespace ASC.Mail.Autoreply
{
    public class AutoreplyServiceConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("debug")]
        public bool IsDebug
        {
            get { return (bool)this["debug"]; }
            set { this["debug"] = value; }
        }

        [ConfigurationProperty("mailFolder", DefaultValue = "..\\MailDebug")]
        public string MailFolder
        {
            get { return (string)this["mailFolder"]; }
            set { this["mailFolder"] = value; }
        }

        [ConfigurationProperty("https", DefaultValue = false)]
        public bool Https
        {
            get { return (bool)this["https"]; }
            set { this["https"] = value; }
        }

        [ConfigurationProperty("smtp")]
        public SmtpConfigurationElement SmtpConfiguration
        {
            get { return (SmtpConfigurationElement)this["smtp"]; }
            set { this["smtp"] = value; }
        }

        [ConfigurationProperty("cooldown")]
        public CooldownConfigurationElement CooldownConfiguration
        {
            get { return (CooldownConfigurationElement)this["cooldown"]; }
            set { this["cooldown"] = value; }
        }

        public static AutoreplyServiceConfiguration GetSection()
        {
            return (AutoreplyServiceConfiguration)ConfigurationManager.GetSection("autoreply");
        }
    }

    public class SmtpConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("maxTransactions", DefaultValue = 0)]
        public int MaxTransactions
        {
            get { return (int)this["maxTransactions"]; }
            set { this["maxTransactions"] = value; }
        }

        [ConfigurationProperty("maxBadCommands", DefaultValue = 0)]
        public int MaxBadCommands
        {
            get { return (int)this["maxBadCommands"]; }
            set { this["maxBadCommands"] = value; }
        }

        [ConfigurationProperty("maxMessageSize", DefaultValue = 15 * 1024 * 1024)]
        public int MaxMessageSize
        {
            get { return (int)this["maxMessageSize"]; }
            set { this["maxMessageSize"] = value; }
        }

        [ConfigurationProperty("maxRecipients", DefaultValue = 1)]
        public int MaxRecipients
        {
            get { return (int)this["maxRecipients"]; }
            set { this["maxRecipients"] = value; }
        }

        [ConfigurationProperty("maxConnectionsPerIP", DefaultValue = 0)]
        public int MaxConnectionsPerIP
        {
            get { return (int)this["maxConnectionsPerIP"]; }
            set { this["maxConnectionsPerIP"] = value; }
        }

        [ConfigurationProperty("maxConnections", DefaultValue = 0)]
        public int MaxConnections
        {
            get { return (int)this["maxConnections"]; }
            set { this["maxConnections"] = value; }
        }

        [ConfigurationProperty("port", DefaultValue = 25)]
        public int Port
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }
    }

    public class CooldownConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("length", DefaultValue = "0:10:0")]
        public TimeSpan Length
        {
            get { return (TimeSpan)this["length"]; }
            set { this["length"] = value; }
        }

        [ConfigurationProperty("allowedRequests", DefaultValue = 5)]
        public int AllowedRequests
        {
            get { return (int)this["allowedRequests"]; }
            set { this["allowedRequests"] = value; }
        }

        [ConfigurationProperty("duringInterval", DefaultValue = "0:5:0")]
        public TimeSpan DuringTimeInterval
        {
            get { return (TimeSpan)this["duringInterval"]; }
            set { this["duringInterval"] = value; }
        }
    }
}