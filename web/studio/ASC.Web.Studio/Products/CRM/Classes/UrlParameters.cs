/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


#region Import

using System;
using System.Globalization;
using System.Web;
using ASC.Core.Tenants;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.Web.CRM.Classes   
{

    public static class UrlParameters
    {
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

        public static Int32 LinkMessageId
        {
            get
            {
                int result;
                return int.TryParse(HttpContext.Current.Request[UrlConstant.LinkMessageId], out result) ? result : 0;
            }
        }

    }

}