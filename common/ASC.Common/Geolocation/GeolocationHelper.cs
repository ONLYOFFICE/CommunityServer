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

using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using log4net;
using System;
using System.Linq;
using System.Web;

namespace ASC.Geolocation
{
    public class GeolocationHelper
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Geo");

        private readonly string dbid;


        public GeolocationHelper(string dbid)
        {
            this.dbid = dbid;
        }


        public IPGeolocationInfo GetIPGeolocation(string ip)
        {
            try
            {
                var ipformatted = FormatIP(ip);
                using (var db = new DbManager(dbid))
                {
                    var q = new SqlQuery("dbip_location")
                        .Select("ip_start", "ip_end", "country", "city", "timezone_offset", "timezone_name")
                        .Where(Exp.Le("ip_start", ipformatted))
                        .OrderBy("ip_start", false)
                        .SetMaxResults(1);
                    return db
                        .ExecuteList(q)
                        .Select(r => new IPGeolocationInfo()
                        {
                            IPStart = Convert.ToString(r[0]),
                            IPEnd = Convert.ToString(r[1]),
                            Key = Convert.ToString(r[2]),
                            City = Convert.ToString(r[3]),
                            TimezoneOffset = Convert.ToDouble(r[4]),
                            TimezoneName = Convert.ToString(r[5])
                        })
                        .SingleOrDefault(i => ipformatted.CompareTo(i.IPEnd) <= 0) ??
                        IPGeolocationInfo.Default;
                }
            }
            catch (Exception error)
            {
                log.Error(error);
            }
            return IPGeolocationInfo.Default;
        }

        public IPGeolocationInfo GetIPGeolocationFromHttpContext()
        {
            return GetIPGeolocationFromHttpContext(HttpContext.Current);
        }

        public IPGeolocationInfo GetIPGeolocationFromHttpContext(HttpContext context)
        {
            if (context != null && context.Request != null)
            {
                var ip = context.Request.Headers["X-Forwarded-For"] ?? context.Request.ServerVariables["REMOTE_ADDR"];
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    return GetIPGeolocation(ip);
                }
            }
            return IPGeolocationInfo.Default;
        }

        private static string FormatIP(string ip)
        {
            ip = (ip ?? "").Trim();
            if (ip.Contains('.'))
            {
                //ip v4
                if (ip.Length == 15)
                {
                    return ip;
                }
                return string.Join(".", ip.Split(':')[0].Split('.').Select(s => ("00" + s).Substring(s.Length - 1)).ToArray());
            }
            else if (ip.Contains(':'))
            {
                //ip v6
                if (ip.Length == 39)
                {
                    return ip;
                }
                var index = ip.IndexOf("::");
                if (0 <= index)
                {
                    ip = ip.Insert(index + 2, new String(':', 8 - ip.Split(':').Length));
                }
                return string.Join(":", ip.Split(':').Select(s => ("0000" + s).Substring(s.Length)).ToArray());
            }
            else
            {
                throw new ArgumentException("Unknown ip " + ip);
            }
        }
    }
}