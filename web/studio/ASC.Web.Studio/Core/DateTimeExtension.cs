/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Globalization;
using System.Text;
using Resources;
using ASC.Web.Core.Helpers;
using ASC.Core.Tenants;

namespace System
{
    public static class DateTimeExtension
    {
        public static string DateMaskForJQuery
        {
            get
            {
                return Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern
                                .Replace("dd", "99")
                                .Replace("d", "99")
                                .Replace("MM", "99")
                                .Replace("M", "99")
                                .Replace("y", "9")
                                .Replace("Y", "9");
            }
        }

        public static string DateFormatPattern
        {
            get { return ShortDatePattern; }
        }

        public static string GetGrammaticalCase(int number, string nominative, string genitiveSingular, string genitivePlural)
        {
            return GrammaticalHelper.ChooseNumeralCase(number, nominative, genitiveSingular, genitivePlural);
        }

        public static string Yet(this DateTime target)
        {
            return Yet(target, TenantUtil.DateTimeNow());
        }

        public static string Yet(int days)
        {
            var result = new StringBuilder();
            switch (days)
            {
                case 0:
                    result.Append(Resource.DrnToday);
                    break;
                case 1:
                    result.Append(Resource.DrnTomorrow);
                    break;
                default:
                    result.AppendFormat(GetGrammaticalCase(days, Resource.DrnAgoDaysI, Resource.DrnAgoDaysR1, Resource.DrnAgoDaysRm), days);
                    break;
            }

            return result.ToString();
        }

        public static string Yet(this DateTime target, DateTime from)
        {
            var temp = target.Date;
            temp = temp.AddYears(from.Date.Year - temp.Year);
            var diff = (temp - from.Date);

            return Yet(diff.Days);

        }

        public static string Ago(this DateTime target)
        {
            var result = new StringBuilder();
            var diff = (TenantUtil.DateTimeNow().Date - target.Date);

            result.AppendFormat("{0} ", target.ToShortTimeString());
            switch (diff.Days)
            {
                case 0:
                    result.Append(Resource.DrnToday);
                    break;
                case 1:
                    result.Append(Resource.DrnYesterday);
                    break;
                default:
                    result.AppendFormat("{0}", target.ToShortDateString());
                    break;
            }

            return result.ToString();
        }

        public static string Ago(this DateTime from, DateTime to)
        {
            var result = new StringBuilder("");
            TimeSpan diffFrom, diffTo;
            if ((from.Ticks != 0) && (to.Ticks == 0))
            {
                diffFrom = (TenantUtil.DateTimeNow().Date - from.Date);
                switch (diffFrom.Days)
                {
                    case 0:
                        result.Append(Resource.DrnToday);
                        break;
                    case 1:
                        result.Append(Resource.DrnYesterday);
                        break;
                    default:
                        result.AppendFormat("{0}", from.ToShortDateString());
                        break;
                }

                result.AppendFormat(" " + Resource.AtTime + " {0} ", from.ToShortTimeString());
            }

            if ((from.Ticks == 0) && (to.Ticks != 0))
            {
                diffTo = (TenantUtil.DateTimeNow().Date - to.Date);
                result.AppendFormat(Resource.ToTime);
                result.AppendFormat(" {0} ", to.ToShortTimeString());
                switch (diffTo.Days)
                {
                    case 0:
                        result.Append(Resource.DrnToday);
                        break;
                    case 1:
                        result.Append(Resource.DrnYesterday);
                        break;
                    default:
                        result.AppendFormat("{0}", to.ToShortDateString());
                        break;
                }

            }

            if ((from.Ticks != 0) && (to.Ticks != 0))
            {
                diffFrom = (TenantUtil.DateTimeNow().Date - from.Date);
                diffTo = (TenantUtil.DateTimeNow().Date - to.Date);

                if (diffFrom == diffTo)
                {
                    switch (diffFrom.Days)
                    {
                        case 0:
                            result.Append(Resource.DrnToday);
                            break;
                        case 1:
                            result.Append(Resource.DrnYesterday);
                            break;
                        default:
                            result.AppendFormat("{0}", from.ToShortDateString());
                            break;
                    }
                    result.AppendFormat(" " + Resource.FromTime + " {0} " + Resource.ToTime + " {1}", from.ToShortTimeString(), to.ToShortTimeString());
                }
                else
                {
                    result.AppendFormat(Resource.FromTime + " {0} ", from.ToShortTimeString());
                    switch (diffFrom.Days)
                    {
                        case 0:
                            result.Append(Resource.DrnToday);
                            break;
                        case 1:
                            result.Append(Resource.DrnYesterday);
                            break;
                        default:
                            result.AppendFormat("{0}", from.ToShortDateString());
                            break;
                    }
                    result.AppendFormat(" " + Resource.ToTime + " {0} ", to.ToShortTimeString());
                    switch (diffTo.Days)
                    {
                        case 0:
                            result.Append(Resource.DrnToday);
                            break;
                        case 1:
                            result.Append(Resource.DrnYesterday);
                            break;
                        default:
                            result.AppendFormat("{0}", to.ToShortDateString());
                            break;
                    }
                }
            }
            return result.ToString();
        }

