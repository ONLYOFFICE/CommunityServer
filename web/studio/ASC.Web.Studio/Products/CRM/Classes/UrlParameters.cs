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