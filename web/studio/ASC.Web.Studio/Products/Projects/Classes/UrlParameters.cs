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
using System.Web;
using ASC.Projects.Core.Domain.Reports;

namespace ASC.Web.Projects.Classes
{
    public static class UrlParameters
    {
        public static UrlAction? ActionType
        {
            get
            {
                UrlAction result;
                if (Enum.TryParse(HttpContext.Current.Request[UrlConstant.Action] ?? string.Empty, true, out result))
                {
                    return result;
                }
                return null;
            }
        }

        public static int EntityID
        {
            get
            {
                int result;
                if (int.TryParse(HttpContext.Current.Request[UrlConstant.EntityID] ?? string.Empty, out result))
                {
                    return result;
                }
                return -1;
            }
        }

        public static int ProjectID
        {
            get
            {
                int result;
                if (int.TryParse(HttpContext.Current.Request[UrlConstant.ProjectID] ?? string.Empty, out result))
                {
                    return result;
                }
                return -1;
            }
        }

        public static ReportType ReportType
        {
            get
            {
                ReportType result;
                if (Enum.TryParse(HttpContext.Current.Request[UrlConstant.ReportType] ?? string.Empty, out result))
                {
                    return result;
                }
                return ReportType.EmptyReport;
            }
        }
    }
}
