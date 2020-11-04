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
