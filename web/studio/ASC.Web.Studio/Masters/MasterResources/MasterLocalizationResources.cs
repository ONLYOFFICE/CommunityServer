/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
            var dateTimeFormat = Thread.CurrentThread.CurrentCulture.DateTimeFormat;

            return new List<KeyValuePair<string, object>>(22)
                   {
                       RegisterResourceSet("Resource", ResourceJS.ResourceManager),
                       RegisterResourceSet("FeedResource", FeedResource.ResourceManager),
                       RegisterResourceSet("ChatResource", ChatResource.ResourceManager),
                       RegisterResourceSet("UserControlsCommonResource", UserControlsCommonResource.ResourceManager),
                       RegisterObject("DatePattern", dateTimeFormat.ShortDatePattern),
                       RegisterObject("TimePattern", dateTimeFormat.ShortTimePattern),
                       RegisterObject("DateTimePattern", dateTimeFormat.FullDateTimePattern),
                       RegisterObject("DatePatternJQ", DateTimeExtension.DateMaskForJQuery), //.Replace(" ", string.Empty) -  remove because, crash date in datepicker on czech language (bug 21954)
                       RegisterObject("DatepickerDatePattern", GetDatepikerDateFormat(dateTimeFormat.ShortDatePattern)),
                       RegisterObject("DatepickerTimePattern", GetDatepikerDateFormat(dateTimeFormat.ShortTimePattern)),
                       RegisterObject("DatepickerDateTimePattern", GetDatepikerDateFormat(dateTimeFormat.FullDateTimePattern)),
                       RegisterObject("FirstDay", (int)dateTimeFormat.FirstDayOfWeek),
                       RegisterObject("DayNames", dateTimeFormat.AbbreviatedDayNames),
                       RegisterObject("DayNamesFull", dateTimeFormat.DayNames),
                       RegisterObject("MonthNames", Thread.CurrentThread.CurrentUICulture.DateTimeFormat.AbbreviatedMonthGenitiveNames),
                       RegisterObject("MonthNamesFull", Thread.CurrentThread.CurrentUICulture.DateTimeFormat.MonthNames),
                       RegisterObject("TwoLetterISOLanguageName", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName),
                       RegisterObject("CurrentCultureName", Thread.CurrentThread.CurrentCulture.Name.ToLowerInvariant()),
                       RegisterObject("CurrentCulture", CultureInfo.CurrentCulture.Name),
                       RegisterObject("FileSizePostfix", Resource.FileSizePostfix),
                       RegisterObject("AccessRightsAccessToProduct", Resource.AccessRightsAccessToProduct),
                       RegisterObject("AccessRightsDisabledProduct", Resource.AccessRightsDisabledProduct)
                   };
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
            return new List<KeyValuePair<string, object>>(12)
                   {
                       RegisterObject("Admin", CustomNamingPeople.Substitute<Resource>("Administrator")),
                       RegisterObject("User", CustomNamingPeople.Substitute<Resource>("User")),
                       RegisterObject("Guest", CustomNamingPeople.Substitute<Resource>("Guest")),
                       RegisterObject("Department", CustomNamingPeople.Substitute<Resource>("Department")),
                       RegisterObject("ConfirmRemoveUser", CustomNamingPeople.Substitute<Resource>("ConfirmRemoveUser").HtmlEncode()),
                       RegisterObject("ConfirmRemoveDepartment", CustomNamingPeople.Substitute<Resource>("DeleteDepartmentConfirmation").HtmlEncode()),
                       RegisterObject("AddDepartmentHeader", CustomNamingPeople.Substitute<Resource>("AddDepartmentDlgTitle").HtmlEncode()),
                       RegisterObject("EditDepartmentHeader", CustomNamingPeople.Substitute<Resource>("DepEditHeader").HtmlEncode()),
                       RegisterObject("EmployeeAllDepartments", CustomNamingPeople.Substitute<Resource>("EmployeeAllDepartments").HtmlEncode()),
                       RegisterObject("AddEmployees", CustomNamingPeople.Substitute<UserControlsCommonResource>("AddEmployees").HtmlEncode()),
                       RegisterObject("AccessRightsAddUser", CustomNamingPeople.Substitute<Resources.Resource>("AccessRightsAddUser").HtmlEncode()),
                       RegisterObject("AccessRightsAddGroup", CustomNamingPeople.Substitute<Resources.Resource>("AccessRightsAddGroup").HtmlEncode())
                   };
        }
    }
}