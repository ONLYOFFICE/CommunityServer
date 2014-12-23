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

#region Usings

using System;
using System.Web;

#endregion

namespace ASC.Web.Projects.Classes
{
    public static class UrlParameters
    {
        public static string ProjectsFilter
        {
            get { return HttpContext.Current.Request[UrlConstant.ProjectsFilter] ?? string.Empty; }
        }

        public static string ProjectsTag
        {
            get { return HttpContext.Current.Request[UrlConstant.ProjectsTag] ?? string.Empty; }
        }

        public static string ActionType
        {
            get { return HttpContext.Current.Request[UrlConstant.Action] ?? string.Empty; }
        }

        public static string Search
        {
            get { return HttpContext.Current.Request[UrlConstant.Search] ?? string.Empty; }
        }

        public static string EntityID
        {
            get { return HttpContext.Current.Request[UrlConstant.EntityID] ?? string.Empty; }
        }

        public static string ProjectID
        {
            get { return HttpContext.Current.Request[UrlConstant.ProjectID] ?? string.Empty; }
        }

        public static int PageNumber
        {
            get
            {
                int result;
                return int.TryParse(HttpContext.Current.Request[UrlConstant.PageNumber], out result) ? result : 1;
            }
        }

        public static Guid UserID
        {
            get
            {
                var result = HttpContext.Current.Request[UrlConstant.UserID];
                if (!string.IsNullOrEmpty(result))
                {
                    try
                    {
                        return new Guid(result);
                    }
                    catch (OverflowException) { }
                    catch (FormatException) { }
                }
                return Guid.Empty;
            }
        }

        public static String ReportType
        {
            get { return HttpContext.Current.Request[UrlConstant.ReportType] ?? string.Empty; }
        }
    }
}