        public static string ToShortString(this DateTime targetDateTime)
        {
            return String.Format("{0} {1}", targetDateTime.ToShortDateString(), targetDateTime.ToShortTimeString());
        }

        public static string AgoSentence(this DateTime target)
        {
            const int yearDuration = 365;
            const int monthDuration = 30;
            var isYesterdayOrDayBefore = false;

            var result = new StringBuilder();
            var diff = (TenantUtil.DateTimeNow() - target);

            var years = diff.Days/yearDuration;
            var months = diff.Days/monthDuration;

            if (years > 0)
                result.AppendFormat(GetGrammaticalCase(years, Resource.DrnAgoYearsI, Resource.DrnAgoYearsR1, Resource.DrnAgoYearsRm), years);
            else if (months > 0)
                result.AppendFormat(GetGrammaticalCase(months, Resource.DrnAgoMonthsI, Resource.DrnAgoMonthsR1, Resource.DrnAgoMonthsRm), months);
            else if (diff.Days > 0)
            {
                switch (diff.Days)
                {
                    case 1:
                        result.Append(Resource.DrnYesterday);
                        isYesterdayOrDayBefore = true;
                        break;
                        //case 2:
                        //    result.Append(Resources.Resource.DrnDayBeforeYesterday);
                        //    isYesterdayOrDayBefore = true;
                        //    break;
                    default:
                        result.AppendFormat(GetGrammaticalCase(diff.Days, Resource.DrnAgoDaysI, Resource.DrnAgoDaysR1, Resource.DrnAgoDaysRm), diff.Days);
                        break;
                }
            }
            else if (diff.Hours > 0)
                result.AppendFormat(GetGrammaticalCase(diff.Hours, Resource.DrnAgoHoursI, Resource.DrnAgoHoursR1, Resource.DrnAgoHoursRm), diff.Hours);
            else if (diff.Minutes > 0)
                result.AppendFormat(GetGrammaticalCase(diff.Minutes, Resource.DrnAgoMinutesI, Resource.DrnAgoMinutesR1, Resource.DrnAgoMinutesRm), diff.Minutes);

            if (result.Length == 0)
                return Resource.DrnAgoFewMoments;
            else if (!isYesterdayOrDayBefore)
                return String.Format(Resource.DrnAgo, result.ToString());

            return result.ToString();
        }

        public static string ShortDatePattern
        {
            get
            {
                var pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                if (pattern.LastIndexOf("MM") < 0)
                    pattern = pattern.Replace("M", "MM");
                if (pattern.LastIndexOf("dd") < 0)
                    pattern = pattern.Replace("d", "dd");

                return pattern;
            }
        }

        public static string ToShortDayMonth(this DateTime date)
        {
            return date.ToString(
                CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
                           .Replace(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator + "yyyy", "")
                           .Replace(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator + "yy", "")
                );
        }
    }
}