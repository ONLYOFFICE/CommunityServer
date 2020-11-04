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
                                .Replace("dd", "00")
                                .Replace("d", "00")
                                .Replace("MM", "00")
                                .Replace("M", "00")
                                .Replace("y", "0")
                                .Replace("Y", "0")
                                .Replace("'", "");
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