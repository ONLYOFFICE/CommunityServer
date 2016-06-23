/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Mail.Aggregator.Common.Logging;

namespace ASC.Mail.Server.Administration.Interfaces
{
    public class ServerSetup
    {
        public int ServerId { get; private set; }
        public int Tenant { get; private set; }
        public string User { get; private set; }
        public string ConnectionString { get; private set; }
        public ILogger Logger { get; private set; }
        public ServerLimits Limits { get; private set; }
        public DnsPresets DnsPresets { get; private set; }

        public class Builder
        {
            internal int server_id;
            internal int tenant;
            internal string user;
            internal string connection_string;
            internal ILogger logger;
            internal ServerLimits limits;
            internal DnsPresets dns_presets;

            public virtual Builder SetConnectionString(string connectionStringObj)
            {
                connection_string = connectionStringObj;
                return this;
            }

            public virtual Builder SetLogger(ILogger loggerObj)
            {
                logger = loggerObj;
                return this;
            }

            public virtual Builder SetServerLimits(ServerLimits limitsObj)
            {
                limits = limitsObj;
                return this;
            }

            public Builder(int idServer, int tenant, string user)
            {
                server_id = idServer;
                this.tenant = tenant;
                this.user = user;
            }

            public virtual Builder SetDnsPresets(DnsPresets dnsPresetsObj)
            {
                dns_presets = dnsPresetsObj;
                return this;
            }

            public ServerSetup Build()
            {
                return new ServerSetup(this);
            }
        }

        private ServerSetup(Builder builder)
        {
            if (builder.server_id < 0)
                throw new ArgumentException("Invalid server id", "builder");

            if (builder.tenant < 0)
                throw new ArgumentException("Invalid tenant id", "builder");

            ServerId = builder.server_id;
            Tenant = builder.tenant;
            User = builder.user;
            ConnectionString = builder.connection_string;
            Logger = builder.logger ?? LoggerFactory.GetLogger(LoggerFactory.LoggerType.Null, string.Empty);
            Limits = builder.limits;
            DnsPresets = builder.dns_presets;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            var other = (ServerSetup)obj;

            return ServerId == other.ServerId &&
                   Tenant == other.Tenant &&
                   User == other.User &&
                   ConnectionString == other.ConnectionString &&
                   Logger == other.Logger &&
                   Limits == other.Limits &&
                   DnsPresets == other.DnsPresets;
        }

        public override int GetHashCode()
        {
            return ServerId ^ Tenant ^ User.GetHashCode() ^ ConnectionString.GetHashCode() ^ Logger.GetHashCode() ^ Limits.GetHashCode() ^ DnsPresets.GetHashCode();
        }
    }
}
