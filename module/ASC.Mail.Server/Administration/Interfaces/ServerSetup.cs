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

            public virtual Builder SetConnectionString(string connection_string_obj)
            {
                connection_string = connection_string_obj;
                return this;
            }

            public virtual Builder SetLogger(ILogger logger_obj)
            {
                logger = logger_obj;
                return this;
            }

            public virtual Builder SetServerLimits(ServerLimits limits_obj)
            {
                limits = limits_obj;
                return this;
            }

            public Builder(int id_server, int id_tenant, string id_user)
            {
                server_id = id_server;
                tenant = id_tenant;
                user = id_user;
            }

            public virtual Builder SetDnsPresets(DnsPresets dns_presets_obj)
            {
                dns_presets = dns_presets_obj;
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
