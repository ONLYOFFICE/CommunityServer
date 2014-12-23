/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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