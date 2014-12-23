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

#region Import

using System;
using System.Globalization;
using System.Web;
using ASC.Core.Tenants;

#endregion

namespace ASC.Web.CRM.Classes   
{

    public static class UrlParameters
    {
        private static readonly string[] Formats = new[]
                                                       {
                                                           "o",
                                                           "yyyy'-'MM'-'dd'T'HH'-'mm'-'ss'.'fffK",
                                                           "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",
                                                           "yyyy-MM-ddTHH:mm:ss"
                                                       };

        public static DateTime ApiDateTimeParse(string data)
        {
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException("data");

            if (data.Length < 7) throw new ArgumentException("invalid date time format");

            DateTime dateTime;
            if (DateTime.TryParseExact(data, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dateTime))
            {
                return new DateTime(dateTime.Ticks, DateTimeKind.Unspecified);
            }
            throw new ArgumentException("invalid date time format");
        }

        public static String Filter
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Filter];
                return result ?? string.Empty;
            }
        }

        public static String Tag
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Tag];
                return result ?? string.Empty;
            }
        }

        public static Int32 Status
        {
            get
            {
                int result;
                return int.TryParse(HttpContext.Current.Request[UrlConstant.Status], out result) ? result : 0;
            }
        }

        public static String Action
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Action];
                return result ?? string.Empty;
            }
        }

        public static String Search
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Search];
                return result ?? string.Empty;
            }
        }

        public static String ID
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.ID];
                return result ?? string.Empty;
            }
        }

        public static Int32 PageNumber
        {
            get
            {
                int result;
                return int.TryParse(HttpContext.Current.Request[UrlConstant.PageNumber], out result) ? result : 1;
            }
        }

        public static int? Version
        {

            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Version];
                int version;

                if (!int.TryParse(result, out version))
                    return null;

                return version;
            }

        }

        public static String ReportType
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.ReportType];
                return result ?? string.Empty;
            }
        }

        public static Guid UserID
        {

            get
            {
                string result = HttpContext.Current.Request[UrlConstant.UserID];
                return result == null ? Guid.Empty : new Guid(result);
            }

        }

        public static String View
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.View];
                return result ?? string.Empty;
            }
        }

        public static Int32 ContactID
        {
            get
            {
                int result;
                return int.TryParse(HttpContext.Current.Request[UrlConstant.ContactID], out result) ? result : 0;
            }
        }
 
        public static String Type
        {
            get
            {
                var result = HttpContext.Current.Request[UrlConstant.Type];
                return result ?? string.Empty;
            }
        }
        public static String FullName
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.FullName];
                return result ?? string.Empty;
            }
        }
        public static String Email
        {
            get
            {
                string result = HttpContext.Current.Request[UrlConstant.Email];
                return result ?? string.Empty;
            }
        }
       
    }

}