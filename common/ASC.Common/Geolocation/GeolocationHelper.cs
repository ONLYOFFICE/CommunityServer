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