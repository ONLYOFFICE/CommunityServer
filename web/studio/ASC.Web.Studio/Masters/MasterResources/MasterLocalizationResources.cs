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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.PublicResources;
using Resources;

namespace ASC.Web.Studio.Masters.MasterResources
{
    public class MasterLocalizationResources : ClientScriptLocalization
    {
        private static string GetDatepikerDateFormat(string s)
        {
            return s
                .Replace("yyyy", "yy")
                .Replace("yy", "yy")
                .Replace("MMMM", "MM")
                .Replace("MMM", "M")
                .Replace("MM", "mm")
                .Replace("M", "mm")
                .Replace("dddd", "DD")
                .Replace("ddd", "D")
                .Replace("dd", "11")
                .Replace("d", "dd")
                .Replace("11", "dd")
                ;
        }

        protected override string BaseNamespace
        {
            get { return "ASC.Resources.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterResourceSet("Resource", ResourceJS.ResourceManager);
            yield return RegisterResourceSet("FeedResource", FeedResource.ResourceManager);
            yield return RegisterResourceSet("ChatResource", ChatResource.ResourceManager);

            var dateTimeFormat = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
            yield return RegisterObject("DatePattern", dateTimeFormat.ShortDatePattern);
            yield return RegisterObject("TimePattern", dateTimeFormat.ShortTimePattern);
            yield return RegisterObject("DateTimePattern", dateTimeFormat.FullDateTimePattern);
            yield return RegisterObject("DatePatternJQ", DateTimeExtension.DateMaskForJQuery); //.Replace(" ", string.Empty) -  remove because, crash date in datepicker on czech language (bug 21954)

            yield return RegisterObject("DatepickerDatePattern", GetDatepikerDateFormat(dateTimeFormat.ShortDatePattern));
            yield return RegisterObject("DatepickerTimePattern", GetDatepikerDateFormat(dateTimeFormat.ShortTimePattern));
            yield return RegisterObject("DatepickerDateTimePattern", GetDatepikerDateFormat(dateTimeFormat.FullDateTimePattern));

            yield return RegisterObject("FirstDay", (int)dateTimeFormat.FirstDayOfWeek);
            yield return RegisterObject("DayNames", dateTimeFormat.AbbreviatedDayNames);
            yield return RegisterObject("DayNamesFull", dateTimeFormat.DayNames);
            yield return RegisterObject("MonthNames", Thread.CurrentThread.CurrentUICulture.DateTimeFormat.AbbreviatedMonthGenitiveNames);
            yield return RegisterObject("MonthNamesFull", Thread.CurrentThread.CurrentUICulture.DateTimeFormat.MonthNames);

            yield return RegisterObject("TwoLetterISOLanguageName", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            yield return RegisterObject("CurrentCultureName", Thread.CurrentThread.CurrentCulture.Name.ToLowerInvariant());
            yield return RegisterObject("CurrentCulture", CultureInfo.CurrentCulture.Name);

            yield return RegisterObject("FileSizePostfix", Resource.FileSizePostfix);
        }
    }

    public class MasterCustomResources : ClientScriptCustom
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Resources.Master"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            yield return RegisterObject("Admin", CustomNamingPeople.Substitute<Resource>("Administrator"));
            yield return RegisterObject("User", CustomNamingPeople.Substitute<Resource>("User"));
            yield return RegisterObject("Guest", CustomNamingPeople.Substitute<Resource>("Guest"));
            yield return RegisterObject("Department", CustomNamingPeople.Substitute<Resource>("Department"));
            yield return RegisterObject("ConfirmRemoveUser", CustomNamingPeople.Substitute<Resource>("ConfirmRemoveUser").HtmlEncode());
            yield return RegisterObject("ConfirmRemoveDepartment", CustomNamingPeople.Substitute<Resource>("DeleteDepartmentConfirmation").HtmlEncode());
            yield return RegisterObject("AddDepartmentHeader", CustomNamingPeople.Substitute<Resource>("AddDepartmentDlgTitle").HtmlEncode());
            yield return RegisterObject("EditDepartmentHeader", CustomNamingPeople.Substitute<Resource>("DepEditHeader").HtmlEncode());
            yield return RegisterObject("EmployeeAllDepartments", CustomNamingPeople.Substitute<Resource>("EmployeeAllDepartments").HtmlEncode());
            yield return RegisterObject("AddEmployees", CustomNamingPeople.Substitute<UserControlsCommonResource>("AddEmployees").HtmlEncode());
        }
    }
}