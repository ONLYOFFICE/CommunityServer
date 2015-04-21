/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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