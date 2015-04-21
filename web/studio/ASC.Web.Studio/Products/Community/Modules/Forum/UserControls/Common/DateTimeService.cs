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
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Forum.Common
{
    
    public class DateTimeService
    {
        public static DateTime CurrentDate()
        {
            return TenantUtil.DateTimeNow();
        }
        public static string DateTime2String(DateTime dateTime, string format)
        {
            DateTime now = CurrentDate();

            if ((now.DayOfYear - dateTime.DayOfYear) < 1 &&
                now.Year == dateTime.Year &&
                now.Month == dateTime.Month
                )
            {
                return Resources.ForumUCResource.Today + ", " + dateTime.ToShortTimeString();
            }
            if ((now.DayOfYear - dateTime.DayOfYear) < 2 &&
                now.Year == dateTime.Year &&
                now.Month == dateTime.Month
                )
            {
                return Resources.ForumUCResource.Yesterday + ", " + dateTime.ToShortTimeString();
            }
            return dateTime.ToString(format);
        }
        public static string DateTime2StringTopicStyle(DateTime dateTime)
        {
            DateTime now = CurrentDate();

            if ((now.DayOfYear - dateTime.DayOfYear) < 1 &&
                now.Year == dateTime.Year &&
                now.Month == dateTime.Month
                )
            {
                return "<span class='text-medium-describe'>" + Resources.ForumUCResource.Today + " " + dateTime.ToShortTimeString() + "</span>";
            }
            if ((now.DayOfYear - dateTime.DayOfYear) < 2 &&
                now.Year == dateTime.Year &&
                now.Month == dateTime.Month
                )
            {
                return "<span class='text-medium-describe'>" + Resources.ForumUCResource.Yesterday + " " + dateTime.ToShortTimeString() + "</span>";
            }
            return "<span class='text-medium-describe'>" + dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString() + "</span>";
        }

        public static string DateTime2StringWidgetStyle(DateTime dateTime)
        {
            return "<span class='text-medium-describe'>" + dateTime.ToShortDayMonth() + "<br/>" + dateTime.ToShortTimeString() + "</span>";
        }

        public static string DateTime2StringPostStyle(DateTime dateTime)
        {
            DateTime now = CurrentDate();

            if ((now.DayOfYear - dateTime.DayOfYear) < 1 &&
                now.Year == dateTime.Year &&
                now.Month == dateTime.Month
                )
            {
                return "<span class='text-medium-describe'>" + dateTime.ToShortTimeString() + "  " + Resources.ForumUCResource.Today + "</span>";
            }
            if ((now.DayOfYear - dateTime.DayOfYear) < 2 &&
                now.Year == dateTime.Year &&
                now.Month == dateTime.Month
                )
            {
                return "<span class='text-medium-describe'>" + dateTime.ToShortTimeString() + "  " + Resources.ForumUCResource.Yesterday + "</span>";
            }
            return "<span class='text-medium-describe'>" + dateTime.ToShortTimeString() + "  " + dateTime.ToShortDateString() + "</span>";
        }
    }    
}
