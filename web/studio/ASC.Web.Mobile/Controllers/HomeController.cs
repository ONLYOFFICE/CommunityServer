/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Configuration;
using System.Web.Mvc;
using ASC.Web.Mobile.Attributes;
using ASC.Web.Mobile.Models;

namespace ASC.Web.Mobile.Controllers
{
    [HandleError]
    [AscAuthorization]
    public class HomeController : Controller
    {
        private String EscapeJsString(String s)
        {
            return s.Replace("'", "\\'");
        }

        private string GetMonthNames()
        {
            return String.Join(",", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.MonthNames);
        }

        private string GetShortMonthNames()
        {
            return String.Join(",", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames);
        }

        private string GetDayNames()
        {
            return String.Join(",", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.DayNames);
        }

        private string GetShortDayNames()
        {
            return String.Join(",", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.AbbreviatedDayNames);
        }

        private string GetDateTimePattern()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FullDateTimePattern;
        }

        private string GetTimePattern()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern;
        }

        private string GetDatePattern()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
        }

        public ActionResult Index()
        {
            var home = new HomeModel();

            home.MonthNames = EscapeJsString(GetMonthNames());
            home.ShortMonthNames = EscapeJsString(GetShortMonthNames());
            home.DayNames = EscapeJsString(GetDayNames());
            home.ShortDayNames = EscapeJsString(GetShortDayNames());
            home.DateTimePattern = EscapeJsString(GetDateTimePattern());
            home.TimePattern = EscapeJsString(GetTimePattern());
            home.DatePattern = EscapeJsString(GetDatePattern());

            //HACK: this is very bad >:( Kill it with fire later!
            var apiurl = Url.Content(ConfigurationManager.AppSettings["api.url"] ?? "/api/2.0/");
            apiurl = apiurl.Replace("mobile/", "");
            home.ApiBaseUrl = apiurl;

            home.PreviewedImageExtensions = ConfigurationManager.AppSettings["files.viewed-images"];
            home.PreviewedDocExtensions = ConfigurationManager.AppSettings["files.docservice.viewed-docs"];
            return View(home);
        }        
    }
}
